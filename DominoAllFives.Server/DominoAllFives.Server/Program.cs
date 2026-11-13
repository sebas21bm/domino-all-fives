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
                    dominoServiceHost.Open();
                    lobbyServiceHost.Open();
                    logger.LogInformation("Domino All Fives server is running.");
                    Console.WriteLine("Press Enter to stop the server.");
                    Console.ReadLine();
                }
            }

            return 0;
        }
    }
}
