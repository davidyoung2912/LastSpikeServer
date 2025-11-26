using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Xunit;

namespace GameplaySessionTracker.Tests.Repositories;

public class SessionPlayerRepositoryTests
{
    [Fact]
    public void GetAll_InitiallyEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var repository = new SessionPlayerRepository();

        // Act
        var result = repository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Add_AddsSessionPlayer_CanBeRetrieved()
    {
        // Arrange
        var repository = new SessionPlayerRepository();
        var sessionPlayer = new SessionPlayer
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            Data = "Test Data"
        };

        // Act
        repository.Add(sessionPlayer);
        var result = repository.GetById(sessionPlayer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionPlayer.Id, result.Id);
        Assert.Equal(sessionPlayer.SessionId, result.SessionId);
        Assert.Equal(sessionPlayer.PlayerId, result.PlayerId);
        Assert.Equal(sessionPlayer.Data, result.Data);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNull()
    {
        // Arrange
        var repository = new SessionPlayerRepository();

        // Act
        var result = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Update_ExistingSessionPlayer_UpdatesData()
    {
        // Arrange
        var repository = new SessionPlayerRepository();
        var sessionPlayer = new SessionPlayer
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            Data = "Original Data"
        };
        repository.Add(sessionPlayer);

        // Act
        sessionPlayer.Data = "Updated Data";
        repository.Update(sessionPlayer);
        var result = repository.GetById(sessionPlayer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Data", result.Data);
    }

    [Fact]
    public void Update_NonExistent_DoesNothing()
    {
        // Arrange
        var repository = new SessionPlayerRepository();
        var sessionPlayer = new SessionPlayer
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            Data = "Test Data"
        };

        // Act
        repository.Update(sessionPlayer);
        var result = repository.GetById(sessionPlayer.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Delete_ExistingSessionPlayer_RemovesFromCollection()
    {
        // Arrange
        var repository = new SessionPlayerRepository();
        var sessionPlayer = new SessionPlayer
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            Data = "Test Data"
        };
        repository.Add(sessionPlayer);

        // Act
        repository.Delete(sessionPlayer.Id);
        var result = repository.GetById(sessionPlayer.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Delete_NonExistent_DoesNothing()
    {
        // Arrange
        var repository = new SessionPlayerRepository();

        // Act & Assert (no exception should be thrown)
        repository.Delete(Guid.NewGuid());
    }

    [Fact]
    public void GetAll_MultipleItems_ReturnsAll()
    {
        // Arrange
        var repository = new SessionPlayerRepository();
        var sp1 = new SessionPlayer { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "Data1" };
        var sp2 = new SessionPlayer { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), Data = "Data2" };

        // Act
        repository.Add(sp1);
        repository.Add(sp2);
        var result = repository.GetAll().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Id == sp1.Id);
        Assert.Contains(result, s => s.Id == sp2.Id);
    }
}
