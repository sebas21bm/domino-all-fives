using System.Collections.Generic;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    public interface IRankingClient
    {
        List<RankingEntry> GetTopWinsRanking();

        int GetWinsRankingPositionById(int playerId);
    }
}

