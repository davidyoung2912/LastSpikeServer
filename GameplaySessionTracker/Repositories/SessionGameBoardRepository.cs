using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public class SessionGameBoardRepository : ISessionGameBoardRepository
    {
        private readonly List<SessionGameBoard> _sessionGameBoards = new();

        public IEnumerable<SessionGameBoard> GetAll()
        {
            return _sessionGameBoards;
        }

        public SessionGameBoard? GetById(Guid id)
        {
            return _sessionGameBoards.FirstOrDefault(sgb => sgb.Id == id);
        }

        public void Add(SessionGameBoard sessionGameBoard)
        {
            _sessionGameBoards.Add(sessionGameBoard);
        }

        public void Update(SessionGameBoard sessionGameBoard)
        {
            var existing = GetById(sessionGameBoard.Id);
            if (existing != null)
            {
                existing.SessionId = sessionGameBoard.SessionId;
                existing.BoardId = sessionGameBoard.BoardId;
                existing.Data = sessionGameBoard.Data;
            }
        }

        public void Delete(Guid id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                _sessionGameBoards.Remove(existing);
            }
        }
    }
}
