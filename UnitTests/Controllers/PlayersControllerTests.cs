using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Controllers;

public class PlayersControllerTests
{
    private readonly Mock<IPlayerService> _mockService;
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _mockService = new Mock<IPlayerService>();
        _controller = new PlayersController(_mockService.Object);
    }

    [Fact]
    public void GetAll_ReturnsOkWithPlayers()
    {
        // Arrange
        var players = new List<Player> { new Player { Id = Guid.NewGuid(), Name = "Test", Alias = "T" } };
        _mockService.Setup(s => s.GetAll()).Returns(players);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(players, okResult.Value);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsOk()
    {
        // Arrange
        var player = new Player { Id = Guid.NewGuid(), Name = "Test", Alias = "T" };
        _mockService.Setup(s => s.GetById(player.Id)).Returns(player);

        // Act
        var result = _controller.GetById(player.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(player, okResult.Value);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((Player?)null);

        // Act
        var result = _controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_ValidPlayer_ReturnsCreated()
    {
        // Arrange
        var player = new Player { Name = "New", Alias = "N" };
        var createdPlayer = new Player { Id = Guid.NewGuid(), Name = "New", Alias = "N" };
        _mockService.Setup(s => s.GetAll()).Returns(new List<Player>());
        _mockService.Setup(s => s.Create(It.IsAny<Player>())).Returns(createdPlayer);

        // Act
        var result = _controller.Create(player);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(createdPlayer, createdResult.Value);
    }

    [Fact]
    public void Create_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var existing = new Player { Id = Guid.NewGuid(), Name = "Test", Alias = "T" };
        var newPlayer = new Player { Name = "Test", Alias = "Different" };
        _mockService.Setup(s => s.GetAll()).Returns(new List<Player> { existing });

        // Act
        var result = _controller.Create(newPlayer);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("name", badRequestResult.Value?.ToString()?.ToLower());
    }

    [Fact]
    public void Create_DuplicateAlias_ReturnsBadRequest()
    {
        // Arrange
        var existing = new Player { Id = Guid.NewGuid(), Name = "Different", Alias = "T" };
        var newPlayer = new Player { Name = "Test", Alias = "T" };
        _mockService.Setup(s => s.GetAll()).Returns(new List<Player> { existing });

        // Act
        var result = _controller.Create(newPlayer);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("alias", badRequestResult.Value?.ToString()?.ToLower());
    }

    [Fact]
    public void Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var player = new Player { Id = Guid.NewGuid(), Name = "Test", Alias = "T" };

        // Act
        var result = _controller.Update(Guid.NewGuid(), player);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var player = new Player { Id = id, Name = "Test", Alias = "T" };
        _mockService.Setup(s => s.GetById(id)).Returns((Player?)null);

        // Act
        var result = _controller.Update(id, player);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Update_ValidPlayer_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var player = new Player { Id = id, Name = "Test", Alias = "T" };
        _mockService.Setup(s => s.GetById(id)).Returns(player);
        _mockService.Setup(s => s.GetAll()).Returns(new List<Player> { player });

        // Act
        var result = _controller.Update(id, player);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Player { Id = Guid.NewGuid(), Name = "Existing", Alias = "E" };
        var player = new Player { Id = id, Name = "Existing", Alias = "T" };
        _mockService.Setup(s => s.GetById(id)).Returns(new Player { Id = id, Name = "Old", Alias = "T" });
        _mockService.Setup(s => s.GetAll()).Returns(new List<Player> { existing, player });

        // Act
        var result = _controller.Update(id, player);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("name", badRequestResult.Value?.ToString()?.ToLower());
    }

    [Fact]
    public void Delete_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var player = new Player { Id = id, Name = "Test", Alias = "T" };
        _mockService.Setup(s => s.GetById(id)).Returns(player);

        // Act
        var result = _controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((Player?)null);

        // Act
        var result = _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
