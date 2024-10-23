using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Tls;
using StudentManagment.Models;
using System.Collections.Concurrent;

namespace StudentManagment.CallHubs
{
    //public class CallHub : Hub
    //{
    //    private static readonly Dictionary<string, List<int>> pendingRequests = new();

    //    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
    //    private static Dictionary<string, List<string>> userConnectionMap = new Dictionary<string, List<string>>();
    //    private static string userConnectionMapLocker = string.Empty;
    //    public override async Task OnConnectedAsync()
    //    {
    //        KeepUserConnection(Context.ConnectionId);
    //        await base.OnConnectedAsync();
    //    }

    //    public static List<string> GetUserConnections(string userId)
    //    {
    //        var conn = new List<string>();
    //        lock (userConnectionMapLocker)
    //        {
    //            if (userConnectionMap.ContainsKey(userId))
    //            {
    //                conn = userConnectionMap[userId];
    //            }
    //        }
    //        return conn;
    //    }

    //    public void KeepUserConnection(string connectionId)
    //    {
    //        lock (userConnectionMapLocker)
    //        {
    //            string UserId = _httpContextAccessor.HttpContext?.Session.GetString("UserId") ?? "1";
    //            if (!userConnectionMap.ContainsKey(UserId))
    //            {
    //                userConnectionMap[UserId] = new List<string>();
    //            }
    //            userConnectionMap[UserId].Add(connectionId);
    //        }
    //    }


    //    public async Task StartCall(List<string> connectionIds)
    //    {
    //        foreach (var connectionId in connectionIds)
    //        {
    //            if (!pendingRequests.ContainsKey(connectionId))
    //            {
    //                pendingRequests[connectionId] = new List<int>();
    //            }

    //            await Clients.All.SendAsync("CallStarted", connectionId);
    //        }

    //    }

    //    public async Task JoinCall(int participantId, List<string> connectionIds)
    //    {
    //        // Add the participant to the pending requests for the host
    //        //foreach(var connectionId in connectionIds)
    //        //{
    //        try
    //        {
    //            if (pendingRequests.ContainsKey(connectionIds[0]))
    //            {
    //                pendingRequests[connectionIds[0]].Add(participantId);
    //                Console.WriteLine("Client connection ids", Clients.Client(connectionIds[0]));
    //                await Clients.All.SendAsync("JoinRequest", participantId);
    //            }
    //        }
    //        catch(Exception ex)
    //        {
    //            Console.WriteLine($"Error sending message to client {connectionIds[0]}: {ex.Message}");
    //        }

    //        //}

    //    }

    //    public async Task AcceptCall(int participateId)
    //    {
    //        //if (pendingRequests.ContainsKey(connectionId))
    //        //{
    //        //    pendingRequests[connectionId].Remove(participateId);
    //        //}
    //        await Clients.User(participateId.ToString()).SendAsync("ParticipantAccepted", participateId);


    //    }

    //    public async Task DeclineCall(int participantId, List<string> connectionIds)
    //    {
    //        foreach (var connectionId in connectionIds)
    //        {
    //            if (pendingRequests.ContainsKey(connectionId))
    //            {
    //                pendingRequests[connectionId].Remove(participantId);
    //            }
    //            await Clients.Client(connectionId).SendAsync("ParticipantDeclined", participantId);
    //        }
    //    }
    //}

    public class CallHub : Hub
    {
        private static List<Call> activeCalls = new List<Call>();
        private static readonly ConcurrentDictionary<string, string> _connectedClients = new();

        private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        public override async Task OnConnectedAsync()
        {
            string userId = "1";
            if (_httpContextAccessor.HttpContext?.Session.GetString("UserId") != null)
            {
                userId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            }
            await Connect(userId);
            await base.OnConnectedAsync();
        }

        public async Task Connect(string userId)
        {
            // Add the userId with the connectionId
            _connectedClients[userId] = Context.ConnectionId;
            LogConnectedClients();

            // Optionally, send a welcome message to the user
            await Clients.Caller.SendAsync("Welcome", $"Connected as {userId}");
        }

        private void LogConnectedClients()
        {
            Console.WriteLine("Connected Clients:");
            foreach (var kvp in _connectedClients)
            {
                Console.WriteLine($"- ConnectionId: {kvp.Key}, UserId: {kvp.Value}");
            }
        }

        public async Task StartCall(string hostId)
        {
            // Optionally, create a new call or join an existing one.
            var call = new Call { HostId = hostId };
            activeCalls.Add(call);

            await Clients.All.SendAsync("ReceiveCall", hostId);
        }


        public async Task JoinCall(string participantId)
        {
            // Notify the host that a participant wants to join
            var call = activeCalls.FirstOrDefault(c => c.HostId == "1"); // Replace with actual host identification logic
            if (call != null)
            {
                string connectionId = _connectedClients["1"];
                call.Participants.Add(participantId);
                await Clients.Client(connectionId).SendAsync("JoinRequest", participantId);
            }
        }

        public async Task RespondToRequest(string participantId, bool accept)
        {
            try
            {
                string connectionId = _connectedClients[participantId];
                if (accept)
                {
                    await Clients.Client(connectionId).SendAsync("RequestAccepted",participantId);
                }
                else
                {
                    await Clients.Client(connectionId).SendAsync("RequestDeclined",participantId);
                }
            }
            catch(Exception ex)
            {

            }
           
        }

        public async Task CloseCall(string hostId)
        {
            // Optionally, create a new call or join an existing one.
            var call = new Call { HostId = hostId };
            activeCalls.Remove(call);
            await Clients.All.SendAsync("CloseCall", hostId);
        }
    }
}
