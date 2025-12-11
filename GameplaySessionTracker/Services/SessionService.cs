using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;

namespace GameplaySessionTracker.Services
{
    public class SessionService(
        ISessionRepository repository,
        IHubContext<GameHub> hubContext
        ) : ISessionService
    {
        public async Task<IEnumerable<SessionData>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<SessionData?> GetById(Guid id)
        {
            return await repository.GetById(id);
        }

        public async Task<SessionData> Create(SessionData session)
        {
            await repository.Add(session);
            return session;
        }

        public async Task Update(SessionData session)
        {
            await repository.Update(session);
        }

        public async Task Delete(Guid id)
        {
            await repository.Delete(id);
        }

        public async Task StartGame(Guid id, SessionData session)
        {
            session.StartTime = DateTime.UtcNow;
            // Notify players that the game has started
            await hubContext.Clients.Group(session.Id.ToString()).SendAsync("GameStarted");
            await Update(session);
        }

        public async Task AddPlayer(Guid playerId, SessionData session)
        {
            // Add player to session
            if (session.PlayerIds.Count == 0)
            {
                session.StartTime = DateTime.UtcNow;
            }

            session.PlayerIds.Add(playerId);
            // update players in session that a new player has joined
            await hubContext.Clients.Group(session.Id.ToString()).SendAsync("PlayerJoined", playerId);
            await Update(session);
        }

        public async Task RemovePlayer(Guid playerId, SessionData session)
        {
            // Remove player from session
            session.PlayerIds.Remove(playerId);
            // update players in session that a player has been removed
            await hubContext.Clients.Group(session.Id.ToString()).SendAsync("PlayerRemoved", playerId);
            await Update(session);
        }

        public async Task<IEnumerable<Player>> GetSessionPlayers(Guid sessionId)
        {
            return await repository.GetSessionPlayers(sessionId);
        }
    }
}
