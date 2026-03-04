using AutoMapper;
using DataContext.DTO;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Mappings
{
    public class EmployeeProfile: Profile 
    {
        public EmployeeProfile()
        {
            // מיפוי מ-RegisterEmployeeDTO לישות Employee
            CreateMap<RegisterEmployeeDTO, Employee>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // נטפל בסיסמה ידנית כי היא עוברת Hashing
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // מיפוי מ-Employee ל-EmployeeDto (להחזרת נתונים)
            CreateMap<Employee, EmployeeDto>();
        }

    }
}
