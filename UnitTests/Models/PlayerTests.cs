using System;
using Xunit;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Tests.Models;

public class PlayerTests
{
    [Fact]
    public void Player_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var player = new Player();

        // Assert
        Assert.Equal(Guid.Empty, player.Id);
        Assert.Equal(string.Empty, player.Name);
    }

    [Fact]
    public void Player_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var player = new Player();
        var id = Guid.NewGuid();
        var name = "Test Player";

        // Act
        player.Id = id;
        player.Name = name;

        // Assert
        Assert.Equal(id, player.Id);
        Assert.Equal(name, player.Name);
    }
}
