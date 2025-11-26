using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface IMetricsService
    {
        ServiceMetrics Get();
        PlayerMetrics GetById(Guid id);
        bool Reset();
        bool ResetById(Guid id);
    }
}
