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
    public class CategoryRepository : IRepository<Category>
    {
        private readonly Icontext ctx;
        public CategoryRepository(Icontext context)
        {

            ctx = context;
        }

        public async Task<List<Category>> GetAll()
        {
            return await ctx.Categories.ToListAsync();
        }

        public async Task<Category> GetById(int id)
        {
            return await ctx.Categories.FindAsync(id);
        }

        public async Task<Category> AddItem(Category item)
        {
            await ctx.Categories.AddAsync(item);
            await ctx.Save();
            return item;
        }

        public async Task<Category> UpdateItem(int id, Category item)
        {
            var existing = await ctx.Categories.FindAsync(id);
            if (existing == null) return null;

            existing.CategoryName = item.CategoryName;

            await ctx.Save();
            return existing;
        }

        public async Task DeleteItem(int id)
        {
            var existing = await ctx.Categories.FindAsync(id);
            if (existing != null)
            {
                ctx.Categories.Remove(existing);
                await ctx.Save();
            }
        }
    }
}