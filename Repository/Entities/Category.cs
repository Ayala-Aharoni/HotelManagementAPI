using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
namespace Repository.Entities 
{ public class Category
    { public int CategoryId { get; set; }
        public string CategoryName { get; set; } 
        public ICollection<Request> Requests { get; set; } = new List<Request>(); 
        public ICollection<CategoryWord> CategoryWords { get; set; } = new List<CategoryWord>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>(); } 
}