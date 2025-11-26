using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class SessionGameBoardServiceTests
{
    private readonly Mock<ISessionGameBoardRepository> _mockRepository;
    private readonly SessionGameBoardService _service;

    public SessionGameBoardServiceTests()
    {
        _mockRepository = new Mock<ISessionGameBoardRepository>();
        _service = new SessionGameBoardService(_mockRepository.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllSessionGameBoards()
    {
        // Arrange
        var sessionGameBoards = new List<SessionGameBoard>
        {
            new SessionGameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid(), Data = "Data1" },
            new SessionGameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid(), Data = "Data2" }
        };
        _mockRepository.Setup(r => r.GetAll()).Returns(sessionGameBoards);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsSessionGameBoard()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = id, SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid(), Data = "Test" };
        _mockRepository.Setup(r => r.GetById(id)).Returns(sgb);

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
        _mockRepository.Setup(r => r.GetById(id)).Returns((SessionGameBoard?)null);

        // Act
        var result = _service.GetById(id);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public void Create_ValidSessionGameBoard_CallsRepositoryAdd()
    {
        // Arrange
        var sgb = new SessionGameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid(), Data = "New" };

        // Act
        var result = _service.Create(sgb);

        // Assert
        Assert.Equal(sgb, result);
        _mockRepository.Verify(r => r.Add(sgb), Times.Once);
    }

    [Fact]
    public void Update_ValidSessionGameBoard_CallsRepositoryUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sgb = new SessionGameBoard { Id = id, SessionId = Guid.NewGuid(), BoardId = Guid.NewGuid(), Data = "Updated" };
        _mockRepository.Setup(r => r.GetById(id)).Returns(sgb);

        // Act
        _service.Update(id, sgb);

        // Assert
        _mockRepository.Verify(r => r.Update(sgb), Times.Once);
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
