using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface ISessionService
    {
        IEnumerable<SessionData> GetAll();
        SessionData? GetById(Guid id);
        SessionData Create(SessionData session);
        void Update(Guid id, SessionData session);
        void Delete(Guid id);
    }
}
