using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Repository.Interfaces;
using Common.DTO;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Common;
using Service.Interfaces;

namespace Service.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IConfiguration _configuration;

        public RoomService(IRoomRepository roomRepository, IConfiguration configuration)
        {
            _roomRepository = roomRepository;
            _configuration = configuration;
        }

        public async Task<string> SetupRoomAsync(RoomDTO R)
        {
            Console.WriteLine(">>> [RoomService] מתחיל SetupRoomAsync");

            if (R == null)
            {
                Console.WriteLine(">>> [RoomService] שגיאה: ה-DTO שהתקבל הוא NULL");
                throw new Exception("נתוני חדר חסרים");
            }

            Console.WriteLine($">>> [RoomService] מחפש ב-DB חדר מספר: {R.RoomNumber}");

            // 1. חיפוש החדר
            var room = await _roomRepository.GetByRoomNumberAsync(R.RoomNumber);

            if (room == null)
            {
                Console.WriteLine($">>> [RoomService] שגיאה: חדר {R.RoomNumber} לא נמצא בבסיס הנתונים!");
                throw new Exception("החדר לא קיים במערכת");
            }

            Console.WriteLine($">>> [RoomService] חדר נמצא בהצלחה. ID: {room.Id}. מעדכן סטטוס טאבלט...");

            try
            {
                // 2. עדכון סטטוס
                room.IsTabletActive = true;
                await _roomRepository.UpdateItem(room.Id, room);
                Console.WriteLine($">>> [RoomService] סטטוס עודכן ל-Active. עובר ליצירת טוקן...");

                // 3. יצירת הטוקן
                var token = GenerateToken(room);
                Console.WriteLine(">>> [RoomService] טוקן נוצר בהצלחה ומצולם חזרה לקונטרולר.");
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> [RoomService] קריסה בזמן עדכון או יצירת טוקן: {ex.Message}");
                throw;
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