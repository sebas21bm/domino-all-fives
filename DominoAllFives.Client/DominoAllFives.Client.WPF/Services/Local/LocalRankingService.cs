using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.Services.Local
{
    public class LocalRankingService : IRankingService
    {
        private readonly RankingController _rankingController;

        public LocalRankingService(RankingController rankingController)
        {
            _rankingController = rankingController;
        }

        /// <inheritdoc/>
        public RankingResultDto GetTopRanking(int playerId)
        {
            return _rankingController.GetRanking(playerId);
        }
    }
}