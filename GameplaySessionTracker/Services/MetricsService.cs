using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public class MetricsService : IMetricsService
    {
        public ServiceMetrics Get()
        {
            return new ServiceMetrics();
        }

        public PlayerMetrics GetById(Guid id)
        {
            return new PlayerMetrics();
        }

        public bool Reset()
        {
            return true;
        }

        public bool ResetById(Guid id)
        {
            return true;
        }
    }
}
