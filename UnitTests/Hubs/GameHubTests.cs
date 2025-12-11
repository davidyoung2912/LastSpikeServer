using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameplaySessionTracker.Hubs;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Hubs;

public class GameHubTests
{
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly GameHub _hub;

    public GameHubTests()
    {
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockSessionService = new Mock<ISessionService>();

        _hub = new GameHub(_mockSessionService.Object)
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object
        };

        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
    }

    [Fact]
    public async Task OnConnectedAsync_CompletesSuccessfully()
    {
        // Act
        await _hub.OnConnectedAsync();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithoutException_CompletesSuccessfully()
    {
        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task JoinSession_ValidSessionAndPlayer_AddsToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId } };

        _mockSessionService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);

        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinSession(sessionId.ToString(), playerId.ToString());

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", sessionId.ToString(), default), Times.Once);
    }

    [Fact]
    public async Task JoinSession_InvalidSession_DoesNotAddToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        _mockSessionService.Setup(s => s.GetById(sessionId)).ReturnsAsync((SessionData?)null);

        // Act
        await _hub.JoinSession(sessionId.ToString(), playerId.ToString());

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task JoinSession_PlayerNotInSession_DoesNotAddToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() }; // Player not in list

        _mockSessionService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);

        // Act
        await _hub.JoinSession(sessionId.ToString(), playerId.ToString());

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task LeaveSession_RemovesFromGroup()
    {
        // Arrange
        var sessionId = "test-session-123";

        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveSession(sessionId);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync("test-connection-id", sessionId, default), Times.Once);
    }
}
