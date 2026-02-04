using BCrypt.Net;
using Common.DTO;
using DataContext.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Service
{
    public class EmployeeService: IEmployeeService<AuthResponseDTO>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IConfiguration _configuration;


        public EmployeeService(IEmployeeRepository employeeRepository, IRepository<Category> categoryRepository, IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _categoryRepository = categoryRepository;
            _configuration = configuration;
        }

        //עשיתי כאן ללא ההרשאות שרק מנהל יכול לעשות זאת 
        public async Task<AuthResponseDTO> Register(RegisterEmployeeDTO R)
        {
            var existingEmployee = await _employeeRepository.GetByEmailAsync(R.Email);

            if (existingEmployee != null)
            {
                // זריקת שגיאה ברורה שתחזור ללקוח
                throw new Exception("משתמש עם אימייל זה כבר קיים במערכת");
            }


            var category = (await _categoryRepository.GetAll())
                            .FirstOrDefault(c => c.CategoryId == R.CategoryId);

            if (category == null)
                throw new Exception("קטגוריה לא נמצאה");

            var employee = new Employee
            {
                Fullname =R.Fullname,
                Email = R.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(R.PassWord),
                Role = R.Role,
                CategoryId = category.CategoryId
            };
            await _employeeRepository.AddItem(employee);
           
            var token = GenerateToken(employee); //האם אני צריכה כאן גם ליצור טוקן??

            // החזרת DTO
            return new AuthResponseDTO
            {
                Token = token,
                Name = employee.Fullname,
                Email = employee.Email,
                Role = employee.Role
            };

        }
        //עשיתי גאן עם זרואו זרקתי כאילו אקסשים האם ככה?
        public async Task<AuthResponseDTO> Login(LoginEmployeeDTO l)
        {

            var employee = await _employeeRepository.GetByEmailAsync(l.Email);

            if (employee == null || !BCrypt.Net.BCrypt.Verify(l.Password, employee.PasswordHash))
                throw new Exception("אימייל או סיסמה שגויים");



            var token = GenerateToken(employee);

            // החזרת DTO
            return new AuthResponseDTO
            {
                Token = token,
                Name = employee.Fullname,
                Email = employee.Email,
                Role = employee.Role
            };

        }

        private string GenerateToken(Employee e)
        {
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, e.EmployeeId.ToString()),
        new Claim(ClaimTypes.Name, e.Fullname),
        new Claim(ClaimTypes.Email, e.Email),
        new Claim(ClaimTypes.Role, e.Role)
    };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }





    }
}
