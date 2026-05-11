using DataContext.DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly Icontext ctx;
        public EmployeeRepository(Icontext context)
        {
            ctx = context;
        }

        public async Task<List<Employee>> GetAll()
        {
            return await ctx.Employees
         .Include(e => e.Category)
         .ToListAsync();
        }

        public async Task<Employee?> GetById(int id)
        {
            return await ctx.Employees.FindAsync(id);
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await ctx.Employees.FirstOrDefaultAsync(e => e.Email == email);
        }


        public async Task<Employee> AddItem(Employee item)
        {
            await ctx.Employees.AddAsync(item);
            await ctx.Save();
            return item;
        }

        public async Task<Employee> UpdateItem(int id, Employee item)
        {
            var existing = await ctx.Employees.FindAsync(id);
            if (existing == null) return null;

            existing.Fullname = item.Fullname;
            existing.Email = item.Email;
            existing.Role = item.Role.ToString();
         //   existing.IsAviable = item.IsAviable;

            await ctx.Save();
            return existing;
        }

        public async Task DeleteItem(int id)
        {
            var existing = await ctx.Employees.FindAsync(id);
            if (existing != null)
            {
                ctx.Employees.Remove(existing);
                await ctx.Save();
            }
        }
        public async Task UpdateAvailabilityAsync(int id, bool isAvailable)
        {
            var employee = await ctx.Employees.FindAsync(id);
            if (employee == null)
                throw new EntityNotFoundException("Employee", id);
            employee.IsAviable = isAvailable; 
            await ctx.Save  ();
        }
    }
}

