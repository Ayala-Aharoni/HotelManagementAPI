using Common;
using Common.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;   
namespace Service.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmployeeRepository _employeeRepository;

        public RoomService(IRoomRepository roomRepository, IConfiguration configuration, IEmployeeRepository employeeRepository )
        {
            _roomRepository = roomRepository;
            _configuration = configuration;
            _employeeRepository = employeeRepository;
        }

        public async Task<GetRoomDTO> AddRoomAsync(AddRoomDTO roomDto)
        {
            // 1. נוודא שלא שלחו לנו בטעות חדר בלי מספר
            if (string.IsNullOrWhiteSpace(roomDto.RoomNumber))
            {
                throw new AppException("מספר החדר אינו יכול להיות ריק", HttpStatusCode.BadRequest);
            }

            // 2. בדיקה אם החדר כבר קיים
            var existingRoom = await _roomRepository.GetByRoomNumberAsync(roomDto.RoomNumber);
            if (existingRoom != null)
            {
                throw new AppException($"חדר מספר {roomDto.RoomNumber} כבר קיים במערכת", HttpStatusCode.Conflict);
            }

            // 3. יצירת האובייקט
            var roomEntity = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                IsTabletActive = false
            };

            // 4. שמירה במסד הנתונים (הפונקציה AddItem מעדכנת את ה-Id בתוך roomEntity)
            var createdRoom = await _roomRepository.AddItem(roomEntity);

            // 5. החזרת DTO מלא שכולל את ה-Id שנוצר!
            return new GetRoomDTO
            {
                Id = createdRoom.Id,
                RoomNumber = createdRoom.RoomNumber,
                IsTabletActive = createdRoom.IsTabletActive
            };
        }
        public async Task<IEnumerable<GetRoomDTO>> GetAllRoomsAsync()
        {
            var rooms = await _roomRepository.GetAll();
            return rooms.Select(r => new GetRoomDTO
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                IsTabletActive = r.IsTabletActive
            }).ToList();
        }
        public async Task<string> SetupRoomAsync(RoomDTO setupDto)
        {
            // 1. בדיקת קלט בסיסית
            if (setupDto == null)
            {
                throw new AppException("נתוני הגדרת החדר לא התקבלו.", HttpStatusCode.BadRequest);
            }

            // 2. אימות זהות (כל עובד רשום יכול לבצע את הפעולה)
            var employee = await _employeeRepository.GetByEmailAsync(setupDto.AdminEmail);
            if (employee == null || !BCrypt.Net.BCrypt.Verify(setupDto.AdminPassword, employee.PasswordHash))
            {
                throw new AppException("אימות נכשל - דוא\"ל או סיסמה שגויים.", HttpStatusCode.Unauthorized);
            }

            // 3. מציאת החדר
            var room = await _roomRepository.GetByRoomNumberAsync(setupDto.RoomNumber);
            if (room == null)
            {
                throw new EntityNotFoundException("חדר", setupDto.RoomNumber);
            }

            // 4. בדיקה שאין טאבלט כפול
            if (room.IsTabletActive)
            {
                throw new AppException($"חדר מספר {setupDto.RoomNumber} כבר מחובר לטאבלט פעיל. יש לנתק את הטאבלט הקודם לפני חיבור חדש.", HttpStatusCode.Conflict);
            }

            // 5. שמירת השינוי ויצירת טוקן
            room.IsTabletActive = true;
            await _roomRepository.UpdateItem(room.Id, room);

            var token = GenerateToken(room);
            return token;
        }
        private string GenerateToken(Room room)
        {
            try
            {
                Console.WriteLine(">>> [GenerateToken] מושך הגדרות JWT מה-Configuration...");

                var keyString = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(keyString))
                {
                    Console.WriteLine(">>> [GenerateToken] שגיאה קריטית: ה-Key של ה-JWT חסר ב-appsettings.json!");
                    throw new Exception("JWT Key is missing");
                }

                var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

                var claims = new[]
              {
    // המפתח הזה חייב להיות RoomId כי זה מה שהקונטרולר מחפש!
    new Claim("RoomId", room.Id.ToString()), 
    
    // נוסיף גם את אלו ליתר ביטחון (בשביל העיצוב שעשינו מקודם)
    new Claim("RoomNumber", room.RoomNumber ?? ""),
    new Claim(ClaimTypes.Name, room.RoomNumber ?? ""),
    new Claim(ClaimTypes.Role, "RoomTablet")
};

                Console.WriteLine(">>> [GenerateToken] בונה אובייקט JwtSecurityToken...");

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.Now.AddYears(1), // טאבלט לא צריך להתנתק מהר
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> [GenerateToken] שגיאה בתהליך יצירת ה-JWT: {ex.Message}");
                throw;
            }
        }
    }
}