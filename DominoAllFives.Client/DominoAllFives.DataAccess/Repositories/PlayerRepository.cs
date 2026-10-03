using System.Linq;

using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;

namespace DominoAllFives.DataAccess.Repositories
{
    /// <summary>
    /// Provides Entity Framework persistence operations for player accounts.
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly DominoAllFivesEntities _context;

        /// <summary>
        /// Initializes a new instance of the repository with the specified database context.
        /// </summary>
        /// <param name="databaseContext">
        /// The database context used by the repository.
        /// </param>
        public PlayerRepository(DominoAllFivesEntities databaseContext)
        {
            _context = databaseContext;
        }

        /// <inheritdoc />
        public Player GetByUsernameOrEmail(string identifier)
        {
            return _context.Player.FirstOrDefault(
                player =>
                    player.Username == identifier ||
                    player.Email == identifier);
        }

        /// <inheritdoc />
        public Player GetById(int playerId)
        {
            return _context.Player.FirstOrDefault(
                player => player.IdPlayer == playerId);
        }

        /// <inheritdoc />
        public bool UsernameExists(string username)
        {
            return _context.Player.Any(
                player => player.Username == username);
        }

        /// <inheritdoc />
        public bool EmailExists(string email)
        {
            return _context.Player.Any(
                player => player.Email == email);
        }

        /// <inheritdoc />
        public void Add(Player playerToAdd)
        {
            _context.Player.Add(playerToAdd);
        }
    }
}