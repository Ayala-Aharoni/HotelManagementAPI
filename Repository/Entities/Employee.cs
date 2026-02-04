using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;//שתיהם בשביל תקינות
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "שם מלא חובה")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "שם חייב להיות בין 2 ל-100 תווים")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "תפקיד חובה")]
        [StringLength(50)]
        public string Role { get; set; }

        [Required(ErrorMessage = "אימייל חובה")]
        [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
        public string Email { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Required(ErrorMessage = "סיסמה חובה")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$",
            ErrorMessage = "סיסמה חייבת להכיל לפחות 6 תווים, אות אחת ומספר אחד")]
        public string PasswordHash { get; set; }

        public bool IsAviable { get; set; }

        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
