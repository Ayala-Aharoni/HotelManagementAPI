using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Exception
{
    public class AppException :System.Exception
    {
        public HttpStatusCode StatusCode { get; }
        public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public class AuthException : AppException
        {
            public AuthException(string message = "אימייל או סיסמה שגוייםםםםםם!")
                : base(message, HttpStatusCode.Unauthorized) { }
        }

    }
}
