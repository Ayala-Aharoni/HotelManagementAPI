using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class RoomDTO
    {
        [Required(ErrorMessage = "מספר חדר חובה")]
        public string RoomNumber { get; set; }
        [Required(ErrorMessage = "אימייל עובד חובה  ")]

        public string AdminEmail { get; set; }

        [Required(ErrorMessage = "סיסמת עובד חובה ")]
        public string AdminPassword { get; set; }
    }
}
