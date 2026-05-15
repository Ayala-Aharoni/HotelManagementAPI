using DataContext.DTO;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> Get()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories); 
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> Get(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(category);
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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