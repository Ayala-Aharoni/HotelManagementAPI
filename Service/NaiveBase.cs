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

        public Dictionary<string, WordClassificationDTO> WordStatistics { get; private set; } = new Dictionary<string, WordClassificationDTO>();
        private int[] _totalWordsPerCategory;

        private Dictionary<int, int> _categoryIdToIndex = new Dictionary<int, int>();//זה בשביל מיפוי האינדסים 

        private int _vocabularySize;

        private int _numCategories;

        private double[] _categoryLogPriors;




        public NaiveBase(IRepository<Category> repositoryy, IRepository<Request> reqrepository, IRepository<Word> wordRepo, ICategoryWordRepository categoryWordRepo)
        {
            _Categoryrepo = repositoryy;
            _Requestrepo = reqrepository;
            _wordRepo = wordRepo;
            _categoryWordRepo = categoryWordRepo;
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
            return _categoryIdToIndex[categoryId];
        }
        public async Task LoadModel()
        {
            var categories = await _Categoryrepo.GetAll();
            _numCategories = categories.Count();

            //פה אני ממלא את המילון שלי הקטגוריות 
            _categoryIdToIndex.Clear();
            for (int i = 0; i < categories.Count; i++)
            {
                // המפתח הוא ה-ID מהדאטה-בייס, הערך הוא המיקום במערך (0, 1, 2...)
                _categoryIdToIndex[categories[i].CategoryId] = i;
            }
            

            var allRequests = await _Requestrepo.GetAll();
            int totalAllRequests = allRequests.Count();


            var _categoryRequestsCounts = new int[_numCategories];//פה אני ממלא מערך כמה בקשות יש לי בעבור כל קטגוריה
            for (int i = 0; i < _numCategories; i++)
            {
                _categoryRequestsCounts[i] = allRequests.Count(r => r.CategoryId == categories[i].CategoryId);
            }

             _categoryLogPriors = new double[_numCategories];//כאן אני שמה כבר את ההסתברות של הקטגוריה עצמהה 
            for (int i = 0; i < _numCategories; i++)
            {
                double pCat;
                if (totalAllRequests == 0)
                {
                    // אין בקשות כלל → נותנים הסתברות שווה לכל קטגוריה
                    pCat = 1.0 / _numCategories;
                }
                else
                {
                    pCat = (double)_categoryRequestsCounts[i] / totalAllRequests;
                }

                _categoryLogPriors[i] = Math.Log(pCat);
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

            foreach (var word in words)
            {
                Console.WriteLine($"--- Analyzing word: '{word}' ---");//עשיתי רק לבדיקה נא למחוק אחר כך!!!!!!!!!!!!!!!
              

                int[] countsForWord = null;

                if (WordStatistics.TryGetValue(word, out WordClassificationDTO stats))
                {
                    // עכשיו זה יעבוד! השתמשנו ב-stats במקום בשם של המחלקה
                    Console.WriteLine($"Word '{word}' FOUND in Dictionary! Counts: {string.Join(", ", stats.CategoryCounts)}");
                    countsForWord = stats.CategoryCounts;
                }
                else
                {
                    Console.WriteLine($"Word '{word}' NOT FOUND in Dictionary.");
                }

                //עד כאן למחוקק אחכ כךךךךךך זה לבדיקהה


                if (WordStatistics.TryGetValue(word, out WordClassificationDTO WordClassificationDTO))
                {
                    countsForWord = WordClassificationDTO.CategoryCounts;
                }
                else
                {
                    // אם לא נמצא - משתמשים בלוגיקה של משי למציאת מילים דומות
                 //   countsForWord = await GetAverageCountsForSimilarWords(word);
                }


                for (int i = 0; i < _numCategories; i++)
                {
                    int wordCountInCat = (countsForWord != null) ? countsForWord[i] : 0;

                    double pWordGivenCat = (double)(wordCountInCat + 1) / (_totalWordsPerCategory[i] + _vocabularySize);

                    finalScores[i] += Math.Log(pWordGivenCat);
                }
            }

            int bestCategoryIndex = 0;
            for (int i = 1; i < finalScores.Length; i++)
            {
                Console.WriteLine($"Category Index {i} Score: {finalScores[i]}");//גם זה למחוק זה רק לבדיקה!!!!!
                if (finalScores[i] > finalScores[bestCategoryIndex])
                    bestCategoryIndex = i;
            }

            return _categoryIdToIndex.FirstOrDefault(x => x.Value == bestCategoryIndex).Key;
        }


        //private async Task<int[]> GetAverageCountsForSimilarWords(string word)
        //{
        //    // 1. קוראים לפונקציה של משי/האלגוריתם שמוצא רשימת מילים דומות (Strings)
        //    // הערה: את צריכה לוודא שיש לך פונקציה כזו שמחזירה List<string>
        //    List<string> similarWords = await FindSimilarWords(word);

        //    if (similarWords == null || !similarWords.Any())
        //        return null;

        //    // 2. מכינים מערך צובר בגודל כמות הקטגוריות
        //    int[] sumCounts = new int[_numCategories];
        //    int matchCount = 0;

        //    // 3. רצים על כל המילים הדומות שמצאנו
        //    foreach (var simWord in similarWords)
        //    {
        //        // בודקים אם המילה הדומה קיימת במילון הסטטיסטיקות שלנו
        //        if (WordStatistics.TryGetValue(simWord, out var stats))
        //        {
        //            for (int i = 0; i < _numCategories; i++)
        //            {
        //                sumCounts[i] += stats.CategoryCounts[i];
        //            }
        //            matchCount++;
        //        }
        //    }

        //    // 4. אם מצאנו לפחות מילה אחת דומה שיש עליה מידע - מחשבים ממוצע
        //    if (matchCount > 0)
        //    {
        //        for (int i = 0; i < _numCategories; i++)
        //        {
        //            sumCounts[i] /= matchCount; // מחלקים בכמות המילים שמצאנו כדי לקבל ממוצע
        //        }
        //        return sumCounts;
        //    }
        //    return null;
        //}






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
