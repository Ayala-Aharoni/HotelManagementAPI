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
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Request> _requestRepository;

        public CategoryService(IRepository<Category> categoryRepository,IMapper mapper , IRepository<Employee> employeeRepository , IRepository<Request> requestRepository)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _requestRepository = requestRepository;
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
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "נתוני הקטגוריה לא התקבלו.");
            }
            var existing = (await _categoryRepository.GetAll())
                           .FirstOrDefault(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());
            if (existing != null)
            {
                throw new EntityAlreadyExistsException("קטגוריה", dto.CategoryName);
            }
            var category = _mapper.Map<Category>(dto);
            return await _categoryRepository.AddItem(category);
        }
        public async Task UpdateCategoryAsync(int id, CategoryDTO dto)
        {
            var category = await _categoryRepository.GetById(id);
            if (category == null)
                throw new EntityNotFoundException("קטגוריה", id);
            category.CategoryName = dto.CategoryName;
            await _categoryRepository.UpdateItem(id, category);
        }
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetById(id);
            if (category == null)
            {
                throw new EntityNotFoundException("קטגוריה", id);
            }
            var allRequests = await _requestRepository.GetAll();
            var hasRequests = allRequests.Any(r => r.CategoryId == id);
            if (hasRequests)
            {
                throw new Exception("לא ניתן למחוק את הקטגוריה כיוון שישנן בקשות המשויכות אליה.");
            }
            var allEmployees = await _employeeRepository.GetAll();
            var hasEmployees = allEmployees.Any(e => e.CategoryId == id);
            if (hasEmployees)
            {
                throw new Exception("לא ניתן למחוק את הקטגוריה כיוון שישנם עובדים המשויכים אליה.");
            }
            await _categoryRepository.DeleteItem(id);
        }
    }
}

        



