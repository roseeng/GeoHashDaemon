using GeoHashDaemon;

namespace GeoHashTest
{
    [TestClass]
    public sealed class TestPushover
    {
        [TestMethod]
        public void TestSend()
        {
            PushoverImpl.apiToken = "";
            PushoverImpl.userToken = "";

            PushoverImpl.SendNotification("Test", "Test");
        }
    }
}
