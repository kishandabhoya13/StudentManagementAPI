using Azure.Core;
using Microsoft.Identity.Client;
using StudentManagement_API.DataContext;
using StudentManagement_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using static DemoApiWithoutEF.Utilities.Enums;

namespace StudentManagment_API.Middleware
{
    public class CustomHeaderMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IJwtServices _jwtServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomHeaderMiddleWare(RequestDelegate requestDelegate,
            IConfiguration configuration, IJwtServices jwtServices, IHttpContextAccessor httpContextAccessor)
        {
            _next = requestDelegate;
            _configuration = configuration;
            _jwtServices = jwtServices;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var apiVersion = context.Request.Headers["Api-Version"];
                if (!string.IsNullOrEmpty(apiVersion))
                {
                    var token = context.Request.Headers["token"];
                    if (!string.IsNullOrEmpty(token) && apiVersion != GetCurrentApiVersion(token))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("API version changed. Please login again.");
                       
                        return;
                    }
                }
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

        private string GetCurrentApiVersion(string token)
        {
            if(_jwtServices.ValidateToken(token, out JwtSecurityToken jwtSecurityToken))
            {
                Claim claim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Version);
                return claim.Value;
            }
            return "";
        }
    }
}
