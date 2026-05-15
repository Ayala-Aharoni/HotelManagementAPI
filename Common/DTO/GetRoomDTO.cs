using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class GetRoomDTO
    {
       
            public int Id { get; set; } // בשביל הלוגיקה וה-React
            public string RoomNumber { get; set; } // בשביל התצוגה
            public bool IsTabletActive { get; set; } // בשביל הסטטוס
     }
  }

