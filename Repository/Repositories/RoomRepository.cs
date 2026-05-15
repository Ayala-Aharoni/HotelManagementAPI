using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class RoomRepositoryv : IRoomRepository
    {
        private readonly Icontext _ctx;

        public RoomRepositoryv(Icontext context)
        {
            _ctx = context;
        }

        public async Task<List<Room>> GetAll()
        {
            return await _ctx.Rooms.ToListAsync();
        }
        public async Task<Room> GetById(int id)
        {
            return await _ctx.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        }

        // 3. הוספת חדר חדש
        public async Task<Room> AddItem(Room item)
        {
            _ctx.Rooms.Add(item);
            await _ctx.Save();
            return item;
        }

        // 4. עדכון חדר קיים
        public async Task<Room> UpdateItem(int id, Room item)
        {
            var existingRoom = await GetById(id);
            if (existingRoom == null) return null;

            // עדכון השדות
            existingRoom.RoomNumber = item.RoomNumber;
            existingRoom.IsTabletActive = item.IsTabletActive;
            await _ctx.Save();
            return existingRoom;
        }

        // 5. מחיקת חדר
        public async Task DeleteItem(int id)
        {
            var room = await GetById(id);
            if (room != null)
            {
                _ctx.Rooms.Remove(room);
                await _ctx.Save();
            }
        }

        // 6. הפונקציה המיוחדת שאנחנו צריכים בשביל ה-Setup!
        // בתוך RoomRepository.cs
        public async Task<Room> GetByRoomNumberAsync(string roomNumber)
        {
            Console.WriteLine($">>> [Repository] נכנסתי לפונקציה, מחפש את: {roomNumber}");
            try
            {
                var room = await _ctx.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
                Console.WriteLine($">>> [Repository] הפעולה ב-DB הסתיימה. האם נמצא חדר? {room != null}");
                return room;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($">>> [Repository] שגיאה קריטית בגישה ל-DB: {ex.Message}");
                throw;
            }
        }
    }
}