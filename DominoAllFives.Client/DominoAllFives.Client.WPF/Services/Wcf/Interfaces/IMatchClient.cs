using System;
using MatchService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    /// <summary>
    /// Provides match operations and snapshots independently of the WCF transport.
    /// </summary>
    public interface IMatchClient : IDisposable
    {
        event Action<MatchView> MatchChanged;
        event Action<PlayerHand> HandChanged;
        event Action ConnectionLost;

        MatchView Open(string sessionKey, int matchId);
        MatchView GetMatch(int matchId);
        PlayerHand GetHand(int matchId);
        void Play(int matchId, DominoTile tile, BoardEnd end, long version);
        void Draw(int matchId, long version);
        void Pass(int matchId, long version);
        void Leave(int matchId);
    }
}
