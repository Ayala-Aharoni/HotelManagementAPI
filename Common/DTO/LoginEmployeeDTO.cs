
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DTO
{
    public class LoginEmployeeDTO
    {
        // הסרנו את Fullname כי הוא לא רלוונטי ללוגין
        public string Email { get; set; }
        public string Password { get; set; } // שימי לב: w קטנה
    }
}
