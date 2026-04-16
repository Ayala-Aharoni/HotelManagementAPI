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

namespace Service
{
    public class NaiveBase : INaiveBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public Dictionary<string, WordClassificationDTO> WordStatistics { get; private set; } = new();
        private int[] _totalWordsPerCategory;
        private Dictionary<int, int> _categoryIdToIndex = new();
        private Dictionary<int, int> _indexToCategoryId = new();
        private Dictionary<string, int[]> _similarWordsScoresCache = new();
        private int _vocabularySize;
        private int _numCategories;
        private double[] _categoryLogPriors;

        public NaiveBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LoadModel()
        {
            Console.WriteLine("\n[LoadModel] === STARTING MODEL LOAD WITH PRIORS ===");

            using (var scope = _scopeFactory.CreateScope())
            {
                var categoryRepo = scope.ServiceProvider.GetRequiredService<IRepository<Category>>();
                var categoryWordRepo = scope.ServiceProvider.GetRequiredService<ICategoryWordRepository>();
                // הוספת ה-Repository של הבקשות כדי לחשב הסתברות מוקדמת
                var requestRepo = scope.ServiceProvider.GetRequiredService<IRepository<Request>>();

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
                await LoadDictionaryInternal(categories.ToList(), categoryWordRepo);

                Console.WriteLine($"[LoadModel] SUCCESS: Model loaded with {WordStatistics.Count} words and statistical Priors.");
            }
        }
        private async Task LoadDictionaryInternal(List<Category> categories, ICategoryWordRepository categoryWordRepo)
        {
            var allLinks = await categoryWordRepo.GetAll();
            WordStatistics.Clear();
            _vocabularySize = 0;

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
            Console.WriteLine("\n***** STARTING PREDICTION *****");

            if (_categoryLogPriors == null || _categoryLogPriors.Length == 0)
            {
                Console.WriteLine("[Warning] Model not loaded. Loading now...");
                await LoadModel();
            }

            double[] finalScores = new double[_numCategories];
            Array.Copy(_categoryLogPriors, finalScores, _numCategories);

            var tokens = words?.Select(w => w.ToLower().Trim())
                               .Where(w => !string.IsNullOrWhiteSpace(w)).ToList() ?? new();

            foreach (var word in tokens)
            {
                int[] countsForWord = null;
                if (WordStatistics.TryGetValue(word, out WordClassificationDTO stats))
                {
                    countsForWord = stats.CategoryCounts;
                    Console.WriteLine($"[DB] Match: '{word}' -> Counts: {string.Join(",", countsForWord)}");
                }
                else
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var similarWordsService = scope.ServiceProvider.GetRequiredService<ISimiliarWord>();
                        countsForWord = await GetSynonymCounts(word, similarWordsService);
                    }
                }

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

            int bestIndex = 0;
            for (int i = 1; i < finalScores.Length; i++)
            {
                if (finalScores[i] > finalScores[bestIndex]) bestIndex = i;
            }

            // בדיקת ה-ID
            if (_indexToCategoryId.TryGetValue(bestIndex, out int winnerId))
            {
                Console.WriteLine($"WINNER: Category ID {winnerId} (Index {bestIndex})");
                return winnerId;
            }

            Console.WriteLine("WINNER: None (Mapping failed)");
            return -1;
        }

        private async Task<int[]> GetSynonymCounts(string word, ISimiliarWord similarService)
        {
            if (_similarWordsScoresCache.TryGetValue(word, out var cached)) return cached;

            Console.WriteLine($"[Synonyms] Searching for: {word}");
            var similarWordsList = await similarService.GetSimilarWordsAsync(word);
            int[] averageCounts = GetAverageCountsForSimilarWords(similarWordsList?.ToList());

            if (averageCounts != null) _similarWordsScoresCache[word] = averageCounts;
            return averageCounts;
        }

        public int[] GetAverageCountsForSimilarWords(List<string> similarWords)
        {
            if (similarWords == null || !similarWords.Any()) return null;

            int[] sumCounts = new int[_numCategories];
            int matchCount = 0;

            foreach (var simWord in similarWords)
            {
                if (WordStatistics.TryGetValue(simWord.ToLower().Trim(), out var stats))
                {
                    for (int i = 0; i < _numCategories; i++) sumCounts[i] += stats.CategoryCounts[i];
                    matchCount++;
                }
            }

            if (matchCount > 0)
            {
                for (int i = 0; i < _numCategories; i++) sumCounts[i] /= matchCount;
                return sumCounts;
            }
            return null;
        }

        public int GetIndex(int categoryId) => _categoryIdToIndex.TryGetValue(categoryId, out int index) ? index : -1;

        public void AddNewWordToDictinary(string wordText, int categoryId, int wordId)
        {
            int catIdx = GetIndex(categoryId);
            if (catIdx == -1) return;

            string word = wordText.ToLower().Trim();
            if (!WordStatistics.ContainsKey(word))
            {
                WordStatistics[word] = new WordClassificationDTO(_numCategories) { Word = word, WordId = wordId };
                _vocabularySize++;
            }
            WordStatistics[word].CategoryCounts[catIdx]++;
            _totalWordsPerCategory[catIdx]++;
        }

        // מימוש ה-Interface שחסר לך
        public async Task LoadDictionaryAsync(List<Category> categories, ICategoryWordRepository _categoryWordRepo)
        {
            await LoadDictionaryInternal(categories, _categoryWordRepo);
        }
    }
}