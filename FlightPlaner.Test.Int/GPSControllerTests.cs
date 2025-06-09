using FlightPlaner.Test.sdk;

namespace FlightPlaner.Test.Int
{
    [TestFixture]
    public class GPSControllerTests
    {
        private CustomWebApplicationFactory factory=null!;

        private HttpClient client = null!;

        [SetUp]
        public void Setup()
        {
            factory = new CustomWebApplicationFactory();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }
    }
}
