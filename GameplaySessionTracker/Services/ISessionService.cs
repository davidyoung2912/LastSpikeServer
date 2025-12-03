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
        Task Update(Guid id, SessionData session);
        Task Delete(Guid id);
    }
}
