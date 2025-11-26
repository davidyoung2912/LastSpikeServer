using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class SessionPlayerServiceTests
{
    private readonly Mock<ISessionPlayerRepository> _mockRepository;
    private readonly SessionPlayerService _service;

    public SessionPlayerServiceTests()
    {
        _mockRepository = new Mock<ISessionPlayerRepository>();
        _service = new SessionPlayerService(_mockRepository.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllSessionPlayers()
    {
        // Arrange
        var sessionPlayers = new List<SessionPlayer>
        {
            new SessionPlayer { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "Data1" },
            new SessionPlayer { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "Data2" }
        };
        _mockRepository.Setup(r => r.GetAll()).Returns(sessionPlayers);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsSessionPlayer()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = id, SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "Test" };
        _mockRepository.Setup(r => r.GetById(id)).Returns(sp);

        // Act
        var result = _service.GetById(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetById(id)).Returns((SessionPlayer?)null);

        // Act
        var result = _service.GetById(id);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public void Create_ValidSessionPlayer_CallsRepositoryAdd()
    {
        // Arrange
        var sp = new SessionPlayer { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "New" };

        // Act
        var result = _service.Create(sp);

        // Assert
        Assert.Equal(sp, result);
        _mockRepository.Verify(r => r.Add(sp), Times.Once);
    }

    [Fact]
    public void Update_ValidSessionPlayer_CallsRepositoryUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = id, SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "Updated" };
        _mockRepository.Setup(r => r.GetById(id)).Returns(sp);

        // Act
        _service.Update(id, sp);

        // Assert
        _mockRepository.Verify(r => r.Update(sp), Times.Once);
    }

    [Fact]
    public void Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _service.Delete(id);

        // Assert
        _mockRepository.Verify(r => r.Delete(id), Times.Once);
    }
}
