using System;

namespace GameplaySessionTracker.Models
{
    public class SessionPlayer
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid PlayerId { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}
