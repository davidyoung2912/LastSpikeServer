using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public class SessionPlayerRepository : ISessionPlayerRepository
    {
        private readonly List<SessionPlayer> _sessionPlayers = new();

        public IEnumerable<SessionPlayer> GetAll()
        {
            return _sessionPlayers;
        }

        public SessionPlayer? GetById(Guid id)
        {
            return _sessionPlayers.FirstOrDefault(sp => sp.Id == id);
        }

        public void Add(SessionPlayer sessionPlayer)
        {
            _sessionPlayers.Add(sessionPlayer);
        }

        public void Update(SessionPlayer sessionPlayer)
        {
            var existing = GetById(sessionPlayer.Id);
            if (existing != null)
            {
                existing.SessionId = sessionPlayer.SessionId;
                existing.PlayerId = sessionPlayer.PlayerId;
                existing.Data = sessionPlayer.Data;
            }
        }

        public void Delete(Guid id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                _sessionPlayers.Remove(existing);
            }
        }
    }
}
