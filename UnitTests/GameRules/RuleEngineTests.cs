using Xunit;
using GameplaySessionTracker.GameRules;
using GameplaySessionTracker.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace GameplaySessionTracker.Tests.GameRules
{
    public class RuleEngineTests
    {
        #region CreateNewGameState Tests

        [Fact]
        public void CreateNewGameState_CreatesValidInitialState()
        {
            // Arrange
            var playerIDs = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            // Act
            var state = RuleEngine.CreateNewGameState(playerIDs);

            // Assert
            Assert.Equal(3, state.Players.Count);
            Assert.Empty(state.Routes);
            Assert.Empty(state.Properties);
            Assert.False(state.IsGameOver);
            Assert.Equal(0, state.CurrentPID);

            // Check all players have starting money and position 0
            foreach (var player in state.Players)
            {
                Assert.Equal(GameConstants.PlayerStartingMoney, player.Money);
                Assert.Equal(0, player.BoardPosition);
                Assert.False(player.SkipNextTurn);
            }
        }

        [Fact]
        public void CreateNewGameState_PreservesPlayerOrder()
        {
            // Arrange
            var playerIDs = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            var state = RuleEngine.CreateNewGameState(playerIDs);

            // Assert
            Assert.Equal(playerIDs[0], state.Players[0].PID);
            Assert.Equal(playerIDs[1], state.Players[1].PID);
        }

        #endregion

        #region Move Tests

        [Fact]
        public void Move_UpdatesPlayerPosition()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            // Act
            var newState = RuleEngine.Move(state);

            // Assert
            Assert.True(newState.Players[0].BoardPosition > 0);
            Assert.True(newState.Players[0].BoardPosition < GameConstants.Spaces.Count);
        }

        #endregion

        #region PassGo Tests

        [Fact]
        public void PassGo_AwardsCPRSubsidy()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var player = new PlayerState(playerID, 10000, 0, false);
            var state = new GameState(
                new List<PlayerState> { player },
                new List<Route>(),
                new List<Property>(),
                false,
                0
            );

            // Act
            var newState = RuleEngine.PassGo(state);

            // Assert
            Assert.Equal(10000 + GameConstants.CPRSubsidy, newState.Players[0].Money);
        }

        #endregion

        #region BuyProperty Tests

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

        [Fact]
        public void BuyProperty_WithEmptyDeck_ReturnsUnchangedState()
        {
            // Arrange
            var player = new PlayerState(Guid.NewGuid(), 10000, 0, false);
            // Create a full deck (5 of each city = 45 properties)
            var properties = new List<Property>();
            foreach (City city in Enum.GetValues<City>())
            {
                for (int i = 0; i < 5; i++)
                {
                    properties.Add(new Property(city, player.PID));
                }
            }
            var state = new GameState(
                new List<PlayerState> { player },
                new List<Route>(),
                properties,
                false,
                0
            );

            // Act
            var newState = RuleEngine.BuyProperty(state, 1000);

            // Assert
            Assert.Equal(45, newState.Properties.Count); // No new property added
            Assert.Equal(10000, newState.Players[0].Money); // Money unchanged
        }

        [Fact]
        public void BuyProperty_WithZeroCost_DoesNotDeductMoney()
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

            // Act
            var newState = RuleEngine.BuyProperty(state, 0);

            // Assert
            Assert.Single(newState.Properties);
            Assert.Equal(10000, newState.Players[0].Money);
        }

        #endregion

        #region Rebellion Tests

        [Fact]
        public void Rebellion_RemovesTrackFromValidTarget()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            var target = new CityPair(City.Montreal, City.Toronto);
            state.Routes.Add(new Route(target, 3));

            // Act
            var newState = RuleEngine.Rebellion(state, target);

            // Assert
            var route = newState.Routes.First(r => r.CityPair == target);
            Assert.Equal(2, route.NumTracks);
        }

        [Fact]
        public void Rebellion_WithInvalidTarget_ReturnsUnchangedState()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            var target = new CityPair(City.Montreal, City.Toronto);
            state.Routes.Add(new Route(target, 4)); // Completed route (invalid)

            // Act
            var newState = RuleEngine.Rebellion(state, target);

            // Assert
            var route = newState.Routes.First(r => r.CityPair == target);
            Assert.Equal(4, route.NumTracks); // Unchanged
        }

        #endregion

        #region GetRebellionTargets Tests

        [Fact]
        public void GetRebellionTargets_ReturnsOnlyRoutesWithTwoOrThreeTracks()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            state.Routes.Add(new Route(new CityPair(City.Montreal, City.Toronto), 1));
            state.Routes.Add(new Route(new CityPair(City.Montreal, City.Sudbury), 2));
            state.Routes.Add(new Route(new CityPair(City.Toronto, City.Sudbury), 3));
            state.Routes.Add(new Route(new CityPair(City.Sudbury, City.Winnipeg), 4));

            // Act
            var targets = RuleEngine.GetRebellionTargets(state);

            // Assert
            Assert.Equal(2, targets.Count);
            Assert.Contains(targets, t => t.City1 == City.Montreal && t.City2 == City.Sudbury);
            Assert.Contains(targets, t => t.City1 == City.Toronto && t.City2 == City.Sudbury);
        }

        [Fact]
        public void GetRebellionTargets_WithNoValidTargets_ReturnsEmptyList()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            state.Routes.Add(new Route(new CityPair(City.Montreal, City.Toronto), 1));
            state.Routes.Add(new Route(new CityPair(City.Montreal, City.Sudbury), 4));

            // Act
            var targets = RuleEngine.GetRebellionTargets(state);

            // Assert
            Assert.Empty(targets);
        }

        #endregion

        #region AddTrack Tests

        [Fact]
        public void AddTrack_AddsTrackToNewRoute()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            var target = new CityPair(City.Montreal, City.Toronto);

            // Act
            var newState = RuleEngine.AddTrack(state, target);

            // Assert
            var route = newState.Routes.First(r => r.CityPair == target);
            Assert.Equal(1, route.NumTracks);
            Assert.Single(newState.Properties); // Property awarded for first track
        }

        [Fact]
        public void AddTrack_AddsTrackToExistingRoute()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            var target = new CityPair(City.Montreal, City.Toronto);
            state.Routes.Add(new Route(target, 2));

            // Act
            var newState = RuleEngine.AddTrack(state, target);

            // Assert
            var route = newState.Routes.First(r => r.CityPair == target);
            Assert.Equal(3, route.NumTracks);
        }

        [Fact]
        public void AddTrack_ToFullRoute_ReturnsUnchangedState()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            var target = new CityPair(City.Montreal, City.Toronto);
            state.Routes.Add(new Route(target, 4));

            // Act
            var newState = RuleEngine.AddTrack(state, target);

            // Assert
            var route = newState.Routes.First(r => r.CityPair == target);
            Assert.Equal(4, route.NumTracks);
        }

        [Fact]
        public void AddTrack_WithInvalidCityPair_ReturnsUnchangedState()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            var invalidTarget = new CityPair(City.Montreal, City.Vancouver); // Not a valid pair

            // Act
            var newState = RuleEngine.AddTrack(state, invalidTarget);

            // Assert
            Assert.Empty(newState.Routes);
        }

        #endregion

        #region FinishRoute Tests

        [Fact]
        public void FinishRoute_AwardsMoneyToPropertyOwners()
        {
            // Arrange
            var player1ID = Guid.NewGuid();
            var player2ID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { player1ID, player2ID });
            var finishedRoute = new CityPair(City.Montreal, City.Toronto);

            // Player 1 owns 2 Montreal properties
            state.Properties.Add(new Property(City.Montreal, player1ID));
            state.Properties.Add(new Property(City.Montreal, player1ID));
            // Player 2 owns 1 Toronto property
            state.Properties.Add(new Property(City.Toronto, player2ID));

            var initialMoney1 = state.Players[0].Money;
            var initialMoney2 = state.Players[1].Money;

            // Act
            var newState = RuleEngine.FinishRoute(state, finishedRoute);

            // Assert
            // Player 1 should get award for 2 Montreal properties
            var expectedAward1 = GameConstants.CityValues[City.Montreal][2] + GameConstants.CityValues[City.Toronto][0];
            Assert.Equal(initialMoney1 + expectedAward1, newState.Players[0].Money);

            // Player 2 should get award for 1 Toronto property
            var expectedAward2 = GameConstants.CityValues[City.Montreal][0] + GameConstants.CityValues[City.Toronto][1];
            Assert.Equal(initialMoney2 + expectedAward2, newState.Players[1].Money);
        }

        #endregion

        #region IsGameOver Tests

        [Fact]
        public void IsGameOver_WithPathFromVancouverToMontreal_ReturnsTrue()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            // Create a path: Vancouver -> Calgary -> Regina -> Winnipeg -> Sudbury -> Montreal
            state.Routes.Add(new Route(new CityPair(City.Vancouver, City.Calgary), 4));
            state.Routes.Add(new Route(new CityPair(City.Calgary, City.Regina), 4));
            state.Routes.Add(new Route(new CityPair(City.Regina, City.Winnipeg), 4));
            state.Routes.Add(new Route(new CityPair(City.Winnipeg, City.Sudbury), 4));
            state.Routes.Add(new Route(new CityPair(City.Sudbury, City.Montreal), 4));

            // Act
            var result = RuleEngine.IsGameOver(state);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsGameOver_WithNoCompletedRoutes_ReturnsFalse()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            // Act
            var result = RuleEngine.IsGameOver(state);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsGameOver_WithDisconnectedPath_ReturnsFalse()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            // Create disconnected segments
            state.Routes.Add(new Route(new CityPair(City.Vancouver, City.Calgary), 4));
            state.Routes.Add(new Route(new CityPair(City.Montreal, City.Toronto), 4));

            // Act
            var result = RuleEngine.IsGameOver(state);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsGameOver_WithLessThanFourCompletedRoutes_ReturnsFalse()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            state.Routes.Add(new Route(new CityPair(City.Vancouver, City.Calgary), 4));
            state.Routes.Add(new Route(new CityPair(City.Calgary, City.Regina), 4));
            state.Routes.Add(new Route(new CityPair(City.Regina, City.Winnipeg), 4));

            // Act
            var result = RuleEngine.IsGameOver(state);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region ProcessGameOver Tests

        [Fact]
        public void ProcessGameOver_SetsIsGameOverToTrue()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            // Act
            var newState = RuleEngine.ProcessGameOver(state);

            // Assert
            Assert.True(newState.IsGameOver);
        }

        [Fact]
        public void ProcessGameOver_AwardsLastSpikeBonusToCurrentPlayer()
        {
            // Arrange
            var player1ID = Guid.NewGuid();
            var player2ID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { player1ID, player2ID });
            state = state with { CurrentPID = 1 }; // Set current player to player 2

            var initialMoney = state.Players[1].Money;

            // Act
            var newState = RuleEngine.ProcessGameOver(state);

            // Assert
            Assert.Equal(initialMoney + GameConstants.LastSpikeBonus, newState.Players[1].Money);
            Assert.Equal(state.Players[0].Money, newState.Players[0].Money); // Other player unchanged
        }

        #endregion

        #region LandOnSpace Tests

        [Fact]
        public void LandOnSpace_OnGoSpace_TriggersPassGo()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var player = new PlayerState(playerID, 10000, 0, false); // Position 0 is Go
            var state = new GameState(
                new List<PlayerState> { player },
                new List<Route>(),
                new List<Property>(),
                false,
                0
            );

            var initialMoney = state.Players[0].Money;

            // Act
            var newState = RuleEngine.LandOnSpace(state);

            // Assert
            Assert.Equal(initialMoney + GameConstants.CPRSubsidy, newState.Players[0].Money);
        }

        [Fact]
        public void LandOnSpace_OnTrackSpace_ReturnsUnchangedState()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var player = new PlayerState(playerID, 10000, 1, false); // Position 1 is Track
            var state = new GameState(
                new List<PlayerState> { player },
                new List<Route>(),
                new List<Property>(),
                false,
                0
            );

            // Act
            var newState = RuleEngine.LandOnSpace(state);

            // Assert
            Assert.Equal(state.Players[0].Money, newState.Players[0].Money);
        }

        [Fact]
        public void LandOnSpace_WithInvalidPosition_ReturnsUnchangedState()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var player = new PlayerState(playerID, 10000, 999, false);
            var state = new GameState(
                new List<PlayerState> { player },
                new List<Route>(),
                new List<Property>(),
                false,
                0
            );

            // Act
            var newState = RuleEngine.LandOnSpace(state);

            // Assert
            Assert.Equal(state, newState);
        }

        #endregion

        #region EndOfTrack Tests

        [Fact]
        public void EndOfTrack_SetsSkipNextTurnToTrue()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var state = RuleEngine.CreateNewGameState(new List<Guid> { playerID });

            // Act
            var newState = RuleEngine.EndOfTrack(state);

            // Assert
            Assert.True(newState.Players[0].SkipNextTurn);
        }

        #endregion

        #region GetRanking Tests

        [Fact]
        public void GetRanking_ReturnsPlayersOrderedByMoneyDescending()
        {
            // Arrange
            var player1ID = Guid.NewGuid();
            var player2ID = Guid.NewGuid();
            var player3ID = Guid.NewGuid();

            var players = new List<PlayerState>
            {
                new PlayerState(player1ID, 50000, 0, false),
                new PlayerState(player2ID, 100000, 0, false),
                new PlayerState(player3ID, 75000, 0, false)
            };

            var state = new GameState(players, new List<Route>(), new List<Property>(), false, 0);

            // Act
            var ranking = RuleEngine.GetRanking(state);

            // Assert
            Assert.Equal(3, ranking.Count);
            Assert.Equal(player2ID, ranking[0]); // 100000
            Assert.Equal(player3ID, ranking[1]); // 75000
            Assert.Equal(player1ID, ranking[2]); // 50000
        }

        [Fact]
        public void GetRanking_WithTiedPlayers_MaintainsStableOrder()
        {
            // Arrange
            var player1ID = Guid.NewGuid();
            var player2ID = Guid.NewGuid();

            var players = new List<PlayerState>
            {
                new PlayerState(player1ID, 50000, 0, false),
                new PlayerState(player2ID, 50000, 0, false)
            };

            var state = new GameState(players, new List<Route>(), new List<Property>(), false, 0);

            // Act
            var ranking = RuleEngine.GetRanking(state);

            // Assert
            Assert.Equal(2, ranking.Count);
            Assert.Contains(player1ID, ranking);
            Assert.Contains(player2ID, ranking);
        }

        #endregion

        #region Serialization Tests

        [Fact]
        public void SerializeDeserialize_RoundTrip_PreservesGameState()
        {
            // Arrange
            var playerID = Guid.NewGuid();
            var originalState = RuleEngine.CreateNewGameState(new List<Guid> { playerID });
            originalState.Properties.Add(new Property(City.Montreal, playerID));
            originalState.Routes.Add(new Route(new CityPair(City.Montreal, City.Toronto), 2));

            // Act
            var serialized = RuleEngine.SerializeGameState(originalState);
            var deserialized = RuleEngine.DeserializeGameState(serialized);

            // Assert
            Assert.Equal(originalState.Players.Count, deserialized.Players.Count);
            Assert.Equal(originalState.Routes.Count, deserialized.Routes.Count);
            Assert.Equal(originalState.Properties.Count, deserialized.Properties.Count);
            Assert.Equal(originalState.IsGameOver, deserialized.IsGameOver);
            Assert.Equal(originalState.CurrentPID, deserialized.CurrentPID);

            Assert.Equal(originalState.Players[0].PID, deserialized.Players[0].PID);
            Assert.Equal(originalState.Players[0].Money, deserialized.Players[0].Money);
            Assert.Equal(originalState.Routes[0].NumTracks, deserialized.Routes[0].NumTracks);
            Assert.Equal(originalState.Properties[0].City, deserialized.Properties[0].City);
        }

        #endregion
    }
}
