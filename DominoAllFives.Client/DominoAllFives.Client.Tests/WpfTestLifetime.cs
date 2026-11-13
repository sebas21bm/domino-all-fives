using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [SetUpFixture]
    public sealed class WpfTestLifetime
    {
        [OneTimeTearDown]
        public void StopDispatcher()
        {
            WpfTestHost.Stop();
        }
    }
}
