using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface ISessionRepository
    {
        Task<IEnumerable<SessionData>> GetAll();
        Task<SessionData?> GetById(Guid id);
        Task Add(SessionData session);
        Task Update(SessionData session);
        Task Delete(Guid id);
        Task<IEnumerable<Player>> GetSessionPlayers(Guid sessionId);
    }
}
