
using Repository.Exception;
using System.Net;
namespace HotelAp.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;


        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // הבקשה ממשיכה ל-Controller או לשכבות הבאות
                await _next(context);
            }
            catch (Exception ex)
            {
                // אם זורקים Exception – אנחנו תופסים אותו פה
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // ברירת מחדל
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "התרחשה שגיאה בשרת.";

            // אם זו AppException – נשתמש בקוד וההודעה שהגדרת
            if (exception is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = context.Response.StatusCode,
                message = message
            };

            var json = System.Text.Json.JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }

    }
}
