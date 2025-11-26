using System;
using System.Collections.Generic;

namespace GameplaySessionTracker.Models
{
    public class SessionData
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid BoardId { get; set; }
        public List<Guid> PlayerIds { get; set; } = new List<Guid>();
    }
}
