using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using Common.DTO;

using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Service
{
    public class NaiveBase : INaiveBase
    {
        private readonly IRepository<Category> _Categoryrepo;
        private readonly IRepository<Request> _Requestrepo;
        private readonly IRepository<Word> _wordRepo;
        private readonly ICategoryWordRepository _categoryWordRepo;
        private readonly ISimiliarWord _similarWordsService;    

        public Dictionary<string, WordClassificationDTO> WordStatistics { get; private set; } = new Dictionary<string, WordClassificationDTO>();
        private int[] _totalWordsPerCategory;

        private Dictionary<int, int> _categoryIdToIndex = new Dictionary<int, int>(); // מ-ID לאינדקס (0,1,2...)
        private Dictionary<int, int> _indexToCategoryId = new Dictionary<int, int>(); // מאינדקס (0,1,2...) חזרה ל-ID
                                                                                      // מילון ששומר את חישובי הממוצע שכבר עשינו למילים שלא היו ב-DB
        private Dictionary<string, int[]> _similarWordsScoresCache = new Dictionary<string, int[]>(); //זה בשביל המילים הדומות שלא ישלח כמה פעמים לבדיקה
        private int _vocabularySize;

        private int _numCategories;

        private double[] _categoryLogPriors;




        public NaiveBase(IRepository<Category> repositoryy, IRepository<Request> reqrepository, IRepository<Word> wordRepo, ICategoryWordRepository categoryWordRepo ,ISimiliarWord similiarWord)
        {
            _Categoryrepo = repositoryy;
            _Requestrepo = reqrepository;
            _wordRepo = wordRepo;
            _categoryWordRepo = categoryWordRepo;
            _similarWordsService = similiarWord;
        }

         public async Task LoadDictionaryAsync(List<Category> categories)
        {
            // 1. נשלוף את כל טבלת הקישור מה-DB
            var allCategoryWords = await _categoryWordRepo.GetAll();

            // 2. אתחול הדיקשנרי והמערך
            WordStatistics = new Dictionary<string, WordClassificationDTO>();
            _totalWordsPerCategory = new int[_numCategories];

            foreach (var cw in allCategoryWords)
            {
                string text = cw.Word.Text;

                if (!_categoryIdToIndex.TryGetValue(cw.CategoryId, out int catIdx))
                {
                    continue; // אם ה-ID לא קיים במילון המיפוי, נדלג
                }

                // אם המילה לא קיימת – צור DTO חדש
                if (!WordStatistics.ContainsKey(text))
                {
                    WordStatistics[text] = new WordClassificationDTO(_numCategories)
                    {
                        Word = text,
                        WordId = cw.WordId
                    };
                }

                // מלא ספירות
                WordStatistics[text].CategoryCounts[catIdx] += cw.Frequency;
                _totalWordsPerCategory[catIdx] += cw.Frequency;
            }

            _vocabularySize = WordStatistics.Count;
        }
        //זה בשביל ההאינדקסים של הקטגוריות
        public int GetIndex(int categoryId)
        {
            if (_categoryIdToIndex.TryGetValue(categoryId, out int index))
                return index;
            // return -1 when categoryId is not mapped to an index
            return -1;
        }
        public async Task LoadModel()
        {
            var categories = await _Categoryrepo.GetAll();
            _numCategories = categories.Count;

            // ניקוי המילונים לפני טעינה מחדש
            _categoryIdToIndex.Clear();
            _indexToCategoryId.Clear();

            // 1. מילוי מילוני המיפוי (דו-כיווני)
            for (int i = 0; i < categories.Count; i++)
            {
                int currentId = categories[i].CategoryId;

                _categoryIdToIndex[currentId] = i;      // בשביל LoadDictionaryAsync ו-AddNewWord
                _indexToCategoryId[i] = currentId;      // בשביל PredictCategory (התשובה הסופית)
            }

            var allRequests = await _Requestrepo.GetAll();
            int totalAllRequests = allRequests.Count;

            // 2. ספירת בקשות לכל קטגוריה
            var _categoryRequestsCounts = new int[_numCategories];
            for (int i = 0; i < _numCategories; i++)
            {
                // משתמשים ב-ID המקורי מהרשימה כדי לספור
                int catId = categories[i].CategoryId;
                _categoryRequestsCounts[i] = allRequests.Count(r => r.CategoryId == catId);
            }

            // 3. חישוב Priors
            _categoryLogPriors = new double[_numCategories];
            for (int i = 0; i < _numCategories; i++)
            {
                double pCat;
                if (totalAllRequests == 0)
                {
                    pCat = 1.0 / _numCategories;
                }
                else
                {
                    pCat = (double)(_categoryRequestsCounts[i] + 1) / (totalAllRequests + _numCategories);
                }
                _categoryLogPriors[i] = Math.Log(pCat);
            }

            // טעינת המילון הסטטיסטי
            await LoadDictionaryAsync(categories);
        }

     
        public async Task<PredictionResultDTO> PredictCategory(List<string> words)
        {
            var result = new PredictionResultDTO();//זה העצן שנחזיר עם הקטגוריה המנחשת והרשימה של מילים ללמידה  

            Console.WriteLine("\n***** STARTING PREDICTION *****");

            double[] finalScores = new double[_numCategories];
            Array.Copy(_categoryLogPriors, finalScores, _numCategories);

            // ניקוי מילים
            var tokens = words?.Select(w => w.ToLower().Trim()).Where(w => !string.IsNullOrWhiteSpace(w)).ToList() ?? new();
            Console.WriteLine($"Input Tokens: {string.Join(", ", tokens)}");

            foreach (var word in tokens)
            {
                int[] countsForWord = null;
                Console.WriteLine($"\n--- Analyzing: '{word}' ---");

                // 1. חיפוש במילון
                if (WordStatistics.TryGetValue(word, out WordClassificationDTO stats))
                {
                    countsForWord = stats.CategoryCounts;
                    Console.WriteLine($"[DB] Found word '{word}'. Counts: {string.Join(", ", countsForWord)}");
                }
                else
                {
                    // 2. מילים דומות
                    countsForWord = await GetSynonymCounts(word);
                }

                if (countsForWord != null)
                {
                    for (int i = 0; i < _numCategories; i++)
                    {
                        // חישוב הסתברות המילה בקטגוריה (P(Word|Category))
                        double numerator = countsForWord[i] + 0.5; // alpha = 0.5
                        double denominator = _totalWordsPerCategory[i] + (0.5 * _vocabularySize);
                        double pWordGivenCat = numerator / denominator;

                        finalScores[i] += Math.Log(pWordGivenCat);
                        Console.WriteLine($"   -> Cat Index {i} Score updated to: {finalScores[i]:F5}");
                    }
                }
                else
                {
                    Console.WriteLine($"[Skip] No data found for '{word}' or its synonyms.");
                }
            }




            // בחירת המנצח
            int bestIndex = 0;
            for (int i = 1; i < finalScores.Length; i++)
            {
                if (finalScores[i] > finalScores[bestIndex]) bestIndex = i;
            }

            int winnerId = _categoryIdToIndex.FirstOrDefault(x => x.Value == bestIndex).Key;
            Console.WriteLine($"\n********************************");
            Console.WriteLine($"WINNER: Category ID {winnerId} (Index {bestIndex})");
            Console.WriteLine($"********************************\n");


            result.WordsToLearn = tokens;
            result.CategoryId = winnerId;   

            return result;
        }




        private async Task<int[]> GetSynonymCounts(string word)
        {
            // 1. קודם כל בודקים ב"זיכרון המהיר" (ה-Cache) שהוספת למעלה
            if (_similarWordsScoresCache.TryGetValue(word, out var cached))
            {
                return cached;
            }

            // 2. פונים לסרביס של המילים הדומות כדי לקבל רשימת מילים (כמו "ריח", "ארומה")
            var similarWordsList = await _similarWordsService.GetSimilarWordsAsync(word);

            // 3. שולחים את הרשימה לפונקציה שלך שעושה ממוצע
            int[] averageCounts = GetAverageCountsForSimilarWords(similarWordsList?.ToList());

            // 4. שומרים ב-Cache כדי שבפעם הבאה שמישהו יכתוב "ניחוח" זה יהיה מיידי
            if (averageCounts != null)
            {
                _similarWordsScoresCache[word] = averageCounts;
            }

            return averageCounts;
        }

        public int[] GetAverageCountsForSimilarWords(List<string> similarWords)
        {
            // 1. בדיקה אם הרשימה ריקה או לא קיימת
            if (similarWords == null || !similarWords.Any())
            {
                return null;
            }

            // 2. הכנת המערך הצובר (לפי כמות הקטגוריות שלך)
            int[] sumCounts = new int[_numCategories];
            int matchCount = 0;

            // 3. ריצה על רשימת המילים שקיבלנו כפרמטר
            foreach (var simWord in similarWords)
            {
                if (string.IsNullOrWhiteSpace(simWord)) continue;

                // תיקון קריטי: ניקוי המילה לפני החיפוש במילון (Case-insensitive)
                string cleanedSimWord = simWord.ToLower().Trim();

                // בדיקה אם המילה הדומה קיימת במילון הסטטיסטיקות שלנו
                if (WordStatistics.TryGetValue(cleanedSimWord, out var stats))
                {
                    for (int i = 0; i < _numCategories; i++)
                    {
                        sumCounts[i] += stats.CategoryCounts[i];
                    }
                    matchCount++;
                }
            }

            // 4. חישוב ממוצע (רק אם מצאנו לפחות מילה אחת ב-DB)
            if (matchCount > 0)
            {
                for (int i = 0; i < _numCategories; i++)
                {
                    // חילוק המצטבר במספר המילים שמצאנו כדי לקבל "פרופיל ממוצע"
                    sumCounts[i] /= matchCount;
                }
                return sumCounts;
            }

            // אם אף אחת מהמילים הדומות לא נמצאה במילון שלנו
            return null;
        }


        public void AddNewWordToDictinary(string wordText, int categoryId, int wordId)
        {
            // 1. קבלת האינדקס הבטוח באמצעות הפונקציה שיצרת
            int catIdx = GetIndex(categoryId);

            // בדיקת בטיחות: אם משום מה הקטגוריה לא קיימת במילון, לא נמשיך כדי למנוע קריסה
            if (catIdx == -1)
            {
                // אפשר להוסיף כאן לוג או שגיאה, כרגע פשוט נצא כדי לא לשבור את התוכנית
                return;
            }

            var newDto = new WordClassificationDTO(_numCategories)
            {
                Word = wordText,
                WordId = wordId
            };

            // 2. עדכון המערך ב-DTO: משתמשים באינדקס הממופה במקום ב-ID פחות 1
            newDto.CategoryCounts[catIdx] = 1;

            // 3. הוספה לדיקשנרי הסטטיסטיקות
            WordStatistics.Add(wordText, newDto);

            // 4. עדכון משתני העזר של האלגוריתם
            _vocabularySize++;

            _totalWordsPerCategory[catIdx]++;

           
        }
    }


    }
 