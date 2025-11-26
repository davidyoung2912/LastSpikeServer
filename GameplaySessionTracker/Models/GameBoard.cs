using System;

namespace GameplaySessionTracker.Models
{
    public class GameBoard
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
