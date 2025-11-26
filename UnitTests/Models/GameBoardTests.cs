using System;
using Xunit;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Tests.Models;

public class GameBoardTests
{
    [Fact]
    public void GameBoard_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var gameBoard = new GameBoard();

        // Assert
        Assert.Equal(Guid.Empty, gameBoard.Id);
        Assert.Equal(string.Empty, gameBoard.Description);
        Assert.Equal(string.Empty, gameBoard.Data);
    }

    [Fact]
    public void GameBoard_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var gameBoard = new GameBoard();
        var id = Guid.NewGuid();
        var description = "Test Board";
        var data = "Test Data";

        // Act
        gameBoard.Id = id;
        gameBoard.Description = description;
        gameBoard.Data = data;

        // Assert
        Assert.Equal(id, gameBoard.Id);
        Assert.Equal(description, gameBoard.Description);
        Assert.Equal(data, gameBoard.Data);
    }
}
