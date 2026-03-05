using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class NotificationDTO
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = "בקשה חדשה הגיעה!";
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ActionType { get; set; } = "NewRequest"; // עוזר לריאקט לדעת מה לעשות
    }
}
