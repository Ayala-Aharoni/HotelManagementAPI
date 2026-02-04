using DataContext.DTO;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class WordService
    {
       
            private readonly IRepository<Word> _wordRepository;
            public WordService(IRepository<Word> wordRepository)
            {
               _wordRepository= wordRepository;
            }
        public async Task<Word> AddWordAsync(WordDTO dto)
        {
         
            var existing = (await _wordRepository.GetAll())
                           .FirstOrDefault(w =>
                               w.Text.ToLower() == dto.Text.ToLower() &&
                               w.CategoryId == dto.CategoryId);

            if (existing != null)
                throw new Exception("המילה כבר קיימת באותה קטגוריה");

            var word = new Word
            {
                Text = dto.Text,
                CategoryId = dto.CategoryId,
                Frequency = dto.Frequency
            };

            return await _wordRepository.AddItem(word);
        }






    }
}
