using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HebrewNLP;
using Service.Interfaces;

namespace Service
{
    public class TextAnalyzer : ITextAnalyzer
    {
        public List<string> SplitToSentences(string inputText)
        {
            // "אני אכתוב את זה אחר כך, בינתיים אל תעשה לי שגיאות"
            throw new NotImplementedException();
        }
    }
}
