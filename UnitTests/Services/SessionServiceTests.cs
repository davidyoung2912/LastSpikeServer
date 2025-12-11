using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using GameplaySessionTracker.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _mockRepository;
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly SessionService _service;

    public SessionServiceTests()
    {
        _mockRepository = new Mock<ISessionRepository>();
        _mockHubContext = new Mock<IHubContext<GameHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockHubContext.Setup(h => h.Groups).Returns(_mockGroups.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _service = new SessionService(_mockRepository.Object, _mockHubContext.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllSessions()
    {
        // Arrange
        var sessions = new List<SessionData>
        {
            new SessionData { Id = Guid.NewGuid(), Description = "Session1", BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() },
            new SessionData { Id = Guid.NewGuid(), Description = "Session2", BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() }
        };
        _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(sessions);

        // Act
        var result = await _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, Description = "Test", BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() };
        _mockRepository.Setup(r => r.GetById(sessionId)).ReturnsAsync(session);

        // Act
        var result = await _service.GetById(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.Id);
        _mockRepository.Verify(r => r.GetById(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetById(sessionId)).ReturnsAsync((SessionData?)null);

        // Act
        var result = await _service.GetById(sessionId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(sessionId), Times.Once);
    }

    [Fact]
    public async Task Create_ValidSession_CallsRepositoryAdd()
    {
        // Arrange
        var session = new SessionData { Id = Guid.NewGuid(), Description = "New", BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() };

        // Act
        var result = await _service.Create(session);

        // Assert
        Assert.Equal(session, result);
        _mockRepository.Verify(r => r.Add(session), Times.Once);
    }

    [Fact]
    public async Task Update_ValidSession_CallsRepositoryUpdate()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, Description = "Updated", BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() };

        // Act
        await _service.Update(session);

        // Assert
        _mockRepository.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public async Task Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        await _service.Delete(sessionId);

        // Assert
        _mockRepository.Verify(r => r.Delete(sessionId), Times.Once);
    }

    [Fact]
    public async Task StartGame_SetsStartTimeAndNotifiesClients()
    {
        // Arrange
        var session = new SessionData { Id = Guid.NewGuid(), PlayerIds = new List<Guid>() };

        // Act
        await _service.StartGame(session.Id, session);

        // Assert
        Assert.NotNull(session.StartTime);
        _mockClients.Verify(c => c.Group(session.Id.ToString()), Times.Once);
        // Verify notification sent using SendCoreAsync for generic SendAsync call
        _mockClientProxy.Verify(c => c.SendCoreAsync("GameStarted", It.IsAny<object[]>(), default), Times.Once);
        _mockRepository.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_FirstPlayer_SetsStartTimeAndNotifies()
    {
        // Arrange
        var session = new SessionData { Id = Guid.NewGuid(), PlayerIds = new List<Guid>() };
        var playerId = Guid.NewGuid();

        // Act
        await _service.AddPlayer(playerId, session);

        // Assert
        Assert.Single(session.PlayerIds);
        Assert.Contains(playerId, session.PlayerIds);
        Assert.NotNull(session.StartTime);
        _mockClients.Verify(c => c.Group(session.Id.ToString()), Times.Once);
        _mockClientProxy.Verify(c => c.SendCoreAsync("PlayerJoined", It.Is<object[]>(o => (Guid)o[0] == playerId), default), Times.Once);
        _mockRepository.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_SecondPlayer_DoesNotUpdateStartTime()
    {
        // Arrange
        var existingPlayerId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-1);
        var session = new SessionData { Id = Guid.NewGuid(), PlayerIds = new List<Guid> { existingPlayerId }, StartTime = startTime };
        var newPlayerId = Guid.NewGuid();

        // Act
        await _service.AddPlayer(newPlayerId, session);

        // Assert
        Assert.Equal(2, session.PlayerIds.Count);
        Assert.Equal(startTime, session.StartTime);
        _mockRepository.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public async Task RemovePlayer_RemovesPlayerAndNotifies()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = Guid.NewGuid(), PlayerIds = new List<Guid> { playerId } };

        // Act
        await _service.RemovePlayer(playerId, session);

        // Assert
        Assert.Empty(session.PlayerIds);
        _mockClients.Verify(c => c.Group(session.Id.ToString()), Times.Once);
        _mockClientProxy.Verify(c => c.SendCoreAsync("PlayerRemoved", It.Is<object[]>(o => (Guid)o[0] == playerId), default), Times.Once);
        _mockRepository.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public async Task GetSessionPlayers_ReturnsPlayers()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var players = new List<Player> { new Player { Id = Guid.NewGuid() } };
        _mockRepository.Setup(r => r.GetSessionPlayers(sessionId)).ReturnsAsync(players);

        // Act
        var result = await _service.GetSessionPlayers(sessionId);

        // Assert
        Assert.Equal(players, result);
    }
}
