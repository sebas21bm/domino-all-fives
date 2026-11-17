using System;
using System.ServiceModel;
using MatchService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Callbacks
{
    /// <summary>
    /// Receives duplex match notifications independently from the channel owner.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal sealed class MatchCallbackHandler : IMatchServiceCallback
    {
        private readonly Action<MatchView> _onMatchChanged;
        private readonly Action<PlayerHand> _onPrivateHandChanged;
        private readonly Action<RoundResult> _onRoundFinished;
        private readonly Action<MatchResult> _onMatchFinished;

        public MatchCallbackHandler(
            Action<MatchView> onMatchChanged,
            Action<PlayerHand> onPrivateHandChanged,
            Action<RoundResult> onRoundFinished,
            Action<MatchResult> onMatchFinished)
        {
            _onMatchChanged = onMatchChanged
                ?? throw new ArgumentNullException(nameof(onMatchChanged));
            _onPrivateHandChanged = onPrivateHandChanged
                ?? throw new ArgumentNullException(nameof(onPrivateHandChanged));
            _onRoundFinished = onRoundFinished
                ?? throw new ArgumentNullException(nameof(onRoundFinished));
            _onMatchFinished = onMatchFinished
                ?? throw new ArgumentNullException(nameof(onMatchFinished));
        }

        /// <summary>
        /// Forwards the public match snapshot to the channel owner's subscribers.
        /// </summary>
        /// <param name="match">The authoritative public snapshot.</param>
        public void OnMatchChanged(MatchView match)
        {
            _onMatchChanged(match);
        }

        /// <summary>Forwards the private hand for the authenticated participant.</summary>
        /// <param name="hand">The participant's private hand and version.</param>
        public void OnPrivateHandChanged(PlayerHand hand)
        {
            _onPrivateHandChanged(hand);
        }

        /// <summary>Forwards the completed round outcome.</summary>
        /// <param name="result">The completed round result.</param>
        public void OnRoundFinished(RoundResult result)
        {
            _onRoundFinished(result);
        }

        /// <summary>Forwards the completed or cancelled match outcome.</summary>
        /// <param name="result">The final match result.</param>
        public void OnMatchFinished(MatchResult result)
        {
            _onMatchFinished(result);
        }
    }
}
