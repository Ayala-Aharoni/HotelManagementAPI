using AutoMapper;
using BCrypt.Net;
using Common.DTO;
using DataContext.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Repository.Exception;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Service.Services
{
    public class EmployeeService: IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;     
        private readonly IConfiguration _configuration;



        public EmployeeService(IEmployeeRepository employeeRepository, IRepository<Category> categoryRepository, IConfiguration configuration ,IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _categoryRepository = categoryRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        //עשיתי כאן ללא ההרשאות שרק מנהל יכול לעשות זאת 
        public async Task<string> Register(RegisterEmployeeDTO R)
        {
            // 1. בדיקה אם המייל כבר תפוס
            var existingEmployee = await _employeeRepository.GetByEmailAsync(R.Email);
            if (existingEmployee != null)
            {
                throw new AppException.UserAlreadyExistsException();
            }

            // 2. בדיקה שהקטגוריה קיימת - שימוש בשליפה לפי ID
            // 2. בדיקה שהקטגוריה קיימת
            // טיפ: עדיף להשתמש ב-GetById ישיר ב-Repository אם יש לך, במקום להביא את כל הרשימה
            var category = (await _categoryRepository.GetAll())
                            .FirstOrDefault(c => c.CategoryId == R.CategoryId);

           
            if (category == null)
            {
                throw new AppException.NotFoundException("הקטגוריה שנבחרה לא נמצאה במערכת");
            }

            // 3. מיפוי והצפנה
            var employee = _mapper.Map<Employee>(R);
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(R.PassWord);
            employee.CategoryId = category.CategoryId;

            // 4. שמירה
            await _employeeRepository.AddItem(employee);

            // 5. יצירת טוקן
            return GenerateToken(employee);
        }        //עשיתי גאן עם זרואו זרקתי כאילו אקסשים האם ככה?
        public async Task<string> Login(LoginEmployeeDTO l)
        {

            var employee = await _employeeRepository.GetByEmailAsync(l.Email);

            if (employee == null || !BCrypt.Net.BCrypt.Verify(l.Password, employee.PasswordHash))
                throw new AppException.AuthException();



            var token = GenerateToken(employee);

           return token;


        }


        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
         
            var employees = await _employeeRepository.GetAll();

            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetById(id);

            if (employee == null)
                throw new EntityNotFoundException("עובד", id);

            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task UpdateEmployeeAsync(int id, Employee emp)
        {
            var existing = await _employeeRepository.GetById(id);
            if (existing == null)
                throw new EntityNotFoundException("עובד", id);

            //TODOO
            await _employeeRepository.UpdateItem(id, emp);
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            // בדיקה שהעובד קיים לפני מחיקה
            var existing = await _employeeRepository.GetById(id);
            if (existing == null)
                throw new EntityNotFoundException("עובד", id);

            await _employeeRepository.DeleteItem(id);
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
        new Claim(ClaimTypes.Role, e.Role),
        new Claim("CategoryId", e.CategoryId.ToString()),
        new Claim("CategoryName", e.Category?.CategoryName ?? "כללי")
    };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }





    }
}
