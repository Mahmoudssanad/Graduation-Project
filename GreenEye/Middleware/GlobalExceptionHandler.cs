using OpenQA.Selenium;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace GreenEye.Middleware
{
    public class GlobalExceptionHandler(RequestDelegate _next, IHostEnvironment _env)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // متكتبش فيه حاجه Response علشان اتاكد ان ال 
                // تاني Exception هيحصل Exception بدأ قبل ال Response لو ال 
                if (context.Response.HasStarted)
                {
                    Log.Error("Cannot write error response because response has already started");
                    throw;
                }
                await GlobalHandler(context, ex);
            }
        }


        private Task GlobalHandler(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Unexcpected Error!";

            switch (ex)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Not authorized";
                    break;

                case BadHttpRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;

                case ValidationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;
            }

            // Message مع ال Path سجلنا ال 
            // اللي هيظهر بيه Structure البرامتر التاني دا بحدد فيه ال 
            Log.Error(ex, "Unhandled exception occurred | Message: {Message} | Path: {Path}", ex.Message, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                IsSuccess = false,
                Message = message,
                StatusCode = (int)statusCode,
                Error = _env.IsDevelopment() ? ex.ToString() : null // Exception show by developer only
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
