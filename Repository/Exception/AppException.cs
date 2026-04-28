using System.Net;

namespace Repository.Exception
{
    public class AppException : System.Exception
    {
        public HttpStatusCode StatusCode { get; }
        public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public class AuthException : AppException
        {
            public AuthException(string message = "אימייל או סיסמה שגויים")
                : base(message, HttpStatusCode.Unauthorized) { }
        }

        // שגיאה עבור משתמש שכבר קיים במערכת
        public class UserAlreadyExistsException : AppException
        {
            public UserAlreadyExistsException(string message = "משתמש עם אימייל זה כבר קיים במערכת")
                : base(message, HttpStatusCode.Conflict) { } // Conflict (409) מתאים כאן
        }

        // שגיאה עבור ישות שלא נמצאה (למשל קטגוריה)
        public class NotFoundException : AppException
        {
            public NotFoundException(string message = "המשאב המבוקש לא נמצא")
                : base(message, HttpStatusCode.NotFound) { }
        }
    }
}