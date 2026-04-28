using Common;
using Common.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            // 1. וולידציה בסיסית - שלא ינסו לשלוח אובייקט ריק
            if (R == null || string.IsNullOrWhiteSpace(R.RoomNumber))
            {
                throw new AppException("חובה להזין מספר חדר תקין", HttpStatusCode.BadRequest);
            }

            // 2. חיפוש החדר - אם לא נמצא, זורקים 404 (NotFound)
            var room = await _roomRepository.GetByRoomNumberAsync(R.RoomNumber);

            if (room == null)
            {
                throw new AppException($"חדר מספר {R.RoomNumber} אינו רשום במערכת המלון", HttpStatusCode.NotFound);
            }

            // 3. עדכון הסטטוס ויצירת הטוקן
            try
            {
                room.IsTabletActive = true;

                await _roomRepository.UpdateItem(room.Id, room);

                // יצירת הטוקן (הזהות של הטאבלט מעכשיו והלאה)
                var token = GenerateToken(room);
                return token;
            }
            catch (Exception ex)
            {
                // כאן אנחנו תופסים שגיאות של בסיס נתונים וכדומה
                throw new AppException("שגיאה בתקשורת עם בסיס הנתונים", HttpStatusCode.ServiceUnavailable);
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