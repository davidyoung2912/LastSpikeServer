using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;

namespace GameplaySessionTracker.Services
{
    public class SessionPlayerService(
        ISessionPlayerRepository repository,
        IHubContext<GameHub> gameHubContext
        ) : ISessionPlayerService
    {

        public IEnumerable<SessionPlayer> GetAll()
        {
            return repository.GetAll();
        }

        public SessionPlayer? GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public SessionPlayer Create(SessionPlayer sessionPlayer)
        {
            repository.Add(sessionPlayer);
            return sessionPlayer;
        }

        public void Update(Guid id, SessionPlayer sessionPlayer)
        {
            repository.Update(sessionPlayer);
        }

        public void Delete(Guid id)
        {
            repository.Delete(id);
        }
    }
}
