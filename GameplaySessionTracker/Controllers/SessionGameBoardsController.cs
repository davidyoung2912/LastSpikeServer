using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using GameplaySessionTracker.GameRules;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionGameBoardsController(
            ISessionGameBoardService sessionGameBoardService,
            ISessionService sessionService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionGameBoard>>> GetAll()
        {
            return Ok(sessionGameBoardService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SessionGameBoard>> GetById(Guid id)
        {
            var sessionGameBoard = sessionGameBoardService.GetById(id);
            if (sessionGameBoard == null)
            {
                return NotFound();
            }
            return Ok(sessionGameBoard);
        }

        [HttpGet("hashedPlayerId")]
        public async Task<ActionResult<string>> GetHashedPlayerId(Guid playerId)
        {
            return Ok(playerId.GetHashCode().ToString());
        }

        [HttpGet("{id}/gamestate")]
        public async Task<ActionResult<GameState>> GetGameState(Guid id)
        {
            var sessionGameBoard = await sessionGameBoardService.GetById(id);
            if (sessionGameBoard == null)
            {
                return NotFound();
            }
            var state = RuleEngine.DeserializeGameState(sessionGameBoard.Data);

            state = state with
            {
                // obfuscate player ids
                Players = new OrderedDictionary<Guid, PlayerState>(state.Players.Select(
                    p => new KeyValuePair<Guid, PlayerState>(
                        new Guid(p.Key.GetHashCode().ToString()),
                        p.Value))),
                CurrentPlayerId = new Guid(state.CurrentPlayerId.GetHashCode().ToString())
            };

            return Ok(state);
        }

        [HttpPost]
        public async Task<ActionResult<SessionGameBoard>> Create(SessionGameBoard sessionGameBoard)
        {
            sessionGameBoard.Id = Guid.NewGuid();
            var created = await sessionGameBoardService.Create(sessionGameBoard);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessionGameBoard sessionGameBoard)
        {
            if (id != sessionGameBoard.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existing = await sessionGameBoardService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate that the session exists
            if (sessionService.GetById(sessionGameBoard.SessionId) == null)
            {
                return BadRequest("Session does not exist");
            }

            await sessionGameBoardService.Update(id, sessionGameBoard);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = sessionGameBoardService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            await sessionGameBoardService.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/action")]
        public async Task<IActionResult> PlayerAction(Guid id, GameAction action)
        {
            // The game board exists
            var gameBoard = await sessionGameBoardService.GetById(id);
            if (gameBoard == null)
            {
                return NotFound();
            }
            // The session exists
            var session = await sessionService.GetById(gameBoard.SessionId);
            if (session == null)
            {
                return BadRequest("Session does not exist");
            }
            // The player exists in the session
            if (!session.PlayerIds.Contains(action.PlayerId))
            {
                return BadRequest("Player does not exist in the session");
            }

            var state = RuleEngine.DeserializeGameState(gameBoard.Data);
            // The current player is the one making the action 
            if (state.CurrentPlayerId != action.PlayerId)
            {
                return BadRequest("Player is not the current player");
            }

            // The action is one of the valid moves of the player
            if (!RuleEngine.GetValidActions(state).Contains(action.Type))
            {
                return BadRequest("Move is not valid");
            }

            // The action contains the right data
            if (action.Type == ActionType.Rebellion || action.Type == ActionType.PlaceTrack)
            {
                if (action.Target == null)
                {
                    return BadRequest("Target is required for Rebellion and PlaceTrack");
                }
            }
            await sessionGameBoardService.PlayerAction(id, action);
            return NoContent();
        }
    }
}
