using Newtonsoft.Json;
using StudentManagment.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace StudentManagment.Middleware
{
    //public class ChatWebSocketHandler
    //{
    //    private static ConcurrentDictionary<WebSocket, string> _clients = new ConcurrentDictionary<WebSocket, string>();
    //    private readonly WebSocket _webSocket;

    //    public ChatWebSocketHandler(WebSocket webSocket)
    //    {
    //        _webSocket = webSocket;
    //    }

    //    public async Task HandleAsync(CancellationToken token)
    //    {
    //        string userName = null;
    //        try
    //        {
    //            var buffer = new byte[1024 * 4];
    //            while (_webSocket.State == WebSocketState.Open)
    //            {
    //                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
    //                if (result.MessageType == WebSocketMessageType.Text)
    //                {
    //                    var messageData = Encoding.UTF8.GetString(buffer, 0, result.Count);
    //                    var message = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatMessage>(messageData);
    //                    if (userName == null)
    //                    {
    //                        userName = message.User;
    //                        _clients.TryAdd(_webSocket, userName);
    //                    }
    //                    var broadcastMessage = JsonConvert.SerializeObject(message);
    //                    foreach (var client in _clients.Keys)
    //                    {
    //                         if (client != _webSocket)
    //                        {
    //                            await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(broadcastMessage)), WebSocketMessageType.Text, true, token);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            _clients.TryRemove(_webSocket, out _);
    //        }

    //    }
    //}

    public static class WebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public static async Task HandleWebSocket(WebSocket webSocket)
        {
            var id = Guid.NewGuid().ToString();
            _sockets.TryAdd(id, webSocket);

            await SendMessage(webSocket, $"Welcome to the call, your ID is {id}");

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    _sockets.TryRemove(id, out _);
                }
            }
        }

        public static async Task StartCall(string hostId)
        {
            var message = $"{hostId} has started a call.";
            foreach (var socket in _sockets)
            {
                await SendMessage(socket.Value, message);
            }
        }

        public static async Task JoinCall(string participantId, string hostId)
        {
            if (_sockets.TryGetValue(hostId, out var hostSocket))
            {
                await SendMessage(hostSocket, $"{participantId} wants to join.");
            }
        }

        private static async Task SendMessage(WebSocket socket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
