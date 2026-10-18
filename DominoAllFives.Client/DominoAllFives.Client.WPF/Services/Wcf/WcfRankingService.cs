using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    /// <summary>Retrieves ranking through the configured remote endpoint.</summary>
    public sealed class WcfRankingService : IRankingClient
    {
        public Ranking GetTopRanking(int playerId)
        {
            return WcfInvoker.Invoke<RankingRemoteServiceClient, Ranking>(
                () => new RankingRemoteServiceClient("RankingEndpoint"), service => service.GetTopRanking(playerId));
        }
    }
}
