using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public enum RequestStatus
    {
        New,        // חדש, עוד לא נעשה
        InProgress, // בתהליך
        Completed   // בוצע
    }

    public class Request
    {
        public int RequestId { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public RequestStatus Status { get; set; }
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }    
        public DateTime CreatedAt { get; set; } = DateTime.Now;

      
    }
}
