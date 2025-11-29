using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController(
            ISessionService sessionService,
            ISessionGameBoardService sessionGameBoardService,
            IPlayerService playerService)
        : ControllerBase
    {

        [HttpGet]
        public ActionResult<IEnumerable<SessionData>> GetAll()
        {
            return Ok(sessionService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<SessionData> GetById(Guid id)
        {
            var session = sessionService.GetById(id);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        [HttpPost]
        public async Task<ActionResult<SessionData>> Create(SessionData session)
        {
            session.Id = Guid.NewGuid();

            // If we don't specify a board, create a new one
            if (session.BoardId == Guid.Empty)
            {
                session.BoardId = Guid.NewGuid();
            }

            var board = await sessionGameBoardService.GetById(session.BoardId) ??
                await sessionGameBoardService.Create(
                    new SessionGameBoard { Id = session.BoardId, SessionId = session.Id });

            // Validate PlayerIds
            foreach (var playerId in session.PlayerIds)
            {
                if (playerService.GetById(playerId) == null)
                {
                    return BadRequest($"PlayerId {playerId} does not exist");
                }
            }

            var createdSession = sessionService.Create(session);
            return CreatedAtAction(nameof(GetById), new { id = createdSession.Id }, createdSession);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessionData session)
        {
            if (id != session.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existing = sessionService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate BoardId
            if (session.BoardId != Guid.Empty)
            {
                var board = await sessionGameBoardService.GetById(session.BoardId);
                if (board == null)
                {
                    return BadRequest($"BoardId {session.BoardId} does not exist");
                }
            }

            // Validate PlayerIds
            foreach (var playerId in session.PlayerIds)
            {
                var player = playerService.GetById(playerId);
                if (player == null)
                {
                    return BadRequest($"PlayerId {playerId} does not exist");
                }
            }

            sessionService.Update(id, session);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var existing = sessionService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            sessionService.Delete(id);
            return NoContent();
        }

        [HttpPost("{sessionId}/players/{playerId}")]
        public IActionResult AddPlayerToSession(Guid sessionId, Guid playerId)
        {
            // Validate session exists
            var session = sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Validate player exists
            var player = playerService.GetById(playerId);
            if (player == null)
            {
                return NotFound($"Player with ID {playerId} not found");
            }

            // Check if player is already in the session
            if (session.PlayerIds.Contains(playerId))
            {
                return BadRequest($"Player {playerId} is already in session {sessionId}");
            }

            // Add player to session
            if (session.PlayerIds.Count == 0)
            {
                session.StartTime = DateTime.UtcNow;
            }
            session.PlayerIds.Add(playerId);
            sessionService.Update(sessionId, session);

            return Ok(new { message = $"Player {playerId} added to session {sessionId}" });
        }

        [HttpDelete("{sessionId}/players/{playerId}")]
        public IActionResult RemovePlayerFromSession(Guid sessionId, Guid playerId)
        {
            // Validate session exists
            var session = sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Check if player is in the session
            if (!session.PlayerIds.Contains(playerId))
            {
                return BadRequest($"Player {playerId} is not in session {sessionId}");
            }

            // Remove player from session
            session.PlayerIds.Remove(playerId);
            if (session.PlayerIds.Count == 0)
            {
                session.EndTime = DateTime.UtcNow;
            }
            sessionService.Update(sessionId, session);

            return Ok(new { message = $"Player {playerId} removed from session {sessionId}" });
        }
    }
}