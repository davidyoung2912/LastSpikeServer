using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class PlayerServiceTests
{
    private readonly Mock<IPlayerRepository> _mockRepository;
    private readonly PlayerService _service;

    public PlayerServiceTests()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _service = new PlayerService(_mockRepository.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllPlayers()
    {
        // Arrange
        var players = new List<Player>
        {
            new Player { Id = Guid.NewGuid(), Name = "Player1" },
            new Player { Id = Guid.NewGuid(), Name = "Player2" }
        };
        _mockRepository.Setup(r => r.GetAll()).Returns(players);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsPlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, Name = "Test" };
        _mockRepository.Setup(r => r.GetById(playerId)).Returns(player);

        // Act
        var result = _service.GetById(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(playerId, result.Id);
        _mockRepository.Verify(r => r.GetById(playerId), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetById(playerId)).Returns((Player?)null);

        // Act
        var result = _service.GetById(playerId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(playerId), Times.Once);
    }

    [Fact]
    public void Create_ValidPlayer_CallsRepositoryAdd()
    {
        // Arrange
        var player = new Player { Id = Guid.NewGuid(), Name = "New Player" };

        // Act
        var result = _service.Create(player);

        // Assert
        Assert.Equal(player, result);
        _mockRepository.Verify(r => r.Add(player), Times.Once);
    }

    [Fact]
    public void Update_ValidPlayer_CallsRepositoryUpdate()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, Name = "Updated" };
        _mockRepository.Setup(r => r.GetById(playerId)).Returns(player);

        // Act
        _service.Update(playerId, player);

        // Assert
        _mockRepository.Verify(r => r.Update(player), Times.Once);
    }

    [Fact]
    public void Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        _service.Delete(playerId);

        // Assert
        _mockRepository.Verify(r => r.Delete(playerId), Times.Once);
    }
}
