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
        public string RoomNumber { get; set; } // מספר החדר (למשל "101" או "סוויטה 5")

        [Required]
        public string TabletIpAddress { get; set; } // ה-IP הייחודי של הטאבלט באותו חדר

        // קשר לבקשות - חדר אחד יכול שיהיו לו הרבה בקשות

        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
