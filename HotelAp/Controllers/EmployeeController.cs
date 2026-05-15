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

     
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> Get()
        {
            // הסרוויס כבר מחזיר רשימת DTOs, אז פשוט מעבירים אותה הלאה
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> Get(int id)
        {
            // אם העובד לא קיים, הסרוויס יזרוק Exception והמערכת תחזיר 404 אוטומטית
            var employee = await _employeeService.GetByIdAsync(id);
            return Ok(employee);
        }
        
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RegisterEmployeeDTO emp)
        {
            await _employeeService.UpdateEmployeeAsync(id, emp);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Employee")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] EmployeeStatusUpdateDto dto)
        {
            // סתם בשביל הבדיקה שלך, לראות שזה מגיע
            Console.WriteLine($"Updating status for employee {id} to {dto.ISAviavle}");

            try
            {
                // שימי לב: אנחנו שולחים ל-Service את dto.IsAvailable 
                // כי ה-Service עדיין מצפה לקבל בוליאני פשוט
                await _employeeService.UpdateAvailabilityAsync(id, dto.ISAviavle);

                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}   