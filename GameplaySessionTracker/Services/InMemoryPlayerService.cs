using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public class InMemoryPlayerService : IPlayerService
    {
        private readonly List<Player> _players = new List<Player>();
        private readonly object _lock = new object();

        public IEnumerable<Player> GetAll()
        {
            lock (_lock)
            {
                return new List<Player>(_players);
            }
        }

        public Player? GetById(Guid id)
        {
            lock (_lock)
            {
                return _players.FirstOrDefault(p => p.Id == id);
            }
        }

        public Player Create(Player player)
        {
            lock (_lock)
            {
                _players.Add(player);
            }
            return player;
        }

        public void Update(Guid id, Player player)
        {
            lock (_lock)
            {
                var existing = _players.FirstOrDefault(p => p.Id == id);
                if (existing != null)
                {
                    existing.Name = player.Name;
                    existing.Alias = player.Alias;
                }
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var existing = _players.FirstOrDefault(p => p.Id == id);
                if (existing != null)
                {
                    _players.Remove(existing);
                }
            }
        }
    }
}
