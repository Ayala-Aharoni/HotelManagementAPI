using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly Icontext ctx;
        public RequestRepository(Icontext context)
        {
            ctx = context;
        }
        public async Task<List<Request>> GetAll()
        {
            return await ctx.Requests
         .Include(r => r.Category)
         .Include(r => r.Employee)
         .Include(r => r.Room)
         .ToListAsync();
        }

        public async Task<Request?> GetById(int id)
        {
            return await ctx.Requests
        .Include(r => r.Category)
        .Include(r => r.Employee)
        .Include(r => r.Room)
        .FirstOrDefaultAsync(r => r.RequestId == id);
        }
        public async Task<Request> AddItem(Request item)
        {
            await ctx.Requests.AddAsync(item);
            await ctx.Save();
            return item;
        }

        public async Task<Request> UpdateItem(int id, Request item)
        {
            var existing = await ctx.Requests.FindAsync(id);
            if (existing == null) return null;
            existing.Description = item.Description;
            existing.Status = item.Status;
            existing.CategoryId = item.CategoryId;
            existing.EmployeeId = item.EmployeeId;
            existing.CreatedAt = item.CreatedAt;

            await ctx.Save();
            return existing;
        }

        public async Task<bool> TryAssignRequestAsync(int requestId, int employeeId)
        {
            var rowsAffected = await ctx.Requests
                .Where(r => r.RequestId == requestId && r.Status == RequestStatus.New)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, RequestStatus.InProgress)
                    .SetProperty(r => r.EmployeeId, employeeId));

            return rowsAffected > 0;
        }
        public async Task<bool> TryCompleteRequestAsync(int requestId, int employeeId)
        {
            var rowsAffected = await ctx.Requests
                .Where(r => r.RequestId == requestId && r.Status == RequestStatus.InProgress && r.EmployeeId == employeeId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, RequestStatus.Completed));

            return rowsAffected > 0;
        }

        public async Task DeleteItem(int id)
        {
            var existing = await ctx.Requests.FindAsync(id);
            if (existing != null)
            {
                ctx.Requests.Remove(existing);
                await ctx.Save();
            }
        }
        public async Task<IEnumerable<Request>> GetByEmployeeIdAsync(int employeeId)
        {
            return await ctx.Requests
                .Include(r => r.Room)
                .Where(r => r.EmployeeId == employeeId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Request>> GetAvailableByCategoryAsync(int categoryId)
        {
            return await ctx.Requests
                .Include(r => r.Room)
                .Where(r => r.Status == RequestStatus.New && r.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}
