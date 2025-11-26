using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public class InMemorySessionService : ISessionService
    {
        private readonly List<SessionData> _sessions = new List<SessionData>();
        private readonly object _lock = new object();

        public IEnumerable<SessionData> GetAll()
        {
            lock (_lock)
            {
                return new List<SessionData>(_sessions);
            }
        }

        public SessionData? GetById(Guid id)
        {
            lock (_lock)
            {
                return _sessions.FirstOrDefault(s => s.Id == id);
            }
        }

        public SessionData Create(SessionData session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
            }
            return session;
        }

        public void Update(Guid id, SessionData session)
        {
            lock (_lock)
            {
                var existing = _sessions.FirstOrDefault(s => s.Id == id);
                if (existing != null)
                {
                    existing.Description = session.Description;
                    existing.BoardId = session.BoardId;
                    existing.PlayerIds = session.PlayerIds;
                }
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var existing = _sessions.FirstOrDefault(s => s.Id == id);
                if (existing != null)
                {
                    _sessions.Remove(existing);
                }
            }
        }
    }
}
