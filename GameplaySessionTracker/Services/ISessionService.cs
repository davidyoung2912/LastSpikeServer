using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionData>> GetAll();
        Task<SessionData?> GetById(Guid id);
        Task<SessionData> Create(SessionData session);
        Task Update(SessionData session);
        Task Delete(Guid id);
        Task StartGame(Guid id, SessionData session);

        Task AddPlayer(Guid id, SessionData session);
        Task RemovePlayer(Guid id, SessionData session);
        Task<IEnumerable<Player>> GetSessionPlayers(Guid sessionId);

    }
}
