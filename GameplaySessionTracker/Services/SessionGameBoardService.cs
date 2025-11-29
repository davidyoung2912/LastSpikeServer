using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;

namespace GameplaySessionTracker.Services
{
    public class SessionGameBoardService(
        ISessionGameBoardRepository repository,
        IHubContext<GameHub> hubContext) : ISessionGameBoardService
    {

        public async Task<IEnumerable<SessionGameBoard>> GetAll()
        {
            return repository.GetAll();
        }

        public async Task<SessionGameBoard?> GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public async Task<SessionGameBoard> Create(SessionGameBoard sessionGameBoard)
        {
            repository.Add(sessionGameBoard);
            return sessionGameBoard;
        }

        public async Task Update(Guid id, SessionGameBoard sessionGameBoard)
        {
            repository.Update(sessionGameBoard);
            await hubContext.Clients.Group(sessionGameBoard.SessionId.ToString()).SendAsync("SessionGameBoardUpdated", sessionGameBoard.Data);
        }

        public async Task Delete(Guid id)
        {
            repository.Delete(id);
        }
    }
}
