using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class SessionPlayerService : ISessionPlayerService
    {
        private readonly ISessionPlayerRepository _repository;

        public SessionPlayerService(ISessionPlayerRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<SessionPlayer> GetAll()
        {
            return _repository.GetAll();
        }

        public SessionPlayer? GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public SessionPlayer Create(SessionPlayer sessionPlayer)
        {
            _repository.Add(sessionPlayer);
            return sessionPlayer;
        }

        public void Update(Guid id, SessionPlayer sessionPlayer)
        {
            _repository.Update(sessionPlayer);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
    }
}
