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

        public async Task<AddRoomDTO> AddRoomAsync(AddRoomDTO roomDto)
        {
            // 1. וולידציה בסיסית
            if (string.IsNullOrWhiteSpace(roomDto.RoomNumber))
            {
                throw new AppException("מספר חדר הוא שדה חובה", HttpStatusCode.BadRequest);
            }

            // 2. בדיקה האם החדר כבר קיים במערכת
            var existingRoom = await _roomRepository.GetByRoomNumberAsync(roomDto.RoomNumber);
            if (existingRoom != null)
            {
                throw new AppException($"חדר מספר {roomDto.RoomNumber} כבר קיים במערכת", HttpStatusCode.Conflict);
            }

            // 3. מיפוי מ-DTO לישות (Entity)
            var roomEntity = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                IsTabletActive = false // חדר חדש מגיע כברירת מחדל לא פעיל
            };

            // 4. קריאה לרפוסיטורי לשמירה
            var createdRoom = await _roomRepository.AddItem(roomEntity);

            // 5. החזרת DTO חזרה לקונטרולר
            return new AddRoomDTO
            {
               
                RoomNumber = createdRoom.RoomNumber
            };
        }

        public async Task<IEnumerable<GetRoomDTO>> GetAllRoomsAsync()
        {
            // שליפת החדרים מהרפוזיטורי
            var rooms = await _roomRepository.GetAll();

            // המרה (Mapping) ל-DTO החדש שיצרת
            return rooms.Select(r => new GetRoomDTO
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                IsTabletActive = r.IsTabletActive // וודאי שקיים שדה כזה ב-Entity שלך
            }).ToList();
        }
        public async Task<string> SetupRoomAsync(RoomDTO R)
        {
            // 1. וולידציה בסיסית - בדיקה שכל השדות הגיעו
            if (R == null || string.IsNullOrWhiteSpace(R.RoomNumber) ||
                string.IsNullOrWhiteSpace(R.AdminEmail) || string.IsNullOrWhiteSpace(R.AdminPassword))
            {
                throw new AppException("חובה להזין מספר חדר ופרטי מנהל תקינים", HttpStatusCode.BadRequest);
            }

            // 2. אימות המנהל - כאן אנחנו "חוזרים" על קוד הלוגין בצורה ישירה
            var admin = await _employeeRepository.GetByEmailAsync(R.AdminEmail);

            // בדיקה שהמשתמש קיים, שהוא מנהל ושהסיסמה נכונה
            if (/*admin == null || admin.Role != "Admin" ||*/ !BCrypt.Net.BCrypt.Verify(R.AdminPassword, admin.PasswordHash))
            {
                throw new AppException("אימות  נכשל - אין הרשאה לבצע הגדרת טאבלט", HttpStatusCode.Unauthorized);
            }

            // 3. חיפוש החדר במערכת
            var room = await _roomRepository.GetByRoomNumberAsync(R.RoomNumber);

            if (room == null)
            {
                throw new AppException($"חדר מספר {R.RoomNumber} אינו רשום במערכת המלון", HttpStatusCode.NotFound);
            }

            if (room.IsTabletActive)
            {
                throw new AppException($"חדר מספר {R.RoomNumber} כבר מחובר לטאבלט פעיל. יש לנתק את הטאבלט הקודם לפני חיבור חדש.", HttpStatusCode.Conflict);
            }
            // 4. עדכון הסטטוס ויצירת הטוקן
            try
            {
                room.IsTabletActive = true;
                await _roomRepository.UpdateItem(room.Id, room);

                // יצירת הטוקן המוגבל לחדר (הזהות של הטאבלט)
                // כאן תוודאי שפונקציית GenerateToken יודעת לקבל אובייקט Room 
                // ולהכניס את מספר החדר לתוך ה-Claims
                var token = GenerateToken(room);
                return token;
            }
            catch (Exception ex)
            {
                // במקום הודעה כללית, נשלח את ההודעה של ה-Exception המקורי
                throw new AppException($"שגיאה בשרת: {ex.Message}", HttpStatusCode.InternalServerError);
            }
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