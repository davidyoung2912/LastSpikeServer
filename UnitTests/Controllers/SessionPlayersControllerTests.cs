using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Threading;

namespace GameplaySessionTracker.Tests.Controllers;

public class SessionPlayersControllerTests
{
    private readonly Mock<ISessionPlayerService> _mockService;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly SessionPlayersController _controller;

    public SessionPlayersControllerTests()
    {
        _mockService = new Mock<ISessionPlayerService>();
        _mockSessionService = new Mock<ISessionService>();
        _mockPlayerService = new Mock<IPlayerService>();
        _mockHubContext = new Mock<IHubContext<GameHub>>();

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClientProxy.Setup(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);

        _controller = new SessionPlayersController(_mockService.Object, _mockSessionService.Object, _mockPlayerService.Object, _mockHubContext.Object);
    }

    [Fact]
    public void GetAll_ReturnsOk()
    {
        _mockService.Setup(s => s.GetAll()).Returns(new List<SessionPlayer>());
        var result = _controller.GetAll();
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_Existing_ReturnsOk()
    {
        var sp = new SessionPlayer { Id = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(sp.Id)).Returns(sp);
        var result = _controller.GetById(sp.Id);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((SessionPlayer?)null);
        var result = _controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetBySessionId_ValidSession_ReturnsOk()
    {
        var sessionId = Guid.NewGuid();
        _mockSessionService.Setup(s => s.GetById(sessionId)).Returns(new SessionData { Id = sessionId });
        _mockService.Setup(s => s.GetAll()).Returns(new List<SessionPlayer> { new SessionPlayer { SessionId = sessionId } });
        var result = _controller.GetBySessionId(sessionId);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetBySessionId_InvalidSession_ReturnsNotFound()
    {
        var sessionId = Guid.NewGuid();
        _mockSessionService.Setup(s => s.GetById(sessionId)).Returns((SessionData?)null);
        var result = _controller.GetBySessionId(sessionId);
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidSessionId_ReturnsBadRequest()
    {
        var sp = new SessionPlayer { SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockSessionService.Setup(s => s.GetById(sp.SessionId)).Returns((SessionData?)null);
        var result = await _controller.Create(sp);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidPlayerId_ReturnsBadRequest()
    {
        var sp = new SessionPlayer { SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockSessionService.Setup(s => s.GetById(sp.SessionId)).Returns(new SessionData { Id = sp.SessionId });
        _mockPlayerService.Setup(s => s.GetById(sp.PlayerId)).Returns((Player?)null);
        var result = await _controller.Create(sp);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var sp = new SessionPlayer { SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockSessionService.Setup(s => s.GetById(sp.SessionId)).Returns(new SessionData { Id = sp.SessionId });
        _mockPlayerService.Setup(s => s.GetById(sp.PlayerId)).Returns(new Player { Id = sp.PlayerId });
        _mockService.Setup(s => s.Create(It.IsAny<SessionPlayer>())).Returns(new SessionPlayer { Id = Guid.NewGuid() });
        var result = await _controller.Create(sp);
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Update_Valid_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = id, SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns(sp);
        _mockSessionService.Setup(s => s.GetById(sp.SessionId)).Returns(new SessionData { Id = sp.SessionId });
        _mockPlayerService.Setup(s => s.GetById(sp.PlayerId)).Returns(new Player { Id = sp.PlayerId });
        var result = await _controller.Update(id, sp);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = id, SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns((SessionPlayer?)null);
        var result = await _controller.Update(id, sp);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        var result = await _controller.Update(id, sp);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_InvalidSessionId_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = id, SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns(sp);
        _mockSessionService.Setup(s => s.GetById(sp.SessionId)).Returns((SessionData?)null);
        var result = await _controller.Update(id, sp);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_InvalidPlayerId_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var sp = new SessionPlayer { Id = id, SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).Returns(sp);
        _mockSessionService.Setup(s => s.GetById(sp.SessionId)).Returns(new SessionData { Id = sp.SessionId });
        _mockPlayerService.Setup(s => s.GetById(sp.PlayerId)).Returns((Player?)null);
        var result = await _controller.Update(id, sp);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).Returns((SessionPlayer?)null);
        var result = await _controller.Delete(id);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).Returns(new SessionPlayer { Id = id });
        var result = await _controller.Delete(id);
        Assert.IsType<NoContentResult>(result);
    }
}
