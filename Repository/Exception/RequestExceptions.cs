using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Exception
{
    public class RequestExceptions
    {
   
            // שגיאה כשמישהו מנסה לקחת משימה תפוסה
            public class RequestAlreadyAssigned : AppException
            {
                public RequestAlreadyAssigned()
                    : base("עובד אחר כבר לקח את המשימה הזו. הרשימה תתעדכן מיד.", HttpStatusCode.Conflict) { }
            }

            // שגיאה כשמנסים לסיים משימה שלא שייכת אליך
            public class RequestNotAssignedToYou : AppException
            {
                public RequestNotAssignedToYou()
                    : base("אינך יכול לבצע פעולה זו - המשימה אינה רשומה על שמך.", HttpStatusCode.Forbidden) { }
            }
        }
   }


