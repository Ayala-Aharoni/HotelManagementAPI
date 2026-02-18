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


        // GET: api/<RequestController>
        [HttpGet]
        public async Task<IEnumerable<Request>> GetAll()
        {
            return await _requestService.GetAll();
        }

        // GET api/<RequestController>/5
        [HttpGet("{id}")]
        public async Task<Request> Get(int id)
        {
            return await _requestService.GetById(id);
        }

        // POST api/<RequestController>
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RequestDTO Req)
        {
            if (Req == null || string.IsNullOrWhiteSpace(Req.Description))
                return BadRequest("Description is required!");

             await _requestService.CreateRequest(Req);

            return Ok(); // מחזיר JSON אוטומטית
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



        // DELETE api/<RequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
