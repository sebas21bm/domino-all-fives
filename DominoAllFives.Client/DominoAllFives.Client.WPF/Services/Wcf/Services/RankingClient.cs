using System.Collections.Generic;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>
    /// Retrieves ranking through the configured remote endpoint.
    /// </summary>
    public sealed class RankingRemoteService : IRankingClient
    {
        public List<RankingEntry> GetTopWinsRanking()
        {
            return WcfInvoker.Invoke<RankingRemoteServiceClient, List<RankingEntry>>(
                () => new RankingRemoteServiceClient(
                "RankingEndpoint"),
                proxy => proxy.GetTopWinsRanking());
        }

        public int GetWinsRankingPositionById(int playerId)
        {
            return WcfInvoker.Invoke<RankingRemoteServiceClient, int>(
                () => new RankingRemoteServiceClient(
                "RankingEndpoint"),
                proxy => proxy.GetWinsRankingPositionById(
                playerId));
        }
    }
}
