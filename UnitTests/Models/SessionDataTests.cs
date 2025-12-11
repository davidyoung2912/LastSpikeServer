using System;
using System.Collections.Generic;
using Xunit;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Tests.Models;

public class SessionDataTests
{
    [Fact]
    public void SessionData_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var session = new SessionData { PlayerIds = new List<Guid>() };

        // Assert
        Assert.Equal(Guid.Empty, session.Id);
        Assert.Equal(string.Empty, session.Description);
        Assert.Equal(Guid.Empty, session.BoardId);
        Assert.Null(session.StartTime);
        Assert.Null(session.EndTime);
        Assert.NotNull(session.PlayerIds);
        Assert.Empty(session.PlayerIds);
    }

    [Fact]
    public void SessionData_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var session = new SessionData { PlayerIds = new List<Guid>() };
        var id = Guid.NewGuid();
        var description = "Test Session";
        var boardId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddHours(1);
        var playerId = Guid.NewGuid();

        // Act
        session.Id = id;
        session.Description = description;
        session.BoardId = boardId;
        session.StartTime = startTime;
        session.EndTime = endTime;
        session.PlayerIds.Add(playerId);

        // Assert
        Assert.Equal(id, session.Id);
        Assert.Equal(description, session.Description);
        Assert.Equal(boardId, session.BoardId);
        Assert.Equal(startTime, session.StartTime);
        Assert.Equal(endTime, session.EndTime);
        Assert.Single(session.PlayerIds);
        Assert.Contains(playerId, session.PlayerIds);
    }
}
