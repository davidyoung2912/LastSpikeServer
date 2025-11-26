using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _mockRepository;
    private readonly SessionService _service;

    public SessionServiceTests()
    {
        _mockRepository = new Mock<ISessionRepository>();
        _service = new SessionService(_mockRepository.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllSessions()
    {
        // Arrange
        var sessions = new List<SessionData>
        {
            new SessionData { Id = Guid.NewGuid(), Description = "Session1", BoardId = Guid.NewGuid() },
            new SessionData { Id = Guid.NewGuid(), Description = "Session2", BoardId = Guid.NewGuid() }
        };
        _mockRepository.Setup(r => r.GetAll()).Returns(sessions);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, Description = "Test", BoardId = Guid.NewGuid() };
        _mockRepository.Setup(r => r.GetById(sessionId)).Returns(session);

        // Act
        var result = _service.GetById(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.Id);
        _mockRepository.Verify(r => r.GetById(sessionId), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetById(sessionId)).Returns((SessionData?)null);

        // Act
        var result = _service.GetById(sessionId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(sessionId), Times.Once);
    }

    [Fact]
    public void Create_ValidSession_CallsRepositoryAdd()
    {
        // Arrange
        var session = new SessionData { Id = Guid.NewGuid(), Description = "New", BoardId = Guid.NewGuid() };

        // Act
        var result = _service.Create(session);

        // Assert
        Assert.Equal(session, result);
        _mockRepository.Verify(r => r.Add(session), Times.Once);
    }

    [Fact]
    public void Update_ValidSession_CallsRepositoryUpdate()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, Description = "Updated", BoardId = Guid.NewGuid() };
        _mockRepository.Setup(r => r.GetById(sessionId)).Returns(session);

        // Act
        _service.Update(sessionId, session);

        // Assert
        _mockRepository.Verify(r => r.Update(session), Times.Once);
    }

    [Fact]
    public void Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        _service.Delete(sessionId);

        // Assert
        _mockRepository.Verify(r => r.Delete(sessionId), Times.Once);
    }
}
