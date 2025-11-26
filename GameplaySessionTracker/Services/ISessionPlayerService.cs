using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface ISessionPlayerService
    {
        IEnumerable<SessionPlayer> GetAll();
        SessionPlayer? GetById(Guid id);
        SessionPlayer Create(SessionPlayer sessionPlayer);
        void Update(Guid id, SessionPlayer sessionPlayer);
        void Delete(Guid id);
    }
}
