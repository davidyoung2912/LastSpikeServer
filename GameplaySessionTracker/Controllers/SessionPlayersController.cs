using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionPlayersController(
        ISessionPlayerService sessionPlayerService,
        ISessionService sessionService,
        IPlayerService playerService)
        : ControllerBase
    {

        [HttpGet]
        public ActionResult<IEnumerable<SessionPlayer>> GetAll()
        {
            return Ok(sessionPlayerService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<SessionPlayer> GetById(Guid id)
        {
            var sessionPlayer = sessionPlayerService.GetById(id);
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
            var session = sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Get all SessionPlayers for this session
            var sessionPlayers = sessionPlayerService.GetAll()
                .Where(sp => sp.SessionId == sessionId);

            return Ok(sessionPlayers);
        }

        [HttpPost]
        public async Task<ActionResult<SessionPlayer>> Create(SessionPlayer sessionPlayer)
        {
            sessionPlayer.Id = Guid.NewGuid();

            // Validate SessionId
            var session = sessionService.GetById(sessionPlayer.SessionId);
            if (session == null)
            {
                return BadRequest($"SessionId {sessionPlayer.SessionId} does not exist");
            }

            // Validate PlayerId
            var player = playerService.GetById(sessionPlayer.PlayerId);
            if (player == null)
            {
                return BadRequest($"PlayerId {sessionPlayer.PlayerId} does not exist");
            }

            var created = sessionPlayerService.Create(sessionPlayer);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessionPlayer sessionPlayer)
        {
            if (id != sessionPlayer.Id && sessionPlayer.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = sessionPlayerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate SessionId
            var session = sessionService.GetById(sessionPlayer.SessionId);
            if (session == null)
            {
                return BadRequest($"SessionId {sessionPlayer.SessionId} does not exist");
            }

            // Validate PlayerId
            var player = playerService.GetById(sessionPlayer.PlayerId);
            if (player == null)
            {
                return BadRequest($"PlayerId {sessionPlayer.PlayerId} does not exist");
            }

            sessionPlayerService.Update(id, sessionPlayer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = sessionPlayerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            sessionPlayerService.Delete(id);
            return NoContent();
        }
    }
}
