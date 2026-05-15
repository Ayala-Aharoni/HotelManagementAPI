using Common;
using Common.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;    
using Service.Services;
namespace HotelAp.Controllers;
using Microsoft.AspNetCore.Authorization;

using Repository.Exception;
using Service.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
   
    public class RoomController : ControllerBase
    {

        private readonly IRoomService _roomService; 
        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }
    [HttpPost]
    [Authorize(Roles = "Admin")] // הגנה על הנתיב כך שרק אדמין יוכל לגשת
    public async Task<IActionResult> AddRoom([FromBody] AddRoomDTO roomDto)
    {
        try
        {
            var newRoom = await _roomService.AddRoomAsync(roomDto);
            return Ok(newRoom);
        }
        catch (AppException ex)
        {
            return StatusCode((int)ex.StatusCode, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
    // הוסיפי את זה בתוך ה-RoomController
    [HttpGet]
    [Authorize(Roles = "Admin")] // הגנה על הנתיב כך שרק אדמין יוכל לגשת
    public async Task<IActionResult> GetAllRooms()
    {
        try
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms); // יחזיר קוד 200 עם רשימת החדרים
        }
        catch (Exception ex)
        {
            // טיפול בשגיאה כללית
            return StatusCode(500, new { Message = "שגיאה בשרת בעת שליפת החדרים" });
        }
    }




    [HttpPost("setup")]
        public async Task<IActionResult> Setup([FromBody] RoomDTO setupDto)
        {
            try
            {
                // אנחנו שולחים לסרוויס רק את המספר מתוך ה-DTO
                var token = await _roomService.SetupRoomAsync(setupDto);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }

