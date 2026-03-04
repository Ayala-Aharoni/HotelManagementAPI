using Common.DTO;
using DataContext.DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Repository.Interfaces
{
    public interface IEmployeeService
    {
        //זה כשזה גנרי שינתי בינתים לא לגנרי
        //Task<T> Register(RegisterEmployeeDTO R);
        //Task<T> Login(LoginEmployeeDTO l);
        //Task<IEnumerable<T>> GetAllEmployeesAsync();
        //Task<Employee> GetByIdAsync(int id);
        //Task UpdateEmployeeAsync(int id, Employee emp);
        //Task DeleteEmployeeAsync(int id);
      
            // כאן אנחנו מחזירים string כי זה הטוקן
            Task<string> Register(RegisterEmployeeDTO R);
            Task<string> Login(LoginEmployeeDTO l);

            // כאן מחזירים רשימה של ישויות (או DTO אם תחליטי למפות גם כאן)
            Task<IEnumerable<Employee>> GetAllEmployeesAsync();

            Task<Employee> GetByIdAsync(int id);
            Task UpdateEmployeeAsync(int id, Employee emp);
            Task DeleteEmployeeAsync(int id);
        


    }
}
