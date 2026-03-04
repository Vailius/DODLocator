using System.Diagnostics;
using System.Runtime.InteropServices;
using DODLocator.Interfaces;

namespace DODLocator.Tests.Mocks
{
    public class MockAllocator : IMemoryAllocator
    {
        public int AllocsCalls { get; private set; }
        public int ReallocsCalls { get; private set; }
        public int FreeCalls { get; private set; }

        private readonly HashSet<nint> _allocated = [];
        public IReadOnlyCollection<nint> AllocatedBlocks => _allocated;
        public int CurrentBlocksAllocatedCount => _allocated.Count;

        public record AllocationInfo(int Bytes, nint Ptr);
        public AllocationInfo LastAlloc { get; private set; } = new(0,0);
        public AllocationInfo LastRealloc { get; private set; } = new(0, 0);
        public unsafe void* Alloc(int bytes)
        {
            AllocsCalls++;
            void *ptr = (void *)Marshal.AllocHGlobal(bytes);
            LastAlloc = new(bytes, (nint) ptr);
            _allocated.Add((nint)ptr);
            return ptr;
        }

        public unsafe void Free(void* mem)
        {
            FreeCalls++;
            if (_allocated.Remove((nint) mem))
            {
                Marshal.FreeHGlobal((nint)mem);
            }
            else throw new InvalidOperationException("Unexpected pointer");
        }

        public unsafe void* Realloc(void* mem, int newSize)
        {
            ReallocsCalls++;
            if (_allocated.Remove((nint)mem))
            {
                void *r = (void *) Marshal.ReAllocHGlobal((nint)mem, newSize);
                LastRealloc = new(newSize, (nint) r);
                _allocated.Add((nint)r);
                return r;
            }
            throw new InvalidOperationException("Unexpected pointer");
        }
    }
}