using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class WordRepository : IRepository<Word>
    {
        private readonly Icontext ctx;
        public WordRepository(Icontext context)
        {
            ctx = context;
        }

        public async Task<List<Word>> GetAll()
        {
            return await ctx.Words.ToListAsync();
        }

        public async Task<Word> GetById(int id)
        {
            return await ctx.Words.FindAsync(id);
        }

        public async Task<Word> AddItem(Word item)
        {
            await ctx.Words.AddAsync(item);
            await ctx.Save();
            return item;
        }

        public async Task<Word> UpdateItem(int id, Word item)
        {
            var existing = await ctx.Words.FindAsync(id);
            if (existing == null) return null;

            existing.Text = item.Text;
            existing.CategoryId = item.CategoryId;
            existing.Frequency = item.Frequency;

            await ctx.Save();
            return existing;
        }

        public async Task DeleteItem(int id)
        {
            var existing = await ctx.Words.FindAsync(id);
            if (existing != null)
            {
                ctx.Words.Remove(existing);
                await ctx.Save();
            }
        }
    }
}