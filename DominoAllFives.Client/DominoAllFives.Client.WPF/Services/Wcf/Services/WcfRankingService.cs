using System.Collections.Generic;
using DominoAllFivesService;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>Retrieves ranking through the configured remote endpoint.</summary>
    public sealed class WcfRankingService : IRankingClient
    {
        public List<RankingEntry> GetTopWinsRanking()
        {
            return WcfInvoker.Invoke<RankingRemoteServiceClient, List<RankingEntry>>(
                () => new RankingRemoteServiceClient("RankingEndpoint"), service => service.GetTopWinsRanking());
        }

        public int GetWinsRankingPositionById(int playerId)
        {
            return WcfInvoker.Invoke<RankingRemoteServiceClient, int>(
                () => new RankingRemoteServiceClient("RankingEndpoint"),
                service => service.GetWinsRankingPositionById(playerId));
        }
    }
}

