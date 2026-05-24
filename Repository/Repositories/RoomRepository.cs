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
        public async Task<Room> AddItem(Room item)
        {
            _ctx.Rooms.Add(item);
            await _ctx.Save();
            return item;
        }

        public async Task<Room> UpdateItem(int id, Room item)
        {
            var existingRoom = await GetById(id);
            if (existingRoom == null) return null;
            existingRoom.RoomNumber = item.RoomNumber;
            existingRoom.IsTabletActive = item.IsTabletActive;
            await _ctx.Save();
            return existingRoom;
        }
        public async Task DeleteItem(int id)
        {
            var room = await GetById(id);
            if (room != null)
            {
                _ctx.Rooms.Remove(room);
                await _ctx.Save();
            }
        }


        public async Task<Room> GetByRoomNumberAsync(string roomNumber)
        {
            try
            {
                // Fetch the room from the database, returns null if not found
                return await _ctx.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
            }
            catch (System.Exception ex)
            {
                // Log critical database/connection exceptions and rethrow
                Console.WriteLine($">>> [Repository] Critical DB Error: {ex.Message}");
                throw;
            }
        }
    }
}