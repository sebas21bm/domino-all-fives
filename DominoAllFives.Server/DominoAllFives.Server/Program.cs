using System;
using System.ServiceModel;
using System.Threading;

namespace DominoAllFives.Server
{
    internal static class Program
    {
        private static int Main()
        {
            using (ServiceHost host = new ServiceHost(typeof(DominoService)))
            {
                host.Open();
                Console.WriteLine("Domino All Fives WCF server ready. Press Ctrl+C to stop.");
                Console.WriteLine("Service discovery: http://localhost:8734/Domino/");
                Console.WriteLine("WSDL: http://localhost:8734/Domino/?wsdl");
                Console.ReadLine();
                return 0;

            }
        }
    }
}
