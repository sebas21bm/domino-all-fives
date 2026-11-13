using System.Collections.Concurrent;
using System.ServiceModel;
using Proxy = global::MatchService;

namespace DominoAllFives.Server.Tests
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode =
        ConcurrencyMode.Multiple)]
    internal sealed class MatchProxyCallback : Proxy.IMatchServiceCallback
    {
        public ConcurrentQueue<Proxy.PlayerHand> Hands { get; } =
            new ConcurrentQueue<Proxy.PlayerHand>();

        public void OnMatchChanged(Proxy.MatchView match)
        {
        // This probe observes private delivery; public state is checked through responses.
        }

        public void OnPrivateHandChanged(Proxy.PlayerHand hand)
        {
            Hands.Enqueue(hand);
        }

        public void OnRoundFinished(Proxy.RoundResult result)
        {
        // The single opening move in this probe cannot finish a round.
        }

        public void OnMatchFinished(Proxy.MatchResult result)
        {
        // The single opening move in this probe cannot reach the target score.
        }
    }
}
