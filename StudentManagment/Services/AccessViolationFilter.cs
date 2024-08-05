using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudentManagment.Services
{
    public class AccessViolationFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is AccessViolationException)
            {
                context.Result = new RedirectResult("/Login/Login");
                context.ExceptionHandled = true;
                context.HttpContext.Session.Clear();
            }
        }
    }
}
