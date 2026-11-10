using System;
using System.ServiceModel;

namespace DominoAllFives.Server
{
    internal static class Program
    {
        private static int Main()
        {
            using (ServiceHost dominoServiceHost = new ServiceHost(typeof(DominoService)))
            using (ServiceHost lobbyServiceHost = new ServiceHost(typeof(LobbyService)))
            {
                dominoServiceHost.Open();
                lobbyServiceHost.Open();

                Console.WriteLine("Domino All Fives server is running.");

                Console.WriteLine("Press Enter to stop the server.");

                Console.ReadLine();
            }

            return 0;
        }
    }
}
