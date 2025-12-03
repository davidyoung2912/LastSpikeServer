using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;
using GameplaySessionTracker.GameRules;


using static GameplaySessionTracker.GameRules.RuleEngine;

namespace GameplaySessionTracker.Services
{
    public class SessionGameBoardService(
        ISessionGameBoardRepository repository,
        IHubContext<GameHub> hubContext) : ISessionGameBoardService
    {

        public async Task<IEnumerable<SessionGameBoard>> GetAll()
        {
            return repository.GetAll();
        }

        public async Task<SessionGameBoard?> GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public async Task<SessionGameBoard> Create(SessionGameBoard sessionGameBoard)
        {
            repository.Add(sessionGameBoard);
            return sessionGameBoard;
        }

        public async Task TurnManager(Guid id, SessionGameBoard sessionGameBoard, GameState state)
        {
            // Notify the current player about their turn
            if (state.TurnPhase == TurnPhase.End)
            {
                state = EndTurn(state);
            }
            await hubContext.Clients.Group(sessionGameBoard.SessionId.ToString()).
                SendAsync("YourTurn", state.CurrentPlayerId.GetHashCode().ToString(), GetValidActions(state));

            sessionGameBoard.Data = SerializeGameState(state);
            await Update(id, sessionGameBoard);
        }

        public async Task Update(Guid id, SessionGameBoard sessionGameBoard)
        {
            repository.Update(sessionGameBoard);
            // Notify all players about the new state
            await hubContext.Clients.Group(sessionGameBoard.SessionId.ToString()).
                SendAsync("SessionGameBoardUpdated");
        }

        public async Task Delete(Guid id)
        {
            repository.Delete(id);
        }

        public async Task PlayerAction(Guid id, GameAction action)
        {
            var sessionGameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = DeserializeGameState(sessionGameBoard.Data);
            var onType = GameConstants.Spaces[state.Players[state.CurrentPlayerId].BoardPosition].Type;

            state = action switch
            {
                { Type: ActionType.Move } => Move(state),
                { Type: ActionType.Accept } when onType == SpaceType.Land => BuyProperty(state),
                { Type: ActionType.Accept } when onType == SpaceType.Track => BuyTrack(state),
                { Type: ActionType.Accept } when onType == SpaceType.SettlerRents => SettlerRents(state),
                { Type: ActionType.Accept } when onType == SpaceType.LandClaims => LandClaims(state),
                { Type: ActionType.Accept } when onType == SpaceType.Rebellion => StartRebellion(state),
                { Type: ActionType.Accept } when onType == SpaceType.SurveyFees => SurveyFees(state),
                { Type: ActionType.Accept } when onType == SpaceType.RoadbedCosts => RoadbedCosts(state),
                { Type: ActionType.Accept } when onType == SpaceType.EndOfTrack => EndOfTrack(state),
                { Type: ActionType.Accept } when onType == SpaceType.Go => Pass(state),
                { Type: ActionType.Rebellion, Target: CityPair target } => Rebellion(state, target),
                { Type: ActionType.PlaceTrack, Target: CityPair target } => PlaceTrack(state, target),
                { Type: ActionType.Pass } => Pass(state),
                { Type: ActionType.Trade } => state, // TODO: implement RuleEngine.Trade(state),
                _ => throw new ArgumentException("Invalid action type")
            };
            sessionGameBoard.Data = SerializeGameState(state);
            await Update(id, sessionGameBoard);
        }
    }
}
