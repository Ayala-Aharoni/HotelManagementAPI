using AutoMapper;
using DataContext.DTO;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class CategoryService : ICategoryService
    {

        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> categoryRepository,IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;   
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAll();

            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetById(id);

            if (category == null)
                throw new EntityNotFoundException("קטגוריה", id);

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<Category> AddCategoryAsync(CategoryDTO dto)
        {
            var existing = (await _categoryRepository.GetAll())
                           .FirstOrDefault(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());

            // שימוש ב-EntityAlreadyExistsException שלך
            if (existing != null)
                throw new EntityAlreadyExistsException("קטגוריה", dto.CategoryName);

            // 2. שורת הקסם - המרה אוטומטית מ-DTO ל-Entity
            var category = _mapper.Map<Category>(dto);

            return await _categoryRepository.AddItem(category);
        }

        public async Task UpdateCategoryAsync(int id, CategoryDTO dto)
        {
            var category = await _categoryRepository.GetById(id);

            // שימוש ב-EntityNotFoundException שלך לפני עדכון
            if (category == null)
                throw new EntityNotFoundException("קטגוריה", id);

            category.CategoryName = dto.CategoryName;
            await _categoryRepository.UpdateItem(id, category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            //TODOOOOOOOOOOO
            //לעשות בדיקה אם יש בקשות שמשוייכות לקטגוריה לפני מחיקה, אם כן להחזיר שגיאה מתאימה
            //וכן עובדים שמשוייכים לקטגוריה, אם כן להחזיר שגיאה מתאימה  
            //זה אמור להיות עם שליחה לסרביס של האמפלוי לראות אם בקטוגריה הזו קיימים וכו...
            var category = await _categoryRepository.GetById(id);

            if (category == null)
                throw new EntityNotFoundException("קטגוריה", id);

            await _categoryRepository.DeleteItem(id);
        }
    }
}

        



