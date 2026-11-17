using System.Collections.Concurrent;
using System.ServiceModel;
using Proxy = global::MatchService;

namespace DominoAllFives.Server.AdditionalTest
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode =
        ConcurrencyMode.Multiple)]
    internal sealed class MatchProxyCallback : Proxy.IMatchServiceCallback
    {
        public ConcurrentQueue<Proxy.PlayerHand> Hands { get; } =
            new ConcurrentQueue<Proxy.PlayerHand>();
        public ConcurrentQueue<Proxy.MatchView> Views { get; } =
            new ConcurrentQueue<Proxy.MatchView>();
        public ConcurrentQueue<Proxy.MatchResult> Results { get; } =
            new ConcurrentQueue<Proxy.MatchResult>();

        public void OnMatchChanged(Proxy.MatchView match)
        {
            Views.Enqueue(match);
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
            Results.Enqueue(result);
        }
    }
}
