using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DTO
{
    public class RegisterEmployeeDTO
    {
        public string Fullname { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public  string PassWord { get; set; }
        public int CategoryId { get; set; }
        

    }
}
