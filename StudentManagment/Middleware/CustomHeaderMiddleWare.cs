using System.Net;

namespace StudentManagment.Middleware
{
    public class CustomHeaderMiddleWare 
    {
        private readonly RequestDelegate _next;
        public CustomHeaderMiddleWare(RequestDelegate requestDelegate) 
        {
            _next= requestDelegate;
        }
        public async Task InvokeAsync(HttpContext context)
        {

            try
            {
                var startTime = DateTime.Now;

                await _next(context);

                var endTime = DateTime.Now;
                var elapsedTime = endTime - startTime;

                var logMessage = $"{context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {elapsedTime.TotalMilliseconds}ms";
                Console.WriteLine(logMessage);
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
