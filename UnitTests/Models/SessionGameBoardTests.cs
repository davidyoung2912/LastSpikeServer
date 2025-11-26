using System;
using Xunit;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Tests.Models;

public class SessionGameBoardTests
{
    [Fact]
    public void SessionGameBoard_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var sessionGameBoard = new SessionGameBoard();

        // Assert
        Assert.Equal(Guid.Empty, sessionGameBoard.Id);
        Assert.Equal(Guid.Empty, sessionGameBoard.SessionId);
        Assert.Equal(Guid.Empty, sessionGameBoard.BoardId);
        Assert.Equal(string.Empty, sessionGameBoard.Data);
    }

    [Fact]
    public void SessionGameBoard_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var sessionGameBoard = new SessionGameBoard();
        var id = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var data = "Test Data";

        // Act
        sessionGameBoard.Id = id;
        sessionGameBoard.SessionId = sessionId;
        sessionGameBoard.BoardId = boardId;
        sessionGameBoard.Data = data;

        // Assert
        Assert.Equal(id, sessionGameBoard.Id);
        Assert.Equal(sessionId, sessionGameBoard.SessionId);
        Assert.Equal(boardId, sessionGameBoard.BoardId);
        Assert.Equal(data, sessionGameBoard.Data);
    }
}
