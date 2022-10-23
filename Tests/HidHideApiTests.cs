using Nefarius.Drivers.HidHide;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestIsActive()
        {
            var service = new HidHideControlService();

            Assert.True(service.IsActive);
        }

        [Test]
        public void TestIsAppListInverted()
        {
            var service = new HidHideControlService();

            Assert.True(service.IsAppListInverted);
        }
    }
}