using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class CategoryWordRepository : ICategoryWordRepository
    {
        private readonly Icontext ctx;
        //לא לשכוחחחחח לתקןןן פהה אתתת כלללל הפנוקציותת!! עשיתי רק בינתים
        public CategoryWordRepository(Icontext context)
        {
            ctx = context;
        }

        public async Task<List<CategoryWord>> GetAll()
        {
            return await ctx.CategoryWords
                .Include(cw => cw.Category)
                .Include(cw => cw.Word)
                .ToListAsync();
        }

        public async Task<CategoryWord> GetById(int id)
        {
            return await ctx.CategoryWords
                .Include(cw => cw.Category)
                .Include(cw => cw.Word)
                .FirstOrDefaultAsync(cw => cw.Id == id);
        }

        public async Task<CategoryWord> AddItem(CategoryWord item)
        {
            await ctx.CategoryWords.AddAsync(item);
            await ctx.Save();
            return item;
        }

        public async Task<CategoryWord> UpdateItem(int id, CategoryWord item)
        {
            var existing = await ctx.CategoryWords.FindAsync(id);
            if (existing == null)
                return null;

            existing.CategoryId = item.CategoryId;
            existing.WordId = item.WordId;
            existing.Frequency = item.Frequency;

            await ctx.Save();
            return existing;
        }

        public async Task DeleteItem(int id)
        {
            var item = await ctx.CategoryWords.FindAsync(id);
            if (item != null)
            {
                ctx.CategoryWords.Remove(item);
                await ctx.Save();
            }
        }


        public async Task IncrementFrequency(string wordText, int categoryId)
        {
            
            var record = await ctx.CategoryWords
                .FirstOrDefaultAsync(cw => cw.Word.Text == wordText && cw.CategoryId == categoryId);

            if (record != null)
            {
                record.Frequency++; 
                await ctx.Save(); 
            }
        }
    }
}