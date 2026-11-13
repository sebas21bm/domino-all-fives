using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;

namespace DominoAllFives.Server.Infrastructure
{
    public static class GameRuntime
    {
        public static MatchEventPublisher Publisher { get; } = new MatchEventPublisher();
        public static MatchController Controller { get; } = new MatchController(
            new MatchManager(), Publisher.Enqueue);
    }
}
