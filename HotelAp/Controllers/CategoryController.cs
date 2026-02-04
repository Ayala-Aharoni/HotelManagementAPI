using DataContext.DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;
using Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> repository;
        private readonly CategoryService _categoryService;
        public CategoryController(IRepository<Category> repo, CategoryService categoryService)
        {
            repository = repo;
            _categoryService = categoryService;
        }   

        // GET: api/<CategoryController>
        [HttpGet]
        // [Authorize(Roles = "Admin")]//  פה רק מנהל יוכל למחוק עובד עשיתי בינתים ירוק נא לשנותת!!!!!!! 

        public async Task<IEnumerable<Category>> Get()
        {
            return await repository.GetAll();
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<Category> Get(int id)
        {
            return await repository.GetById(id);
        }


        // POST api/<CategoryController>
        [HttpPost]
        [HttpPost("AddCategory")]
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


        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public async void Delete(int id)
        {
            repository.DeleteItem(id);  
        }
    }
}
