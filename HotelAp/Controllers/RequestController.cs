using Common.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System.Security.Claims;
using Service.Interfaces;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        public RequestController(IRequestService requestService)
        {
            this._requestService = requestService;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestResponseDTO>>> GetAll()
        {
            var list = await _requestService.GetAll();
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestResponseDTO>> Get(int id)
        {
            var item = await _requestService.GetById(id);
            return Ok(item);
        }
        [Authorize(Roles = "Employee")]
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<RequestResponseDTO>>> GetMyTasks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");
            int employeeId = int.Parse(userIdClaim);
            var tasks = await _requestService.GetRequestsByEmployee(employeeId);
            return Ok(tasks);
        }
        [Authorize(Roles = "Employee")]
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RequestResponseDTO>>> GetAvailableRequests()
        {
            var categoryIdClaim = User.FindFirst("CategoryId")?.Value;
            if (categoryIdClaim == null) return BadRequest("Category ID not found in token");
            int categoryId = int.Parse(categoryIdClaim);
            var availableRequests = await _requestService.GetAvailableRequestsByCategory(categoryId);
            return Ok(availableRequests);
        }


     
        [Authorize]
        [HttpPost]
         public async Task<IActionResult> Post([FromBody] RequestDTO req)
         {
            var roomIdClaim = User.FindFirst("RoomId")?.Value;
            if (string.IsNullOrEmpty(roomIdClaim))
            return Unauthorized("מזהה חדר לא נמצא בטוקן - יש לבצע Setup מחדש");
            int roomId = int.Parse(roomIdClaim);
            await _requestService.CreateRequest(req, roomId);
            return Ok(new { Message = "הבקשה נוצרה בהצלחה", RoomId = roomId });
         }
        [Authorize(Roles = "Employee")]
        [HttpPut("{id}/reassign-to-reception")]
        public async Task<IActionResult> ReassignToReception(int id)
        {
                await _requestService.ReassignToReception(id);
                return Ok(new { message = "Request reassigned successfully" });
        }
        [Authorize(Roles = "Employee")]
        [HttpPost("take/{requestId}")]
        public async Task<IActionResult> TakeRequest(int requestId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("לא נמצא מזהה עובד בטוקן");
            int employeeId = int.Parse(userIdClaim);
            await _requestService.TakeRequest(requestId, employeeId);
            return Ok(new { message = "הבקשה שויכה אליך בהצלחה" });
        }
        [Authorize(Roles = "Employee")] 
        [HttpPost("complete/{requestId}")]
        public async Task<IActionResult> CompleteRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) throw new Exception("משתמש לא מזוהה");
            await _requestService.CompleteRequest(requestId, int.Parse(userId));
            return Ok(new { Message = "הבקשה הושלמה בהצלחה" });
        }
        [Authorize(Roles = "Employee")] 
        [HttpPut("transfer/{requestId}/{correctCategoryId}")]
        public async Task<IActionResult> TransferRequestToCorrectCategory(int requestId, int correctCategoryId)
        {
                await _requestService.TransferRequestToCorrectCategory(requestId, correctCategoryId);
                return Ok(new { Message = $"הבקשה {requestId} הועברה בהצלחה לקטגוריה {correctCategoryId}" });
        }
    }
}
