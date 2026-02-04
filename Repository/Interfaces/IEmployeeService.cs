using Common.DTO;
using DataContext.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Repository.Interfaces
{
    public interface IEmployeeService<T>
    {
        Task<T> Register(RegisterEmployeeDTO R);
        Task<T> Login (LoginEmployeeDTO l);

    }
}
