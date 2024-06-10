using Microsoft.Identity.Client;
using StudentManagment_API.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace StudentManagment_API.Middleware
{
    public class CustomHeaderMiddleWare
    {
        private readonly RequestDelegate _next;
        public CustomHeaderMiddleWare(RequestDelegate requestDelegate)
        {
            _next = requestDelegate;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine("An exception occurred: " + ex.Message);

                // Set the response status code to 500 Internal Server Error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
