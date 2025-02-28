﻿using AppServiceDemo.Data.Contracts.Response;
using AppServiceDemo.Data.Entities;
using AppServiceDemo.Data.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppServiceDemo.Service
{
	public interface IGameSessionService
    {
        Task<NewGameResponse> CreateGameAsync(string playerName, string gameSessionName);
        Task<JoinGameResponse> JoinNewPlayerToGameAsync(string playerName, string gameSessionName);
        Task<GameSessionDataResponse> GetGameSessionDataByOwnerId(int ownerPlayerId);
    }

    public class GameSessionService : IGameSessionService
    {
        private readonly ILogger<GameSessionService> _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IPlayerService _playerService;

        public GameSessionService(
            ILogger<GameSessionService> logger,
            IConfiguration config,
            IPlayerRepository playerRepository,
            IPlayerService playerService, 
            IGameSessionRepository gameSessionRepository)
        {
            _logger = logger;
            _playerRepository = playerRepository;
            _playerService = playerService;
            _gameSessionRepository = gameSessionRepository;
        }

        public async Task<NewGameResponse> CreateGameAsync(string playerName, string gameSessionName)
        {
            _logger.LogInformation("Attempting to create a game {GameSessionName} requested by player {playerName}", gameSessionName, playerName);

            var player = new Player
            {
                Name = playerName
            };

            var gameSession = await _gameSessionRepository.AddAsync(new GameSession()
            {
                Name = gameSessionName,
                Players = new List<Player>() { player },
                JoinCode = Guid.NewGuid().ToString() // TODO: Generate codes not using Guids
            });

            return new NewGameResponse()
            {
                PlayerJWT = _playerService.GeneratePlayerJWT(player),
                GameJoinCode = gameSession.JoinCode
            };
        }

        public async Task<JoinGameResponse> JoinNewPlayerToGameAsync(string playerName, string joinCode)
        {
            var gameSession = await _gameSessionRepository.GetByJoinCodeAsync(joinCode);

            if (gameSession == null)
            {
                throw new ArgumentException($"Game for join code {joinCode} does not exist.");
            }

            if (gameSession.Players.Any(x => x.Name == playerName))
            {
                throw new ArgumentException($"Invalid name {playerName}. Player already exists in game {gameSession.Name}.");
            }

            var player = new Player
            {
                Name = playerName
            };

            gameSession.Players.Add(player);

            await _gameSessionRepository.UpdateAsync(gameSession);

            return new JoinGameResponse
            {
                PlayerJWT = _playerService.GeneratePlayerJWT(player)
            };
        }

        public async Task<GameSessionDataResponse> GetGameSessionDataByOwnerId(int ownerPlayerId)
        {
            var owner = await _playerRepository.GetWithGameSessionAsync(ownerPlayerId);

            return new GameSessionDataResponse
            {
                TeamName = owner.GameSession.Name,
                JoinCode = owner.GameSession.JoinCode
            };
        }
    }
}
