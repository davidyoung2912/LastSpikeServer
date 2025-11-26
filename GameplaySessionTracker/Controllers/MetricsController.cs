using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly IPlayerService _playerService;

        public MetricsController(IMetricsService metricsService, IPlayerService playerService)
        {
            _metricsService = metricsService;
            _playerService = playerService;
        }

        [HttpGet]
        public ActionResult<ServiceMetrics> Get()
        {
            return Ok(_metricsService.Get());
        }

        [HttpGet("{id}")]
        public ActionResult<PlayerMetrics> GetById(Guid id)
        {
            var player = _playerService.GetById(id);
            if (player == null)
            {
                return NotFound();
            }

            return Ok(_metricsService.GetById(id));
        }

        [HttpPut]
        public ActionResult<bool> Reset()
        {
            return Ok(_metricsService.Reset());
        }

        [HttpPut("{id}")]
        public ActionResult<bool> ResetById(Guid id)
        {
            var player = _playerService.GetById(id);
            if (player == null)
            {
                return NotFound();
            }

            return Ok(_metricsService.ResetById(id));
        }
    }
}
