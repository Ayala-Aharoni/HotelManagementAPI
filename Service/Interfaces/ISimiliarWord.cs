using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ISimiliarWord
    {
        //Task<List<string>> GetSimilarWordsAsync(string word);
        Task<List<PythonMatchDTO>> GetSimilarWordsFromPython(string word, List<string> allKeywords, double threshold);
    }
}
