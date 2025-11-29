using Xunit;
using GameplaySessionTracker.GameRules;
using GameplaySessionTracker.Models;
using System.Collections.Generic;
using System;

namespace GameplaySessionTracker.Tests.GameRules
{
    public class RuleEngineTests
    {
        [Fact]
        public void BuyProperty_AddsPropertyAndDeductsMoney()
        {
            // Arrange
            var player = new PlayerState(Guid.NewGuid(), 10000, 0, false);
            var state = new GameState(
                new List<PlayerState> { player },
                new List<Route>(),
                new List<Property>(),
                false,
                0
            );
            int cost = 5000;

            // Act
            var newState = RuleEngine.BuyProperty(state, cost);

            // Assert
            Assert.Single(newState.Properties);
            Assert.Equal(player.PID, newState.Properties[0].Owner_PID);
            Assert.Equal(10000 - cost, newState.Players[0].Money);
        }
    }
}
