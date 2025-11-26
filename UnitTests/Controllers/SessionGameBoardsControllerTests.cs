using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace GameplaySessionTracker.Tests.Controllers;

public class SessionGameBoardsControllerTests
{
    private readonly Mock<ISessionGameBoardService> _mockService;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<IGameBoardService> _mockBoardService;
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly SessionGameBoardsController _controller;

    public SessionGameBoardsControllerTests()
    {
        _mockService = new Mock<ISessionGameBoardService>();
        _mockSessionService = new Mock<ISessionService>();
        _mockBoardService = new Mock<IGameBoardService>();
        _mockHubContext = new Mock<IHubContext<GameHub>>();

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        /*
        mockClientProxy.Setup(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        */

        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);

        _controller = new SessionGameBoardsController(_mockService.Object, _mockSessionService.Object, _mockBoardService.Object, _mockHubContext.Object);
    }

    [Fact]
    public void GetAll_ReturnsOk()
    {
        _mockService.Setup(s => s.GetAll()).Returns(new List<SessionGameBoard>());
        var result = _controller.GetAll();
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_Existing_ReturnsOk()
    {
        var sgb = new SessionGameBoard { Id = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(sgb.Id)).Returns(sgb);
        var result = _controller.GetById(sgb.Id);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((SessionGameBoard?)null);
        var result = _controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidSessionId_ReturnsBadRequest()
    {
        var sgb = new SessionGameBoard { SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockSessionService.Setup(s => s.GetById(sgb.SessionId)).Returns((SessionData?)null);
        var result = await _controller.Create(sgb);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidBoardId_ReturnsBadRequest()
    {
        var sgb = new SessionGameBoard { SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockSessionService.Setup(s => s.GetById(sgb.SessionId)).Returns(new SessionData { Id = sgb.SessionId });
        _mockBoardService.Setup(s => s.GetById(sgb.BoardId)).Returns((GameBoard?)null);
        var result = await _controller.Create(sgb);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var sgb = new SessionGameBoard { SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockSessionService.Setup(s => s.GetById(sgb.SessionId)).Returns(new SessionData { Id = sgb.SessionId });
        _mockBoardService.Setup(s => s.GetById(sgb.BoardId)).Returns(new GameBoard { Id = sgb.BoardId });
        _mockService.Setup(s => s.Create(It.IsAny<SessionGameBoard>())).Returns(new SessionGameBoard { Id = Guid.NewGuid() });
        var result = await _controller.Create(sgb);
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Update_Valid_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = id, SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns(sgb);
        _mockSessionService.Setup(s => s.GetById(sgb.SessionId)).Returns(new SessionData { Id = sgb.SessionId });
        _mockBoardService.Setup(s => s.GetById(sgb.BoardId)).Returns(new GameBoard { Id = sgb.BoardId });
        var result = await _controller.Update(id, sgb);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = id, SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns((SessionGameBoard?)null);
        var result = await _controller.Update(id, sgb);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        var result = await _controller.Update(id, sgb);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_InvalidSessionId_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = id, SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns(sgb);
        _mockSessionService.Setup(s => s.GetById(sgb.SessionId)).Returns((SessionData?)null);
        var result = await _controller.Update(id, sgb);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_InvalidBoardId_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = id, SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns(sgb);
        _mockSessionService.Setup(s => s.GetById(sgb.SessionId)).Returns(new SessionData { Id = sgb.SessionId });
        _mockBoardService.Setup(s => s.GetById(sgb.BoardId)).Returns((GameBoard?)null);
        var result = await _controller.Update(id, sgb);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).Returns((SessionGameBoard?)null);
        var result = await _controller.Delete(id);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).Returns(new SessionGameBoard { Id = id });
        var result = await _controller.Delete(id);
        Assert.IsType<NoContentResult>(result);
    }
}
