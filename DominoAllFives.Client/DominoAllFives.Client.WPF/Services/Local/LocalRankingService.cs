using System;

using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.Services.Local
{
    public class LocalRankingService : IRankingService
    {
        private readonly RankingController _rankingController;

        public LocalRankingService(RankingController rankingController)
        {
            _rankingController = rankingController
                ?? throw new ArgumentNullException(nameof(rankingController));
        }

        /// <inheritdoc/>
        public Ranking GetTopRanking(int playerId)
        {
            return _rankingController.GetRanking(playerId);
        }
    }
}