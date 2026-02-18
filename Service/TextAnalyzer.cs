using HebrewNLP;
using HebrewNLP.Morphology;
using Microsoft.AspNetCore.Builder;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class TextAnalyzer: ITextAnalyzer
    {
        private readonly string _password;

        public TextAnalyzer(string password)
        {
            _password = password;
            HebrewNLP.HebrewNLP.Password = _password;
        }

        public List<string> SplitToSentences(string allContent)
        {
            if (string.IsNullOrWhiteSpace(allContent))
                return new List<string>();

            try
            {
                return HebrewNLP.Sentencer.Sentences(allContent);
            }
            catch
            {
                return new List<string>();
            }
        }

        //זו פונקציה שבעבור כל מילה חוזר לי עצם נניח מגבות-שם עצם , רבים/
        public  List<List<MorphInfo>> AnalyzeSentence(string sentence)
        {
            try
            {
                return HebrewMorphology.AnalyzeSentence(sentence);
            }
            catch (Exception)
            {
                return new List<List<MorphInfo>>();
            }
        }


        public List<string> RemoveIrrelevantWords(List<List<MorphInfo>> analyzedSentence)
        {
            List<string> relevantWords = new List<string>();
            foreach (var item in analyzedSentence)
            {

                foreach (var analyzedWord in item)
                {
                    if (IsRelevantPartOfSpeech(analyzedWord))
                    {

                        relevantWords.Add(analyzedWord.BaseWord);
                        break; //!!! מספיק ניתוח אחד מתאים
                    }
                }
            }

            return relevantWords;
        }
        public bool IsRelevantPartOfSpeech(MorphInfo morphInfo)
        {
            return morphInfo.PartOfSpeech == PartOfSpeech.VERB ||
                   morphInfo.PartOfSpeech == PartOfSpeech.NOUN ||
                   morphInfo.PartOfSpeech == PartOfSpeech.ADJECTIVE ||
                   morphInfo.PartOfSpeech == PartOfSpeech.PROPER_NOUN;
        }

    }
}
