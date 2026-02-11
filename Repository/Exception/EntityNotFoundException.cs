using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Repository.Exception
{
    // מחלקה ראשונה: לטיפול במקרים שבהם משהו חסר
    public class EntityNotFoundException : AppException
    {
        public EntityNotFoundException(string entityName, object key)
            : base($"{entityName} עם המזהה {key} לא נמצא במערכת", HttpStatusCode.NotFound)
        {
        }
    }

    // מחלקה שנייה: לטיפול בכפילויות
    public class EntityAlreadyExistsException : AppException
    {
        public EntityAlreadyExistsException(string entityName, string detail)
            : base($"{entityName} עם הנתון '{detail}' כבר קיים במערכת", HttpStatusCode.Conflict)
        {
        }
    }
}