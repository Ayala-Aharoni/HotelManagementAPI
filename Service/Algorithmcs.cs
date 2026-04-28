using Common.DTO;
using Repository.Entities;
using Repository.Interfaces;

using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace Service
{
    public class Algorithmics : IAlgorithmcs
    {
        private readonly ITextAnalyzer _textAnalyzer;
        private readonly INaiveBase _naiveBayes;
        private readonly IRepository<Word> _wordRepo;
        private readonly ICategoryWordRepository _categoryWordRepo;
        private readonly TextAnalysisService _textAnalysis;

        public Algorithmics(ITextAnalyzer textAnalyzer, INaiveBase naiveBayes, IRepository<Word> wordRepo, ICategoryWordRepository categoryWordRepo, TextAnalysisService textAnalysis)
        {
            _textAnalyzer = textAnalyzer;
            _naiveBayes = naiveBayes;
            _wordRepo = wordRepo;
            _categoryWordRepo = categoryWordRepo;
            _textAnalysis = textAnalysis;

        }
        //public List<string> AnalisisRequest(string content)
        //{
        //    List<string> SplitToSentencesLst = _textAnalyzer.SplitToSentences(content);
        //    SplitToSentencesLst.RemoveAll(x => x.Length < 2);

        //    if (SplitToSentencesLst.Count == 0)
        //    {
        //        Console.WriteLine("No sentences detected – HebrewNLP may not be working!");
        //        return null;
        //    }

        //    List<string> relevantWords = new List<string>();

        //    Console.WriteLine("SplitToSentencesLst:");
        //    foreach (var s in SplitToSentencesLst)
        //        Console.WriteLine($"- {s}");

        //    foreach (string sentence in SplitToSentencesLst)
        //    {
        //        // 🔹 כאן את יכולה לבדוק את ניתוח המורפולוגיה
        //        var lst = _textAnalyzer.AnalyzeSentence(sentence);
        //        Console.WriteLine($"Analyzing sentence: {sentence}");
        //        Console.WriteLine($"Words found: {lst.Count}");

        //        foreach (var wordList in lst)
        //        {
        //            foreach (var morph in wordList)
        //            {
        //                Console.WriteLine($"Word: {morph.BaseWord}, POS: {morph.PartOfSpeech}");
        //            }
        //        }

        //        var forConcat = _textAnalyzer.RemoveIrrelevantWords(lst);
        //        Console.WriteLine("Relevant words:");
        //        foreach (var w in forConcat)
        //            Console.WriteLine($"-- {w}");

        //        relevantWords = relevantWords.Concat(forConcat).ToList();
        //    }

        //    return relevantWords;
        //}


        public async Task<List<string>> AnalisisRequest(string content)
        {
            var features = await _textAnalysis.AnalyzeTextAsync(content);
            return features;
        }




        public async Task<int> ClassifyText(List<string> analysisWords)
        {
            var c = await _naiveBayes.PredictCategory(analysisWords);
            return c;

        }

        //פה עוד לא עדכנתי את הדיקשנרי, רק הוספתי את המילים ל-DB, צריך להוסיף גם לעדכון הדיקשנרי!!!!!!!!!!!!!!!!!!!!!
        //זה צריך לעבורר לוורדסרביס
        public async Task InsertWordsIntoWordTable(List<string> analysisWords, int mycategoryId)
        {
            Console.WriteLine($"\n[SHERLOCK MODE] Starting check for Category {mycategoryId}");
            Console.WriteLine($"[DEBUG] Dictionary pointer: {_naiveBayes.WordStatistics.GetHashCode()}");
            Console.WriteLine($"[DEBUG] Total words in Dictionary: {_naiveBayes.WordStatistics.Count}");

            foreach (var wordText in analysisWords)
            {
                Console.WriteLine($"🔍 Checking word: '>{wordText}<'");

                try
                {
                    if (_naiveBayes.WordStatistics.TryGetValue(wordText, out WordClassificationDTO WordClassificationDTO))
                    {
                        Console.WriteLine($"   ✅ Found in Dictionary! WordId: {WordClassificationDTO.WordId}");

                        int catIdx = _naiveBayes.GetIndex(mycategoryId);
                        Console.WriteLine($"   [DEBUG] Category Index for {mycategoryId} is: {catIdx}");

                        if (catIdx != -1)
                        {
                            // בדיקה מה הערך הנוכחי בזיכרון לפני ההחלטה
                            int currentCountInMemory = WordClassificationDTO.CategoryCounts[catIdx];
                            Console.WriteLine($"   [DEBUG] Current count in memory for this category: {currentCountInMemory}");

                            if (currentCountInMemory > 0)
                            {
                                Console.WriteLine($"   📈 Case 1: Word + Category exists. Calling IncrementFrequency...");
                                await _categoryWordRepo.IncrementFrequency(wordText, mycategoryId);
                                Console.WriteLine($"   Successfully called IncrementFrequency for '{wordText}'");
                            }
                            else
                            {
                                Console.WriteLine($"   🔗 Case 2: Word exists, New Category. Linking in DB...");
                                var newRelation = new CategoryWord { WordId = WordClassificationDTO.WordId, CategoryId = mycategoryId, Frequency = 1 };

                                // הדפסה של הנתונים שנשלחים ל-DB
                                Console.WriteLine($"   [DB-SEND] Adding Relation: WordId={newRelation.WordId}, CatId={newRelation.CategoryId}");

                                await _categoryWordRepo.AddItem(newRelation);
                                Console.WriteLine($"   Successfully called AddItem (Case 2) for '{wordText}'");
                            }

                            WordClassificationDTO.CategoryCounts[catIdx]++;
                            Console.WriteLine($"   [MEM-UPDATE] Memory count for '{wordText}' is now: {WordClassificationDTO.CategoryCounts[catIdx]}");
                        }
                        else
                        {
                            Console.WriteLine($"   ⚠️ WARNING: Category ID {mycategoryId} not found in index mapping!");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"   ❌ NOT FOUND in Dictionary: '>{wordText}<'");

                        var similarKey = _naiveBayes.WordStatistics.Keys.FirstOrDefault(k => k.Trim() == wordText.Trim());
                        if (similarKey != null)
                        {
                            Console.WriteLine($"   ⚠️ WAIT! Found a similar key: '>{similarKey}<'. Check for casing/spaces!");
                        }

                        Console.WriteLine($"   🆕 Going to Case 3: Adding new word to DB...");

                        var newWord = new Word { Text = wordText };
                        await _wordRepo.AddItem(newWord);
                        Console.WriteLine($"   [DB-ADD] New Word added. Generated WordId: {newWord.WordId}");

                        var newRelation = new CategoryWord { WordId = newWord.WordId, CategoryId = mycategoryId, Frequency = 1 };
                        await _categoryWordRepo.AddItem(newRelation);
                        Console.WriteLine($"   [DB-ADD] Relation added for new word.");

                        _naiveBayes.AddNewWordToDictinary(wordText, mycategoryId, newWord.WordId);
                        Console.WriteLine($"   [MEM-ADD] Word added to Dictionary.");
                    }
                }
                catch (Exception ex)
                {
                    // הדפסת שגיאה מפורטת למקרה שה-DB יחזיר שגיאה (כמו מפתח זר או כפילות)
                    Console.WriteLine($"   ‼️ CRITICAL ERROR for word '{wordText}': {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                    }
                }
            }
            Console.WriteLine($"[SHERLOCK MODE] Finished processing all words.\n");
        }




        //זה פונקצית ענישה שמענישה אם המודל טעה, היא מורידה את התדירות של המילים שגרמו לטעות, גם ב-DB וגם בזיכרון של המודל, כדי שהחיזוי הבא יהיה מושפע מהטעות וינסה לא לטעות שוב עם אותן מילים וקטגוריה
        public async Task DecreaseWordsFrequency(List<string> analysisWords, int wrongCategoryId)
        {
            Console.WriteLine($"\n[PUNISHMENT MODE] Reducing strength for Category {wrongCategoryId}");

            int catIdx = _naiveBayes.GetIndex(wrongCategoryId);
            if (catIdx == -1) return; // הגנה אם הקטגוריה לא קיימת

            foreach (var wordText in analysisWords)
            {
                // אנחנו מענישים רק מילים שהמודל כבר מכיר והן אלו שגרמו לטעות
                if (_naiveBayes.WordStatistics.TryGetValue(wordText, out WordClassificationDTO wordInfo))
                {
                    int currentCount = wordInfo.CategoryCounts[catIdx];

                    if (currentCount > 0)
                    {
                        Console.WriteLine($"   📉 Punishing word '{wordText}': Frequency {currentCount} -> {currentCount - 1}");

                        // 1. עדכון בבסיס הנתונים (הפחתה ב-1)
                        await _categoryWordRepo.DecrementFrequency(wordText, wrongCategoryId);

                        // 2. עדכון בזיכרון של המודל (כדי שהחיזוי הבא יושפע מיד)
                        wordInfo.CategoryCounts[catIdx]--;

                        Console.WriteLine($"   [MEM-REDUCE] Word '{wordText}' is now weaker for category {wrongCategoryId}");
                    }
                }
            }
            Console.WriteLine($"[PUNISHMENT MODE] Finished punishing the model.\n");
        }
    }
}
    








