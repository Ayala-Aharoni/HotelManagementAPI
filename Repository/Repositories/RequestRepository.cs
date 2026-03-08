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
            // אנחנו מוסיפים Include כדי ש-EF יבצע JOIN בבסיס הנתונים ויביא את השמות
            return await ctx.Requests
                .Include(r => r.Category)
                .Include(r => r.Employee)
                .ToListAsync();
        }

        public async Task<Request?> GetById(int id)
        {
            // ב-FindAsync אי אפשר להשתמש ב-Include, אז עוברים ל-FirstOrDefaultAsync
            return await ctx.Requests
                .Include(r => r.Category)
                .Include(r => r.Employee)
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

        //זו פונקציה שמנסה לעדכן ת הבקשה לסטטוס בטיפול ואת העובד שרוצה אותה 
        public async Task<bool> TryAssignRequestAsync(int requestId, int employeeId)
        {
            var rowsAffected = await ctx.Requests
                .Where(r => r.RequestId == requestId && r.Status == RequestStatus.New)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, RequestStatus.InProgress)
                    .SetProperty(r => r.EmployeeId, employeeId));

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
    }
}
