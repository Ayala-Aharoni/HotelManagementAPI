using DataContext.DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using Service.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IEnumerable<Category>> Get()
        {
            // שימי לב: השם חייב להיות זהה למה שיש ב-ICategoryService
            return await _categoryService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("AddCategory")] // עדיף להשאיר רק נתיב אחד כדי למנוע בלבול
        public async Task<IActionResult> AddCategory([FromBody] CategoryDTO dto)
        {
            try
            {
                var category = await _categoryService.AddCategoryAsync(dto);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // כאן השם השתנה ל-DeleteCategoryAsync
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent(); // מחזיר 204 (הצליח ואין תוכן להחזיר)
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}