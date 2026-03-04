using DataContext.DTO;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);

        Task<Category> AddCategoryAsync(CategoryDTO dto);

        Task UpdateCategoryAsync(int id, CategoryDTO dto);

        Task DeleteCategoryAsync(int id);
    }
}
