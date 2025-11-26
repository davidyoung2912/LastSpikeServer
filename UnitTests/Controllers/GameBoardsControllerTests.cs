using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Controllers;

public class GameBoardsControllerTests
{
    private readonly Mock<IGameBoardService> _mockService;
    private readonly GameBoardsController _controller;

    public GameBoardsControllerTests()
    {
        _mockService = new Mock<IGameBoardService>();
        _controller = new GameBoardsController(_mockService.Object);
    }

    [Fact]
    public void GetAll_ReturnsOk()
    {
        var gameBoards = new List<GameBoard> { new GameBoard { Id = Guid.NewGuid() } };
        _mockService.Setup(s => s.GetAll()).Returns(gameBoards);
        var result = _controller.GetAll();
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_Existing_ReturnsOk()
    {
        var board = new GameBoard { Id = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(board.Id)).Returns(board);
        var result = _controller.GetById(board.Id);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((GameBoard?)null);
        var result = _controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_Valid_ReturnsCreated()
    {
        var board = new GameBoard { Description = "New", Data = "Data" };
        var created = new GameBoard { Id = Guid.NewGuid(), Description = "New", Data = "Data" };
        _mockService.Setup(s => s.Create(It.IsAny<GameBoard>())).Returns(created);
        var result = _controller.Create(board);
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public void Update_Valid_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var board = new GameBoard { Id = id };
        _mockService.Setup(s => s.GetById(id)).Returns(board);
        var result = _controller.Update(id, board);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).Returns((GameBoard?)null);
        var result = _controller.Update(id, new GameBoard { Id = id });
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).Returns(new GameBoard { Id = id });
        var result = _controller.Delete(id);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_NonExistent_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((GameBoard?)null);
        var result = _controller.Delete(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }
}
