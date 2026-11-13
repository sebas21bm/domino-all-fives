using System;
using System.ServiceModel;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server
{
    internal static class Program
    {
        private static int Main()
        {
            ILogger<DominoService> logger = new ConsoleLogger<DominoService>();
            using (ServiceHost dominoServiceHost = new ServiceHost(typeof(DominoService)))
            {
                using (ServiceHost lobbyServiceHost = new ServiceHost(typeof(LobbyService)))
                {
                    using (ServiceHost matchServiceHost = new ServiceHost(typeof(MatchService)))
                    {
                        dominoServiceHost.Open();
                        lobbyServiceHost.Open();
                        matchServiceHost.Open();
                        logger.LogInformation("Domino All Fives server is running.");
                        Console.WriteLine("Press Enter to stop the server.");
                        Console.ReadLine();
                    }
                }
            }

            return 0;
        }
    }
}
