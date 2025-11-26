using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionGameBoardsController : ControllerBase
    {
        private readonly ISessionGameBoardService _sessionGameBoardService;
        private readonly ISessionService _sessionService;
        private readonly IGameBoardService _gameBoardService;
        private readonly IHubContext<GameHub> _hubContext;

        public SessionGameBoardsController(
            ISessionGameBoardService sessionGameBoardService,
            ISessionService sessionService,
            IGameBoardService gameBoardService,
            IHubContext<GameHub> hubContext)
        {
            _sessionGameBoardService = sessionGameBoardService;
            _sessionService = sessionService;
            _gameBoardService = gameBoardService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SessionGameBoard>> GetAll()
        {
            return Ok(_sessionGameBoardService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<SessionGameBoard> GetById(Guid id)
        {
            var sessionGameBoard = _sessionGameBoardService.GetById(id);
            if (sessionGameBoard == null)
            {
                return NotFound();
            }
            return Ok(sessionGameBoard);
        }

        [HttpPost]
        public async Task<ActionResult<SessionGameBoard>> Create(SessionGameBoard sessionGameBoard)
        {
            sessionGameBoard.Id = Guid.NewGuid();

            // Validate SessionId
            var session = _sessionService.GetById(sessionGameBoard.SessionId);
            if (session == null)
            {
                return BadRequest($"SessionId {sessionGameBoard.SessionId} does not exist");
            }

            // Validate BoardId
            var board = _gameBoardService.GetById(sessionGameBoard.BoardId);
            if (board == null)
            {
                return BadRequest($"BoardId {sessionGameBoard.BoardId} does not exist");
            }

            var created = _sessionGameBoardService.Create(sessionGameBoard);
            await _hubContext.Clients.All.SendAsync("SessionGameBoardUpdated", created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessionGameBoard sessionGameBoard)
        {
            if (id != sessionGameBoard.Id && sessionGameBoard.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = _sessionGameBoardService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate SessionId
            var session = _sessionService.GetById(sessionGameBoard.SessionId);
            if (session == null)
            {
                return BadRequest($"SessionId {sessionGameBoard.SessionId} does not exist");
            }

            // Validate BoardId
            var board = _gameBoardService.GetById(sessionGameBoard.BoardId);
            if (board == null)
            {
                return BadRequest($"BoardId {sessionGameBoard.BoardId} does not exist");
            }

            _sessionGameBoardService.Update(id, sessionGameBoard);
            await _hubContext.Clients.All.SendAsync("SessionGameBoardUpdated", sessionGameBoard);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = _sessionGameBoardService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _sessionGameBoardService.Delete(id);
            await _hubContext.Clients.All.SendAsync("SessionGameBoardUpdated", id);
            return NoContent();
        }
    }
}
