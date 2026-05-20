using DataContext.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class UpDateemploeeDTO
    {
        [Required(ErrorMessage = "שם מלא חובה")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "שם חייב להיות בין 2 ל-100 תווים")]
        public string Fullname { get; set; }

        [Required]
        [EnumDataType(typeof(EmployeeRole))]
        public EmployeeRole Role { get; set; }

        [Required(ErrorMessage = "אימייל חובה")]
        [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
        public string Email { get; set; }

       
        // 2. הוספנו ? אחרי string כדי לאפשר לסיסמה להיות null בעדכון
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$",
            ErrorMessage = "סיסמה חייבת להכיל לפחות 6 תווים, אות אחת ומספר אחד")]
        public string? PassWord { get; set; }

        [Required(ErrorMessage = "קטגוריה חובה")]
        public int CategoryId { get; set; }
    }
}
