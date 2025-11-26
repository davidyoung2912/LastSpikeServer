using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface ISessionGameBoardService
    {
        IEnumerable<SessionGameBoard> GetAll();
        SessionGameBoard? GetById(Guid id);
        SessionGameBoard Create(SessionGameBoard sessionGameBoard);
        void Update(Guid id, SessionGameBoard sessionGameBoard);
        void Delete(Guid id);
    }
}
