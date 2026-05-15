using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class RequestResponseDTO
    {
        public int RequestId { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; } // פשוט השם, בלי האובייקט המסובך
        public string Status { get; set; }      // המילה "New" במקום המספר 0
        public string EmployeeName { get; set; } // רק השם של העובד שטיפל
        public DateTime CreatedAt { get; set; }
        public string RoomNumber { get; set; } // השדה הקריטי שצריך להוסיף
    }
}
