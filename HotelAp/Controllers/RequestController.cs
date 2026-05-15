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

        //[HttpGet("employee/{employeeId}")]
        //public async Task<ActionResult<IEnumerable<RequestResponseDTO>>> GetByEmployee(int employeeId)
        //{
        //    var items = await _requestService.GetRequestsByEmployee(employeeId);
        //    return Ok(items);
        //}
        //אלו הן 2 פונקציות שאני לא בטוחה שריך לעשות!!!!
        //לשאול משהי האם אלו דברים שאמורים להשמר בצד ריקאקט או שזה בסדר שהם יהיו פה

        [Authorize(Roles = "Employee")]
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<RequestResponseDTO>>> GetMyTasks()
        {
            // חילוץ ה-ID מהטוקן (Claims)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            int employeeId = int.Parse(userIdClaim);

            // שליחה ל-Service עם ה-ID האמיתי והמאובטח
            var tasks = await _requestService.GetRequestsByEmployee(employeeId);

            return Ok(tasks);
        }
        [Authorize(Roles = "Employee")]
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RequestResponseDTO>>> GetAvailableRequests()
        {
            // חילוץ הקטגוריה מהטוקן (שימי לב שהשם "CategoryId" חייב להיות זהה למה ששמת ב-Login)
            var categoryIdClaim = User.FindFirst("CategoryId")?.Value;
            if (categoryIdClaim == null) return BadRequest("Category ID not found in token");

            int categoryId = int.Parse(categoryIdClaim);

            // קריאה ל-Service שתביא רק בקשות בסטטוס NEW ובקטגוריה המתאימה
            var availableRequests = await _requestService.GetAvailableRequestsByCategory(categoryId);

            return Ok(availableRequests);
        }


     
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RequestDTO Req)
        {
            // 1. בדיקת תקינות בסיסית
            if (Req == null || string.IsNullOrWhiteSpace(Req.Description))
                return BadRequest("Description is required!");

            try
            {
                // 2. שליפת ה-RoomId מהטוקן (Claims)
                // User.FindFirst שולף את המידע שהוצפן בתוך ה-JWT
                var roomIdClaim = User.FindFirst("RoomId")?.Value;

                if (string.IsNullOrEmpty(roomIdClaim))
                {
                    return Unauthorized("מזהה חדר לא נמצא בטוקן - יש לבצע Setup מחדש");
                }

                int roomId = int.Parse(roomIdClaim);

                // 3. קריאה לסרוויס עם ה-ID המאובטח
                await _requestService.CreateRequest(Req, roomId);

                return Ok(new { Message = "הבקשה נוצרה בהצלחה", RoomId = roomId });
            }
            catch (Exception ex)
            {
                // רישום השגיאה (במציאות עדיף להשתמש ב-Logger)
                return BadRequest(new { Message = "שגיאה ביצירת הבקשה", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Employee")]
        [HttpPut("{id}/reassign-to-reception")]
        public async Task<IActionResult> ReassignToReception(int id)
        {
            try
            {
                // קריאה לפונקציה שכתבנו ב-Service
                await _requestService.ReassignToReception(id);
                return Ok(new { message = "Request reassigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/<RequestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {

        }


        [Authorize(Roles = "Employee")] //חשוב! מודא שיש לו תפקיד של עובד בטוקן כדי שיוכל לתפוס בקשות
        [HttpPost("take/{requestId}")]
        public async Task<IActionResult> TakeRequest(int requestId)
        {
            var userIdClaim = User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("לא נמצא מזהה עובד בטוקן");

            if (!int.TryParse(userIdClaim, out int employeeId))
                return Unauthorized("מזהה עובד לא תקין");

            bool isTaken = await _requestService.TakeRequest(requestId, employeeId);

            if (!isTaken)
                return BadRequest("הבקשה כבר נתפסה על ידי עובד אחר או שאינה קיימת.");

            return Ok(new { message = "הבקשה שויכה אליך בהצלחה" });
        }


        [Authorize(Roles = "Employee")] //חשוב! מודא שיש לו תפקיד של עובד בטוקן כדי שיוכל לתפוס בקשות
        [HttpPost("complete/{requestId}")]
        public async Task<IActionResult> CompleteRequest(int requestId)
        {
            // 1. חילוץ ה-ID מהטוקן
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) throw new Exception("משתמש לא מזוהה");

            // 2. קריאה ל-Service
            // אם זה לא העובד הנכון או שהסטטוס לא מתאים, תיזרק שגיאה
            await _requestService.CompleteRequest(requestId, int.Parse(userId));

            return Ok(new { Message = "הבקשה הושלמה בהצלחה" });
        }

        [Authorize(Roles = "Employee")] 
        [HttpPut("transfer/{requestId}/{correctCategoryId}")]
        public async Task<IActionResult> TransferRequestToCorrectCategory(int requestId, int correctCategoryId)
        {
            try
            {
                // קריאה לפונקציה שכתבת בסרוויס
                await _requestService.TransferRequestToCorrectCategory(requestId, correctCategoryId);

                return Ok(new { Message = $"הבקשה {requestId} הועברה בהצלחה לקטגוריה {correctCategoryId}" });
            }
            catch (Exception ex)
            {
                // במקרה שהבקשה לא נמצאה או שהקטגוריה לא קיימת
                return BadRequest(new { Message = "שגיאה בהעברת הבקשה", Details = ex.Message });
            }
        }


        // DELETE api/<RequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
