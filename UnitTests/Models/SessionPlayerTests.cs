using System;
using Xunit;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Tests.Models;

public class SessionPlayerTests
{
    [Fact]
    public void SessionPlayer_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var sessionPlayer = new SessionPlayer();

        // Assert
        Assert.Equal(Guid.Empty, sessionPlayer.Id);
        Assert.Equal(Guid.Empty, sessionPlayer.SessionId);
        Assert.Equal(Guid.Empty, sessionPlayer.PlayerId);
        Assert.Equal(string.Empty, sessionPlayer.Data);
    }

    [Fact]
    public void SessionPlayer_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var sessionPlayer = new SessionPlayer();
        var id = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var data = "Test Data";

        // Act
        sessionPlayer.Id = id;
        sessionPlayer.SessionId = sessionId;
        sessionPlayer.PlayerId = playerId;
        sessionPlayer.Data = data;

        // Assert
        Assert.Equal(id, sessionPlayer.Id);
        Assert.Equal(sessionId, sessionPlayer.SessionId);
        Assert.Equal(playerId, sessionPlayer.PlayerId);
        Assert.Equal(data, sessionPlayer.Data);
    }
}
