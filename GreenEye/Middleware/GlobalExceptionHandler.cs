using Microsoft.IdentityModel.SecurityTokenService;
using OpenQA.Selenium;
using System.Net;

namespace GreenEye.Middleware
{
    public class GlobalExceptionHandler(RequestDelegate _next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await GlobalHandler(context, ex);
            }
        }

        private Task GlobalHandler(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = ex.Message;

            switch (ex)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;

                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
            }

            context.Response.ContentType = "application/json";

            var response = new
            {
                IsSuccess = false,
                Message = message,
                StatusCode = (int)statusCode,
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
