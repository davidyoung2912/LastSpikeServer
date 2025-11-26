using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class GameBoardServiceTests
{
    private readonly Mock<IGameBoardRepository> _mockRepository;
    private readonly GameBoardService _service;

    public GameBoardServiceTests()
    {
        _mockRepository = new Mock<IGameBoardRepository>();
        _service = new GameBoardService(_mockRepository.Object);
    }

    [Fact]
    public void GetAll_ReturnsAllGameBoards()
    {
        // Arrange
        var gameBoards = new List<GameBoard>
        {
            new GameBoard { Id = Guid.NewGuid(), Description = "Board1", Data = "Data1" },
            new GameBoard { Id = Guid.NewGuid(), Description = "Board2", Data = "Data2" }
        };
        _mockRepository.Setup(r => r.GetAll()).Returns(gameBoards);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsGameBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var gameBoard = new GameBoard { Id = boardId, Description = "Test", Data = "TestData" };
        _mockRepository.Setup(r => r.GetById(boardId)).Returns(gameBoard);

        // Act
        var result = _service.GetById(boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.Id);
        _mockRepository.Verify(r => r.GetById(boardId), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetById(boardId)).Returns((GameBoard?)null);

        // Act
        var result = _service.GetById(boardId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(boardId), Times.Once);
    }

    [Fact]
    public void Create_ValidGameBoard_CallsRepositoryAdd()
    {
        // Arrange
        var gameBoard = new GameBoard { Id = Guid.NewGuid(), Description = "New", Data = "Data" };

        // Act
        var result = _service.Create(gameBoard);

        // Assert
        Assert.Equal(gameBoard, result);
        _mockRepository.Verify(r => r.Add(gameBoard), Times.Once);
    }

    [Fact]
    public void Update_ValidGameBoard_CallsRepositoryUpdate()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var gameBoard = new GameBoard { Id = boardId, Description = "Updated", Data = "Data" };
        _mockRepository.Setup(r => r.GetById(boardId)).Returns(gameBoard);

        // Act
        _service.Update(boardId, gameBoard);

        // Assert
        _mockRepository.Verify(r => r.Update(gameBoard), Times.Once);
    }

    [Fact]
    public void Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act
        _service.Delete(boardId);

        // Assert
        _mockRepository.Verify(r => r.Delete(boardId), Times.Once);
    }
}
