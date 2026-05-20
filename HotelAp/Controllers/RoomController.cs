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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRoom([FromBody] AddRoomDTO roomDto)
    {
            var newRoom = await _roomService.AddRoomAsync(roomDto);
            return Ok(newRoom);
    }
    [HttpGet]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> GetAllRooms()
    {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms); 
    }
    [HttpPost("setup")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Setup([FromBody] RoomDTO setupDto)
    {
              var token = await _roomService.SetupRoomAsync(setupDto);
              return Ok(new { Token = token });
    }

  }

