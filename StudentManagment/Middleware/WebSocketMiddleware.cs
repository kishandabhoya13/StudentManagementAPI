using System.Net.WebSockets;

namespace StudentManagment.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var handler = new ChatWebSocketHandler(webSocket);
                await handler.HandleAsync(context.RequestAborted);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
