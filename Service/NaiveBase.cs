using Common.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Service
{
    public class NaiveBase : INaiveBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        //הסוג הזה עדיף כי הוא מתמודד עם תהליכונים בצורה טובה יותר, מאפשר גישה בטוחה ממספר תהליכים בו זמנית בלי צורך ב-lock מורכב, ומונע בעיות של נתונים לא עקביים או קריסות שיכולות לקרות עם Dictionary רגיל כשיש גישה וכתיבה בו זמנית.
        public ConcurrentDictionary<string, WordClassificationDTO> WordStatistics { get; private set; } = new();
        private Dictionary<int, int> _categoryIdToIndex = new();
        private Dictionary<int, int> _indexToCategoryId = new();
        private readonly IMemoryCache _similarWordsScoresCache;
        private double[] _categoryLogPriors;
        private int[] _totalWordsPerCategory;
        private int _vocabularySize;
        private int _numCategories;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
      
        public NaiveBase(IServiceScopeFactory scopeFactory, IMemoryCache similarWordsScoresCache)
        {
            _scopeFactory = scopeFactory;
            _similarWordsScoresCache = similarWordsScoresCache;
        }
        public async Task LoadModel()
        {
            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine("\n[LoadModel] === STARTING MODEL LOAD WITH PRIORS ===");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var categoryRepo = scope.ServiceProvider.GetRequiredService<IRepository<Category>>();
                    var categoryWordRepo = scope.ServiceProvider.GetRequiredService<ICategoryWordRepository>();
                    var requestRepo = scope.ServiceProvider.GetRequiredService<IRepository<Request>>();
                    var wordRepo = scope.ServiceProvider.GetRequiredService<IRepository<Word>>();
                    var categories = await categoryRepo.GetAll();
                    if (categories == null || !categories.Any())
                    {
                        Console.WriteLine("[LoadModel] ERROR: No categories found in DB!");
                        return;
                    }
                    var allRequests = await requestRepo.GetAll();
                    int totalRequests = allRequests.Count();
                    _numCategories = categories.Count;
                    _categoryLogPriors = new double[_numCategories];
                    _totalWordsPerCategory = new int[_numCategories];
                    // 1. מיפוי קטגוריות
                    _categoryIdToIndex.Clear();
                    _indexToCategoryId.Clear();
                    for (int i = 0; i < categories.Count; i++)
                    {
                        var catId = categories[i].CategoryId;
                        _categoryIdToIndex[catId] = i;
                        _indexToCategoryId[i] = catId;
                    }
                    // 2. חישוב Priors מבוסס נתונים (Laplace Smoothing)
                    // הנוסחה: Log( (בקשות בקטגוריה + 1) / (סך הבקשות + מספר הקטגוריות) )
                    for (int i = 0; i < _numCategories; i++)
                    {
                        int currentCatId = _indexToCategoryId[i];
                        int requestsInThisCat = allRequests.Count(r => r.CategoryId == currentCatId);
                        double probability = (double)(requestsInThisCat + 1) / (totalRequests + _numCategories);
                        _categoryLogPriors[i] = Math.Log(probability);
                        Console.WriteLine($"[LoadModel] Category {currentCatId}: Requests={requestsInThisCat}, Prior={_categoryLogPriors[i]:F4}");
                    }
                    // 3. טעינת המילון הסטטיסטי (מילים ושכיחויות)
                    await LoadDictionaryInternal(categories.ToList(), categoryWordRepo, wordRepo);
                    Console.WriteLine($"[LoadModel] SUCCESS: Model loaded with {WordStatistics.Count} words and statistical Priors.");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private async Task LoadDictionaryInternal(List<Category> categories, ICategoryWordRepository categoryWordRepo, IRepository<Word> wordRepo)
        {
            var allWords = await wordRepo.GetAll();
            WordStatistics.Clear();
            _vocabularySize = 0;

            foreach (var w in allWords)
            {
                string text = w.Text.ToLower().Trim();
                if (!WordStatistics.ContainsKey(text))
                {
                    WordStatistics[text] = new WordClassificationDTO(_numCategories)
                    {
                        Word = text,
                        WordId = w.WordId // כאן המפתח: המערכת זוכרת את ה-ID האמיתי מה-DB
                    };
                    _vocabularySize++;
                }
            }
            var allLinks = await categoryWordRepo.GetAll();
            //זה בירוק בגלל החלק הלמעלה אם מורידים תלמעלה להוריד גם אותו 
            // WordStatistics.Clear();

           // _vocabularySize = 0; בגלל שהוא סיוג לי לקטגוריות עם הכי פחות מילים 

            foreach (var link in allLinks)
            {
                // כאן אנחנו חייבים לוודא שהמילה קיימת בטקסט
                if (link.Word == null || string.IsNullOrEmpty(link.Word.Text)) continue;

                string word = link.Word.Text.ToLower().Trim();

                if (!WordStatistics.ContainsKey(word))
                {
                    WordStatistics[word] = new WordClassificationDTO(_numCategories)
                    {
                        Word = word,
                        WordId = link.WordId
                    };
                    _vocabularySize++;
                }

                if (_categoryIdToIndex.TryGetValue(link.CategoryId, out int catIdx))
                {
                    WordStatistics[word].CategoryCounts[catIdx] = link.Frequency;
                    _totalWordsPerCategory[catIdx] += link.Frequency;
                }
            }
            Console.WriteLine($"[LoadDictionary] Loaded {WordStatistics.Count} unique words from DB.");
        }     
        public async Task<int> PredictCategory(List<string> words)
        {
            if (_categoryLogPriors == null) await LoadModel();

            double[] finalScores = new double[_numCategories];
            Array.Copy(_categoryLogPriors, finalScores, _numCategories);

            var tokens = words?.Select(w => w.ToLower().Trim()).Where(w => !string.IsNullOrEmpty(w)).ToList() ?? new();
            Console.WriteLine($"\n--- Analyzing {tokens.Count} tokens ---");

            // =================================================================
            // שלב 1: איסוף כל המילים שלא קיימות במילון ולא קיימות בקאש
            // =================================================================
            var unknownWords = new List<string>();
            foreach (var word in tokens)
            {
                if (!WordStatistics.ContainsKey(word) && !_similarWordsScoresCache.TryGetValue(word, out _))
                {
                    if (!unknownWords.Contains(word))
                    {
                        unknownWords.Add(word);
                    }
                }
            }

            if (unknownWords.Any())
            {
                Console.WriteLine($"[BATCH INFO] Step 1: Found {unknownWords.Count} words missing. Sending them all to Python IN ONE GO!");
            }

            // =================================================================
            // שלב 2: נסיעה אחת ויחידה לפייתון לקבלת התוצאות לכל המילים החסרות
            // =================================================================
            var pythonBatchResults = new Dictionary<string, List<PythonMatchDTO>>();

            if (unknownWords.Any())
            {
                using var scope = _scopeFactory.CreateScope();
                var similarService = scope.ServiceProvider.GetRequiredService<ISimiliarWord>();
                var allKnownWords = WordStatistics.Keys.ToList();

                Console.WriteLine("[BATCH INFO] Step 2: Calling Python API...");

                // קריאה לפונקציה החדשה שיצרנו, שמקבלת רשימה ומחזירה מילון של תוצאות
                pythonBatchResults = await similarService.GetSimilarWordsFromPython(unknownWords, allKnownWords, 0.7);

                Console.WriteLine($"[BATCH INFO] Step 3: SUCCESS! Python returned data for {pythonBatchResults.Count} words.");
            }

            // =================================================================
            // שלב 3: הלולאה המרכזית - עוברים על כל המילים ומחשבים את הניקוד
            // =================================================================
            foreach (var word in tokens)
            {
                int[] countsForWord = null;

                if (WordStatistics.TryGetValue(word, out WordClassificationDTO stats))
                {
                    // המילה קיימת בבסיס הנתונים שלנו
                    countsForWord = stats.CategoryCounts;
                    Console.WriteLine($"[MATCH] '{word}' in DB.");
                }
                else if (_similarWordsScoresCache.TryGetValue(word, out int[] cachedCounts))
                {
                    // המילה קיימת בזיכרון המטמון שלנו מאתמול
                    countsForWord = cachedCounts;
                    Console.WriteLine($"[CACHE] '{word}' found in IMemoryCache.");
                }
                else if (pythonBatchResults.TryGetValue(word, out var matches) && matches != null && matches.Any())
                {
                    // המילה חזרה עכשיו מהקריאה המרוכזת שעשינו לפייתון!
                    Console.WriteLine($"[PYTHON BATCH] '{word}' matched with {matches.Count} similar words.");
                    countsForWord = CalculateAverageCounts(matches);

                    // שומרים בקאש לפעם הבאה
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                    };
                    _similarWordsScoresCache.Set(word, countsForWord, cacheOptions);
                }

                // חישוב Naive Bayes מתמטי
                if (countsForWord != null)
                {
                    for (int i = 0; i < _numCategories; i++)
                    {
                        double numerator = countsForWord[i] + 0.5;
                        double denominator = _totalWordsPerCategory[i] + (0.5 * _vocabularySize);
                        finalScores[i] += Math.Log(numerator / denominator);
                    }
                }
            }

            // בחירת הקטגוריה המנצחת
            int bestIndex = 0;
            for (int i = 1; i < finalScores.Length; i++)
            {
                if (finalScores[i] > finalScores[bestIndex]) bestIndex = i;
            }

            if (_indexToCategoryId.TryGetValue(bestIndex, out int winnerId))
            {
                Console.WriteLine($"--- FINAL RESULT: Category ID {winnerId} ---\n");
                return winnerId;
            }
            return -1;
        }
        private int[] CalculateAverageCounts(List<PythonMatchDTO> matches)
        {
            int[] sumCounts = new int[_numCategories];
            int matchCount = 0;

            foreach (var match in matches)
            {
                if (WordStatistics.TryGetValue(match.Word, out var stats))
                {
                    for (int i = 0; i < _numCategories; i++)
                    {
                        sumCounts[i] += stats.CategoryCounts[i];
                    }
                    matchCount++; // סופרים כמה מילים מתוך הרשימה באמת קיימות אצלנו במילון
                }
            }
            // התיקון המרכזי: אם מצאנו מילים, מחלקים את הסכום בכמות המילים כדי לקבל ממוצע אמיתי
            if (matchCount > 0)
            {
                for (int i = 0; i < _numCategories; i++)
                {
                    sumCounts[i] /= matchCount;
                }
            }
            Console.WriteLine($"[CALC] Aggregated average counts from {matchCount} matched words: {string.Join(",", sumCounts)}");
            return sumCounts;
        }
        public int GetIndex(int categoryId) => _categoryIdToIndex.TryGetValue(categoryId, out int index) ? index : -1;
        public void AddNewWordToDictinary(string wordText, int categoryId, int wordId)
        {
            int catIdx = GetIndex(categoryId);
            if (catIdx == -1) return;
            string word = wordText.ToLower().Trim();

            bool isNewWord = false;

            // שימוש נכון ב-ConcurrentDictionary לעדכון או הוספה בפעולה אטומית אחת
            WordStatistics.AddOrUpdate(
                word,
                // מקרה 1: המילה לא קיימת במילון - יוצרים חדשה
                addValueFactory: key =>
                {
                    isNewWord = true; // נסמן לעצמנו שזו מילה חדשה
                    var newDto = new WordClassificationDTO(_numCategories) { Word = key, WordId = wordId };
                    newDto.CategoryCounts[catIdx] = 1; // קאונט ראשוני
                    return newDto;
                },
                // מקרה 2: המילה קיימת - מעדכנים את הספירה שלה
                updateValueFactory: (key, existingDto) =>
                {
                    // Interlocked.Increment עושה ++ בצורה שבטוחה לריבוי תהליכים
                    Interlocked.Increment(ref existingDto.CategoryCounts[catIdx]);
                    return existingDto;
                }
            );

            // עדכון המונים הכלליים בצורה בטוחה לתהליכים מקבילים (Thread-Safe)
            if (isNewWord)
            {
                Interlocked.Increment(ref _vocabularySize);
            }
            Interlocked.Increment(ref _totalWordsPerCategory[catIdx]);
        }
        public async Task LoadDictionaryAsync(List<Category> categories, ICategoryWordRepository _categoryWordRepo, IRepository<Word> wordRepo)
        {
            await LoadDictionaryInternal(categories, _categoryWordRepo, wordRepo);
        }
    }
}