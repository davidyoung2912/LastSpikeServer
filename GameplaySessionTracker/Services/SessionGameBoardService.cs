using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class SessionGameBoardService : ISessionGameBoardService
    {
        private readonly ISessionGameBoardRepository _repository;

        public SessionGameBoardService(ISessionGameBoardRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<SessionGameBoard> GetAll()
        {
            return _repository.GetAll();
        }

        public SessionGameBoard? GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public SessionGameBoard Create(SessionGameBoard sessionGameBoard)
        {
            _repository.Add(sessionGameBoard);
            return sessionGameBoard;
        }

        public void Update(Guid id, SessionGameBoard sessionGameBoard)
        {
            _repository.Update(sessionGameBoard);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
    }
}
