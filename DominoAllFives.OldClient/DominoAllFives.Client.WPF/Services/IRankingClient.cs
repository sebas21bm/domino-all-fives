using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services
{
    public interface IRankingClient
    {
        Ranking GetTopRanking(int playerId);
    }
}
