using Common.DTO;
using DataContext.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IRepository<Employee> repository;
        private readonly IEmployeeService<AuthResponseDTO> employeeService;

        public EmployeeController(IRepository<Employee> repo, IEmployeeService<AuthResponseDTO> employeeService)
        {
            repository = repo;
            this.employeeService = employeeService;
        }
        // GET: api/<EmployeeController>
        [HttpGet]
        public async Task<IEnumerable<Employee>> Get()
        {
            return await repository.GetAll();
        }


        // GET api/<EmployeeController>/5

        //[Authorize(Roles = "Admin")] זה כאילו עשיתי בינתים אני רוצה שלהציג את כל העובדים רק המנהל יוכל
        [HttpGet("{id}")]
        public async Task<Employee> Get(int id)
        {
            return await repository.GetById(id);
        }


        // POST api/<EmployeeController>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterEmployeeDTO dto)
        {
            var employee = await employeeService.Register(dto);
            return Ok(employee);
        }

        // POST api/<EmployeeController>/login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginEmployeeDTO dto)
        {
            try
            {
                var employee = await employeeService.Login(dto);
                return Ok(employee); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // PUT api/<EmployeeController>/5
        [HttpPut("{id}")]
        public async void Put(int id, [FromBody] Employee Emp)
        {
            repository.UpdateItem(id,Emp);

        }

        // DELETE api/<EmployeeController>/5
        [HttpDelete("{id}")]
        public async void Delete(int id)
        {
            repository.DeleteItem(id);
        }
    }
}
