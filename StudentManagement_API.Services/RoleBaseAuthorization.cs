using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Services
{
    public class RoleBaseAuthorization : Attribute,IAuthorizationFilter
    {
        private readonly string _role;
        private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        public RoleBaseAuthorization(string role = "")
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var jwtService = filterContext.HttpContext.RequestServices.GetService<IJwtServices>();
            var request = filterContext.HttpContext.Request;
            var token = request.Cookies["jwt"];

            if (token == null || !jwtService.ValidateToken(token, out JwtSecurityToken jwtToken))
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                _httpContextAccessor.HttpContext.Response.WriteAsync("Unauthorized");
                return;
            }

            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);

            if (roleClaim != null)
            {
                if (roleClaim.Value != _role)
                {
                    _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    _httpContextAccessor.HttpContext.Response.WriteAsync("Forbidden");
                    return;
                }
            }
            else
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                _httpContextAccessor.HttpContext.Response.WriteAsync("Bad Request");
                return;
            }

            return;
        }
    }

    public class CustomAuthorize2 : Attribute, IAuthorizationFilter
    {
        private readonly string _role1;
        private readonly string _role2;
        public CustomAuthorize2(string role1 = "", string role2 = "")
        {
            _role1 = role1;
            _role2 = role2;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            //var httpContext = filterContext.HttpContext;
            var jwtService = filterContext.HttpContext.RequestServices.GetService<IJwtServices>();
            var request = filterContext.HttpContext.Request;
            var token = request.Cookies["jwt"];
            if (token == null || !jwtService.ValidateToken(token, out JwtSecurityToken jwtToken))
            {
                //httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //httpContext.Response.WriteAsync("Unauthorized");
                return;

            }

            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);

            if (roleClaim == null)
            {
                //httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                //httpContext.Response.WriteAsync("Bad Request");
                return;
            }
            else if (roleClaim.Value != _role1 && roleClaim.Value != _role2)
            {
                //httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                //httpContext.Response.WriteAsync("Bad Request");
                return;
            }
        }
    }

    public class RoleBasedAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleBasedAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var userRoles = context.HttpContext.User?.Claims?
               .Where(c => c.Type == ClaimTypes.Role)
               .Select(c => c.Value);

            if (userRoles == null || !_roles.Any(role => userRoles.Contains(role)))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
