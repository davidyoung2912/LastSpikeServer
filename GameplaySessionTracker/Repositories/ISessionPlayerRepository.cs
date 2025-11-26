using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface ISessionPlayerRepository
    {
        IEnumerable<SessionPlayer> GetAll();
        SessionPlayer? GetById(Guid id);
        void Add(SessionPlayer sessionPlayer);
        void Update(SessionPlayer sessionPlayer);
        void Delete(Guid id);
    }
}
