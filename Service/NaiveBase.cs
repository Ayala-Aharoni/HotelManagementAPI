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

        private Dictionary<int, int> _categoryIdToIndex = new Dictionary<int, int>();//זה בשביל מיפוי האינדסים 

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
            _numCategories = categories.Count();

            // 1. מילוי מילון המיפוי (ID -> Index)
            _categoryIdToIndex.Clear();
            for (int i = 0; i < categories.Count; i++)
            {
                _categoryIdToIndex[categories[i].CategoryId] = i;
            }

            var allRequests = await _Requestrepo.GetAll();
            int totalAllRequests = allRequests.Count();

            // 2. ספירת בקשות לכל קטגוריה
            var _categoryRequestsCounts = new int[_numCategories];
            for (int i = 0; i < _numCategories; i++)
            {
                _categoryRequestsCounts[i] = allRequests.Count(r => r.CategoryId == categories[i].CategoryId);
            }

            // 3. חישוב הסתברויות קטגוריה (Priors) עם מניעת אפס
            _categoryLogPriors = new double[_numCategories];
            for (int i = 0; i < _numCategories; i++)
            {
                double pCat;
                if (totalAllRequests == 0)
                {
                    // אם אין בקשות בכלל - הסתברות שווה לכולן (1/כמות הקטגוריות)
                    pCat = 1.0 / _numCategories;
                }
                else
                {
                    /* התיקון הקריטי: הוספת +1 למונה ו +_numCategories למכנה.
                       זה מבטיח שאפילו אם לקטגוריה יש 0 בקשות, הציון שלה לא יהיה 0
                       ולא נקבל Log(0) ששווה למינוס אינסוף.
                    */
                    pCat = (double)(_categoryRequestsCounts[i] + 1) / (totalAllRequests + _numCategories);
                }

                _categoryLogPriors[i] = Math.Log(pCat);

                // הדפסת בדיקה לטרמינל כדי לוודא שאין Infinity
                Console.WriteLine($"Category ID {categories[i].CategoryId} (Index {i}) Initial LogPrior: {_categoryLogPriors[i]:F5}");
            }

            await LoadDictionaryAsync(categories);
        }
        public async Task<int> PredictCategory(List<string> words)
        {
            double[] finalScores = new double[_numCategories];
            int totalWords = _totalWordsPerCategory.Sum();

            Array.Copy(_categoryLogPriors, finalScores, _numCategories);
            //for (int i = 0; i < _numCategories; i++)
            //{
            //    double pCategory = (double)_totalWordsPerCategory[i] / totalWords;
            //    finalScores[i] = Math.Log(pCategory); // מתחילים מהלוגריתם של הסתברות הקטגוריה
            //}

            // --- שלב החישוב ---
            foreach (var word in words)
            {
                Console.WriteLine($"\n--- Analyzing word: '{word}' ---");

                // שליפת הסטטיסטיקה למילה
                int[] countsForWord = null;
                if (WordStatistics.TryGetValue(word, out WordClassificationDTO stats))
                {
                    countsForWord = stats.CategoryCounts;
                    Console.WriteLine($"Word Counts in DB: {string.Join(", ", countsForWord)}");
                }

                else
                {
                    // 2. המילה לא קיימת - הולכים למילים נרדפות
                    var synonyms = await _similarWordsService.GetSimilarWordsAsync(word);
                    if (synonyms != null && synonyms.Any())
                    {
                        // פה נכנסת הפונקציה שלך
                        countsForWord = GetAverageCountsForSimilarWords(synonyms);
                    }
                }

                // --- התיקון הזמני (והחכם): אם לא מצאנו מידע סטטיסטי - פשוט מדלגים ---
                if (countsForWord == null)
                {
                    Console.WriteLine($"[Skip] המילה '{word}' לא קיימת ב-DB ולא נמצאו לה נרדפות. מתעלם ממנה.");
                    continue;
                }



                for (int i = 0; i < _numCategories; i++)
                {
                    // 1. מונה: כמות המופעים של המילה בקטגוריה + 1 (Laplace Smoothing)
                    int wordCountInCat = (countsForWord != null) ? countsForWord[i] : 0;
                    int numerator = wordCountInCat + 1;

                    // 2. מכנה: סך כל המילים בקטגוריה הזו במילון + גודל המילון הכולל
                    int denominator = _totalWordsPerCategory[i] + _vocabularySize;

                    // 3. ההסתברות (לפני הלוגריתם - רק בשביל ההדפסה שנבין)
                    double probability = (double)numerator / denominator;
                    double logProb = Math.Log(probability);

                    // עדכון הציון הסופי
                    finalScores[i] += logProb;

                    // הדפסה מפורטת לכל אינדקס
                    int currentCatId = _categoryIdToIndex.FirstOrDefault(x => x.Value == i).Key;
                    Console.WriteLine($"Index {i} (ID {currentCatId}): Prob = {numerator}/{denominator} ({probability:F5}), New Total Score: {finalScores[i]:F5}");
                }
            }

            // --- שלב בחירת המנצח ---
            Console.WriteLine("\n--- Final Results Summary ---");
            int bestCategoryIndex = 0;
            for (int i = 0; i < finalScores.Length; i++)
            {
                int catId = _categoryIdToIndex.FirstOrDefault(x => x.Value == i).Key;
                Console.WriteLine($"Final Score for ID {catId} (Index {i}): {finalScores[i]:F5}");

                if (finalScores[i] > finalScores[bestCategoryIndex])
                {
                    bestCategoryIndex = i;
                }
            }

            int finalWinnerId = _categoryIdToIndex.FirstOrDefault(x => x.Value == bestCategoryIndex).Key;
            Console.WriteLine($"********************************");
            Console.WriteLine($"THE WINNER IS: Category ID {finalWinnerId}");
            Console.WriteLine($"********************************");

            return finalWinnerId;
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
                // בדיקה אם המילה הדומה קיימת במילון הסטטיסטיקות (זה שבזכרון ה-C#)
                if (WordStatistics.TryGetValue(simWord, out var stats))
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
                    sumCounts[i] /= matchCount;
                }
                return sumCounts;
            }

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
 