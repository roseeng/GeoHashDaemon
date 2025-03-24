using GeoHashDaemon;

namespace GeoHashTest
{
    [TestClass]
    public sealed class TestPushover
    {
        [TestMethod]
        public void TestSend()
        {
            PushoverImpl.SendNotification("Test", "Test");
        }
    }
}
