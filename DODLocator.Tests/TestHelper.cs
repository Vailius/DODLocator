using DODLocator.Tests.Mocks;

namespace DODLocator.Tests
{
    public static class TestHepers
    {
        public static SoAConfig CreateConfig(int capacity)
        {
            return new SoAConfig(new MockMemoryCurve(), new MockIdGenerator(), new MockAllocator(), capacity);
        }
    }
}