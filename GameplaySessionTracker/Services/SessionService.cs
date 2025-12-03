using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class SessionService(ISessionRepository repository) : ISessionService
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

        public async Task Update(Guid id, SessionData session)
        {
            await repository.Update(session);
        }

        public async Task Delete(Guid id)
        {
            await repository.Delete(id);
        }
    }
}
