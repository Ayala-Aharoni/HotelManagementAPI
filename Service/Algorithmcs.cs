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
    public class Algorithmics:IAlgorithmcs
    {
        private readonly ITextAnalyzer _textAnalyzer;
        private readonly INaiveBase _naiveBayes;
        private readonly IRepository<Word> _wordRepo;
        private readonly ICategoryWordRepository _categoryWordRepo;
        private readonly TextAnalysisService _textAnalysis;

        public Algorithmics(ITextAnalyzer textAnalyzer, INaiveBase naiveBayes, IRepository<Word> wordRepo, ICategoryWordRepository categoryWordRepo , TextAnalysisService textAnalysis)
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

        public async Task InsertWordsIntoWordTable(List<string> analysisWords, int mycategoryId)
        {
            foreach (var wordText in analysisWords)
            {
                // 1. האם המילה קיימת בדיקשנרי?
                if (_naiveBayes.WordStatistics.TryGetValue(wordText, out WordClassificationDTO WordClassificationDTO))
                {
                    // שולפים את האינדקס הנכון כדי להשתמש בו בבדיקה ובעדכון
                    int catIdx = _naiveBayes.GetIndex(mycategoryId);

                    if (catIdx != -1) // תמיד כדאי לבדוק ליתר ביטחון
                    {
                        // 2. המילה קיימת! עכשיו נבדוק במערך אם היא קיימת בקטגוריה הספציפית
                        // כאן התיקון: משתמשים ב-catIdx במקום ב- (mycategoryId - 1)
                        if (WordClassificationDTO.CategoryCounts[catIdx] > 0)
                        {
                            // תרחיש: המילה כבר הופיעה בקטגוריה הזו בעבר (ה-Cache אומר לנו שיש קשר)
                            // אנחנו רק צריכים לעדכן את השכיחות ב-DB
                            await _categoryWordRepo.IncrementFrequency(wordText, mycategoryId);
                        }
                        else
                        {
                            var newRelation = new CategoryWord
                            {
                                WordId = WordClassificationDTO.WordId,
                                CategoryId = mycategoryId,
                                Frequency = 1
                            };
                            await _categoryWordRepo.AddItem(newRelation);
                        }

                        WordClassificationDTO.CategoryCounts[catIdx]++;//זה עדכון של הדיקשנרי
                    }
                }
                else
                {
                    var newWord = new Word
                    {
                        Text = wordText
                    };
                    await _wordRepo.AddItem(newWord);

                    var newRelation = new CategoryWord
                    {
                        WordId = newWord.WordId,
                        CategoryId = mycategoryId,
                        Frequency = 1
                    };
                    await _categoryWordRepo.AddItem(newRelation);
                    //זה אם המילה לא קיימת!!
                    _naiveBayes.AddNewWordToDictinary(wordText, mycategoryId, newWord.WordId);
                }
            }
        }

    }
}
    






