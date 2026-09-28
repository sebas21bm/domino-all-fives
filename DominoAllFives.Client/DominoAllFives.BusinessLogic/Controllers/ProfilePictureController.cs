using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using System.IO;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates profile picture operations for player accounts.
    /// </summary>
    public class ProfilePictureController
    {
        /// <summary>
        /// Assigns a profile picture to a player account.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player.
        /// </param>
        /// <param name="filePath">
        /// The path of the selected profile picture.
        /// </param>
        /// <returns>
        /// True when the profile picture was successfully
        /// assigned; otherwise, false.
        /// </returns>
        public bool SetProfilePicture(
            int playerId,
            string filePath)
        {
            if (playerId <= 0 ||
                !ProfilePictureValidator.IsValid(filePath))
            {
                return false;
            }

            using (DominoAllFivesEntities context =
                new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository =
                    new PlayerRepository(context);

                Player player =
                    playerRepository.GetById(playerId);

                if (player == null)
                {
                    return false;
                }

                player.ProfilePicture =
                    Path.GetFileName(filePath);

                context.SaveChanges();

                return true;
            }
        }
    }
}