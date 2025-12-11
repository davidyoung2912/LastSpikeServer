using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Collections.Generic;
using System;

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
        var players = new List<Player> { new Player { Id = Guid.NewGuid(), Name = "Test" } };
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
        var player = new Player { Id = Guid.NewGuid(), Name = "Test" };
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
        var player = new Player { Name = "New" };
        var createdPlayer = new Player { Id = Guid.NewGuid(), Name = "New" };
        _mockService.Setup(s => s.Create(It.IsAny<Player>())).Returns(createdPlayer);

        // Act
        var result = _controller.Create(player);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(createdPlayer, createdResult.Value);
    }

    [Fact]
    public void Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var player = new Player { Id = Guid.NewGuid(), Name = "Test" };

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
        var player = new Player { Id = id, Name = "Test" };
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
        var player = new Player { Id = id, Name = "Test" };
        _mockService.Setup(s => s.GetById(id)).Returns(player);

        // Act
        var result = _controller.Update(id, player);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var player = new Player { Id = id, Name = "Test" };
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
