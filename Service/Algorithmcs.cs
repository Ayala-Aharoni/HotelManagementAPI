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
        public class Algorithmics
        {
            private readonly ITextAnalyzer _textAnalyzer;
            private readonly INaiveBase _naiveBayes;
            private readonly IRepository<Word> _wordRepo;
            private readonly ICategoryWordRepository _categoryWordRepo;


            public Algorithmics(ITextAnalyzer textAnalyzer, INaiveBase naiveBayes, IRepository<Word> wordRepo,ICategoryWordRepository categoryWordRepo)
            {
                _textAnalyzer = textAnalyzer;
                _naiveBayes = naiveBayes;
                _wordRepo = wordRepo;
                 _categoryWordRepo= categoryWordRepo;   

            }
            public List<string> AnalisisRequest()
            {
                List<string> SplitToSentencesLst = new List<string>();
                SplitToSentencesLst = _textAnalyzer.SplitToSentences("בינתים"/* עדין לה הבנתי מה אני אמורה לשלוח מי מפעיל את זה*/);
                SplitToSentencesLst.RemoveAll(x => x.Length < 2);
                List<string> relevantWords = new List<string>();
                if (SplitToSentencesLst.Count() == 0) // In a case that fails to access the Hebrew NLP library
                    return null;

                foreach (string sentences in SplitToSentencesLst)
                {
                    var lst = _textAnalyzer.AnalyzeSentence(sentences);
                    var forConcat = _textAnalyzer.RemoveIrrelevantWords(lst);
                    relevantWords = relevantWords.Concat(forConcat).ToList();
                }

                return relevantWords;
            }



            //פה עוד לא עדכנתי את הדיקשנרי, רק הוספתי את המילים ל-DB, צריך להוסיף גם לעדכון הדיקשנרי!!!!!!!!!!!!!!!!!!!!!

            public async Task InsertWordsIntoWordTable(List<string> analysisWords, int mycategoryId)
            {
                foreach (var wordText in analysisWords)
                {
                    // 1. האם המילה קיימת בדיקשנרי?
                    if (_naiveBayes.WordStatistics.TryGetValue(wordText, out WordClassificationDTO WordClassificationDTO))
                    {
                        // 2. המילה קיימת! עכשיו נבדוק במערך אם היא קיימת בקטגוריה הספציפית
                        // נניח ש-categoryId הוא האינדקס
                        if (WordClassificationDTO.CategoryCounts[mycategoryId - 1] > 0)
                        {
                            // תרחיש: המילה כבר הופיעה בקטגוריה הזו בעבר (ה-Cache אומר לנו שיש קשר)
                            // אנחנו רק צריכים לעדכן את השכיחות ב-DB
                            await _categoryWordRepo.IncrementFrequency(wordText,mycategoryId);
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
                        WordClassificationDTO.CategoryCounts[mycategoryId - 1]++;//זה עדכון של הדיקשנרי

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

        
        
