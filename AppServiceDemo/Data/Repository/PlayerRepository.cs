﻿using AppServiceDemo.Data.Entities;
using AppServiceDemo.Data.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AppServiceDemo.Data.Repository
{
    public interface IPlayerRepository : IRepository<Player>
    {
        Task<Player> GetByNameAsync(string playerName);
        Task<Player> GetWithGameSessionAsync(int playerId);
    }

    public class PlayerRepository : BaseRepository<Player, ApplicationContext>, IPlayerRepository
    {
        public PlayerRepository(
            ApplicationContext context, 
            ILogger<PlayerRepository> logger) : base(context, logger)
        { }

        public async Task<Player> GetByNameAsync(string playerName)
        {
            return await _context.Players.FirstOrDefaultAsync(x => x.Name == playerName);
        }

        public async Task<Player> GetWithGameSessionAsync(int playerId)
        {
            return await _context.Players
                .Include(x => x.GameSession)
                .FirstOrDefaultAsync(x => x.Id == playerId);
        }
    }
}
