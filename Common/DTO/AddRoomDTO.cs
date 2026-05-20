using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class AddRoomDTO

    {
        [Required(ErrorMessage = "מספר חדר הוא שדה חובה")]
        public string RoomNumber { get; set; } // מספר החדר (למשל "101", "A2")

    }
}
