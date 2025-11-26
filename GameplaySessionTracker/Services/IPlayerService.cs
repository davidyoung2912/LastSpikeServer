using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface IPlayerService
    {
        IEnumerable<Player> GetAll();
        Player? GetById(Guid id);
        Player Create(Player player);
        void Update(Guid id, Player player);
        void Delete(Guid id);
    }
}
