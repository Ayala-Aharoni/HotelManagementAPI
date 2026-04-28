using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DTO
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
    
        public string Fullname { get; set; }    

        public string Email { get; set; }
        public string Role { get; set; } // מנהל/עובד וכו'
        public int? CategoryId { get; set; } // אם הוא שייך למחלקה מסוימת
        public string CategoryName { get; set; }
        public bool IsAviable { get; set; }

    }
}
