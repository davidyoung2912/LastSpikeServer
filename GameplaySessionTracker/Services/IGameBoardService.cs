using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface IGameBoardService
    {
        IEnumerable<GameBoard> GetAll();
        GameBoard? GetById(Guid id);
        GameBoard Create(GameBoard gameBoard);
        void Update(Guid id, GameBoard gameBoard);
        void Delete(Guid id);
    }
}
