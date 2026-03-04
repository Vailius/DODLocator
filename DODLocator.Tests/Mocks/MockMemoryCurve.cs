using DODLocator.Interfaces;

namespace DODLocator.Tests
{
    public class MockMemoryCurve : IMemoryGrowCurve
    {
        private readonly List<(int, int)> _history = new();

        public IReadOnlyCollection<(int Size, int TargetSize)> History => _history;
        public int Grow(int size, int targetSize)
        {
            _history.Add((size, targetSize));
            while (size < targetSize)
                size *= 2;
            return size;
        }
    }
}