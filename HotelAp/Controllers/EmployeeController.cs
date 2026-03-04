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
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]

        public async Task<IActionResult> Get()
        {
            // כאן כדאי להוסיף מתודה בסרביס GetAllEmployees
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

       // [Authorize(Roles = "Admin")]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterEmployeeDTO dto)
        {
            try
            {
                var token = await _employeeService.Register(dto);
                // אנחנו מחזירים אובייקט עם שדה טוקן כדי שהפרונט יבין בקלות
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginEmployeeDTO dto)
        {
            try
            {
                var token = await _employeeService.Login(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Employee emp)
        {
            await _employeeService.UpdateEmployeeAsync(id, emp);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
    }
}   