using System;
using System.ServiceModel;
using MatchService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Callbacks
{
    [CallbackBehavior(
        UseSynchronizationContext = false,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal sealed class MatchCallbackHandler : IMatchServiceCallback
    {
        private readonly Action<MatchView> _onMatchChanged;
        private readonly Action<PlayerHand> _onPrivateHandChanged;

        public MatchCallbackHandler(
            Action<MatchView> onMatchChanged,
            Action<PlayerHand> onPrivateHandChanged)
        {
            _onMatchChanged = onMatchChanged
                ?? throw new ArgumentNullException(nameof(onMatchChanged));
            _onPrivateHandChanged = onPrivateHandChanged
                ?? throw new ArgumentNullException(nameof(onPrivateHandChanged));
        }

        public void OnMatchChanged(MatchView match)
        {
            _onMatchChanged(match);
        }

        public void OnPrivateHandChanged(PlayerHand hand)
        {
            _onPrivateHandChanged(hand);
        }

        public void OnRoundFinished(RoundResult result)
        {
        }

        public void OnMatchFinished(MatchResult result)
        {
        }
    }
}
