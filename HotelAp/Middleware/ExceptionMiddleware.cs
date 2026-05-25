
//using Repository.Exception;
//using System.Net;
//namespace HotelAp.Middleware
//{
//    public class ExceptionMiddleware
//    {
//        private readonly RequestDelegate _next;


//        public ExceptionMiddleware(RequestDelegate next)
//        {
//            _next = next;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            try
//            {
//                // הבקשה ממשיכה ל-Controller או לשכבות הבאות
//                await _next(context);
//            }
//            catch (Exception ex)
//            {
//                // אם זורקים Exception – אנחנו תופסים אותו פה
//                await HandleExceptionAsync(context, ex);
//            }
//        }
//        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
//        {
//            context.Response.ContentType = "application/json";

//            // ברירת מחדל
//            var statusCode = HttpStatusCode.InternalServerError;
//            var message = "התרחשה שגיאה בשרת.";

//            // אם זו AppException – נשתמש בקוד וההודעה שהגדרת
//            if (exception is AppException appEx)
//            {
//                statusCode = appEx.StatusCode;
//                message = appEx.Message;
//            }

//            context.Response.StatusCode = (int)statusCode;

//            var response = new
//            {
//                status = context.Response.StatusCode,
//                message = message
//            };

//            var json = System.Text.Json.JsonSerializer.Serialize(response);
//            return context.Response.WriteAsync(json);
//        }

//    }
//}
using Repository.Exception;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace HotelAp.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[שגיאה במערכת]: {ex.Message}");

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

         
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "התרחשה שגיאה פנימית בשרת.";

    
            if (exception is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
            }
            else if (exception is EntityNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = statusCode.ToString(),
                Detail = message,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}