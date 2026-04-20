using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using Common.DTO;
using Common;
using Repository.Interfaces;    
namespace HotelAp.Controllers;
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

