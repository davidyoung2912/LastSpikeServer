using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayersController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Player>> GetAll()
        {
            return Ok(_playerService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Player> GetById(Guid id)
        {
            var player = _playerService.GetById(id);
            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        [HttpPost]
        public ActionResult<Player> Create(Player player)
        {
            player.Id = Guid.NewGuid();
            var createdPlayer = _playerService.Create(player);
            return CreatedAtAction(nameof(GetById), new { id = createdPlayer.Id }, createdPlayer);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, Player player)
        {
            if (id != player.Id && player.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = _playerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _playerService.Update(id, player);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var existing = _playerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _playerService.Delete(id);
            return NoContent();
        }
    }
}
