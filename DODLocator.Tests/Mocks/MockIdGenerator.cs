using System.Diagnostics;
using DODLocator.Interfaces;

namespace DODLocator.Tests.Mocks
{
    public class MockIdGenerator : IIdentifierGenerator
    {
        private readonly HashSet<int> _activeIds = new();
        private readonly Queue<int> _freed = new();
        private int _last = 0;

        public IReadOnlyCollection<int> Active => _activeIds;
        public IReadOnlyCollection<int> Freed => _freed;
        public int LastId => _last;

        public int Next()
        {
            int id;
            if (!_freed.TryDequeue(out id))
                id = _last++;
            _activeIds.Add(id);
            return id;
        }

        public void Return(int id)
        {
            if (!_activeIds.Remove(id))
                throw new InvalidOperationException("Unexpected returned ID");
            _freed.Enqueue(id);
        }
    }
}