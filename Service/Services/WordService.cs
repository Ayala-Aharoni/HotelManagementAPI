using DataContext.DTO;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
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
            var allWords = await _wordRepository.GetAll();
            var existing = allWords.FirstOrDefault(w => w.Text.ToLower() == dto.Text.ToLower());

            if (existing != null)
            {
                return existing;
            }

            var word = new Word
            {
                Text = dto.Text
            };

            return await _wordRepository.AddItem(word);
        }






    }
}
