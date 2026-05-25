using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Room
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string RoomNumber { get; set; } // מספר החדר (למשל "101")

        [Required]
        public bool IsTabletActive { get; set; } = false; // האם הטאבלט הופעל בחדר?

        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}

