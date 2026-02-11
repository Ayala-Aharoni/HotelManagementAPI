using DataContext.DTO;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class CategoryService
    {

        private readonly IRepository<Category> _categoryRepository;
        public CategoryService(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> AddCategoryAsync(CategoryDTO dto)
        {
            var existing = (await _categoryRepository.GetAll())
                           .FirstOrDefault(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());

            if (existing != null)
                throw new EntityAlreadyExistsException("קטגוריה", dto.CategoryName);
            var category = new Category
            {
                CategoryName = dto.CategoryName
            };

            return await _categoryRepository.AddItem(category);
        }
    }
}
        



