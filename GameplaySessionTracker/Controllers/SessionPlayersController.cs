using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionPlayersController : ControllerBase
    {
        private readonly ISessionPlayerService _sessionPlayerService;
        private readonly ISessionService _sessionService;
        private readonly IPlayerService _playerService;
        private readonly IHubContext<GameHub> _hubContext;

        public SessionPlayersController(
            ISessionPlayerService sessionPlayerService,
            ISessionService sessionService,
            IPlayerService playerService,
            IHubContext<GameHub> hubContext)
        {
            _sessionPlayerService = sessionPlayerService;
            _sessionService = sessionService;
            _playerService = playerService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SessionPlayer>> GetAll()
        {
            return Ok(_sessionPlayerService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<SessionPlayer> GetById(Guid id)
        {
            var sessionPlayer = _sessionPlayerService.GetById(id);
            if (sessionPlayer == null)
            {
                return NotFound();
            }
            return Ok(sessionPlayer);
        }

        [HttpGet("session/{sessionId}")]
        public ActionResult<IEnumerable<SessionPlayer>> GetBySessionId(Guid sessionId)
        {
            // Validate session exists
            var session = _sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Get all SessionPlayers for this session
            var sessionPlayers = _sessionPlayerService.GetAll()
                .Where(sp => sp.SessionId == sessionId);

            return Ok(sessionPlayers);
        }

        [HttpPost]
        public async Task<ActionResult<SessionPlayer>> Create(SessionPlayer sessionPlayer)
        {
            sessionPlayer.Id = Guid.NewGuid();

            // Validate SessionId
            var session = _sessionService.GetById(sessionPlayer.SessionId);
            if (session == null)
            {
                return BadRequest($"SessionId {sessionPlayer.SessionId} does not exist");
            }

            // Validate PlayerId
            var player = _playerService.GetById(sessionPlayer.PlayerId);
            if (player == null)
            {
                return BadRequest($"PlayerId {sessionPlayer.PlayerId} does not exist");
            }

            var created = _sessionPlayerService.Create(sessionPlayer);
            await _hubContext.Clients.All.SendAsync("SessionPlayerUpdated", created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessionPlayer sessionPlayer)
        {
            if (id != sessionPlayer.Id && sessionPlayer.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = _sessionPlayerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate SessionId
            var session = _sessionService.GetById(sessionPlayer.SessionId);
            if (session == null)
            {
                return BadRequest($"SessionId {sessionPlayer.SessionId} does not exist");
            }

            // Validate PlayerId
            var player = _playerService.GetById(sessionPlayer.PlayerId);
            if (player == null)
            {
                return BadRequest($"PlayerId {sessionPlayer.PlayerId} does not exist");
            }

            _sessionPlayerService.Update(id, sessionPlayer);
            await _hubContext.Clients.All.SendAsync("SessionPlayerUpdated", sessionPlayer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = _sessionPlayerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _sessionPlayerService.Delete(id);
            await _hubContext.Clients.All.SendAsync("SessionPlayerUpdated", id);
            return NoContent();
        }
    }
}
