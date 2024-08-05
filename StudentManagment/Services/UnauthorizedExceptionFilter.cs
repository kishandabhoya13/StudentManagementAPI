using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudentManagment.Services
{
    public class UnauthorizedExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                bool isAjaxRequest = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

                if (isAjaxRequest)
                {
                    context.Result = new JsonResult ( new { redirect = "/Login/Logout" } );
                    context.ExceptionHandled = true;
                }
                else
                {
                    context.Result = new RedirectResult("/Login/Logout");
                    context.ExceptionHandled = true;
                }

            }


        }
    }
}
