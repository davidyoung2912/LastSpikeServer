using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public class InMemoryGameBoardService : IGameBoardService
    {
        private readonly List<GameBoard> _gameBoards = new List<GameBoard>();
        private readonly object _lock = new object();

        public IEnumerable<GameBoard> GetAll()
        {
            lock (_lock)
            {
                return new List<GameBoard>(_gameBoards);
            }
        }

        public GameBoard? GetById(Guid id)
        {
            lock (_lock)
            {
                return _gameBoards.FirstOrDefault(gb => gb.Id == id);
            }
        }

        public GameBoard Create(GameBoard gameBoard)
        {
            lock (_lock)
            {
                _gameBoards.Add(gameBoard);
            }
            return gameBoard;
        }

        public void Update(Guid id, GameBoard gameBoard)
        {
            lock (_lock)
            {
                var existing = _gameBoards.FirstOrDefault(gb => gb.Id == id);
                if (existing != null)
                {
                    existing.Description = gameBoard.Description;
                    existing.Data = gameBoard.Data;
                }
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var existing = _gameBoards.FirstOrDefault(gb => gb.Id == id);
                if (existing != null)
                {
                    _gameBoards.Remove(existing);
                }
            }
        }
    }
}
