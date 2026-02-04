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

        [StringLength(100, MinimumLength = 2)]
        public string Fullname { get; set; }

        [StringLength(50)]
        public string Role { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; } // EF navigation — nullable

        // store the hash here (validate the plain password in DTO/service)
        public string PasswordHash { get; set; }

        public bool IsAviable { get; set; }

        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
