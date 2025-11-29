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
    public class SessionGameBoardsController(
            ISessionGameBoardService sessionGameBoardService,
            ISessionService sessionService,
            IHubContext<GameHub> hubContext)
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

        [HttpPost]
        public async Task<ActionResult<SessionGameBoard>> Create(SessionGameBoard sessionGameBoard)
        {
            sessionGameBoard.Id = Guid.NewGuid();
            var created = sessionGameBoardService.Create(sessionGameBoard);
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

            sessionGameBoardService.Update(id, sessionGameBoard);
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

            sessionGameBoardService.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/action")]
        public async Task<IActionResult> UpdateAction(Guid id, Action action)
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

            sessionGameBoardService.Update(id, sessionGameBoard);
            return NoContent();
        }
    }
}
