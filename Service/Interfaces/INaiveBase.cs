using Common.DTO;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface INaiveBase
    {
        Task LoadModel();
        Task LoadDictionaryAsync(List<Category> categories);
         Task<int> PredictCategory(List<string> words);

        void AddNewWordToDictinary(string wordText, int categoryId, int wordId);
        Dictionary<string, WordClassificationDTO> WordStatistics { get; }







    }
}
