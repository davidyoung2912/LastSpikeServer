using Microsoft.AspNetCore.SignalR;

namespace GameplaySessionTracker.Hubs
{
    public class GameHub(Services.ISessionService sessionService) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }

        public async Task JoinSession(string sessionId, string playerId)
        {
            if (!Guid.TryParse(sessionId, out var sessionIdGuid) || !Guid.TryParse(playerId, out var playerIdGuid))
            {
                return;
            }

            var session = await sessionService.GetById(sessionIdGuid);
            if (session != null && session.PlayerIds.Contains(playerIdGuid))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
                Console.WriteLine($"Client {Context.ConnectionId} joined session {sessionId}");
            }
        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            Console.WriteLine($"Client {Context.ConnectionId} left session {sessionId}");
        }
    }
}
