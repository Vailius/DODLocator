using System.Diagnostics;
using DODLocator.Tests.Mocks;

namespace DODLocator.Tests
{
    public static class StructAttayTests
    {
        public static void TestMemoryLeaks()
        {
            SoAConfig cfg = TestHepers.CreateConfig(2);
            MockAllocator alloc = (MockAllocator)cfg.Allocator;
            
            using (var soa = new StructArray<Transform>(cfg))
            {
                
                Debug.Assert(alloc.AllocsCalls == 4, "Should allocate _data + 3 fields");
                Debug.Assert(alloc.CurrentBlocksAllocatedCount == 4, "All 4 blocks should be alive");
                Debug.Assert(alloc.ReallocsCalls == 0, "No reallocs yet");
                
                soa.InstantiateRange([1, 2, 3]);
                
                Debug.Assert(alloc.ReallocsCalls == 3, "Should realloc each of 3 fields");
                Debug.Assert(alloc.CurrentBlocksAllocatedCount == 4, "Memory leak or double alloc!");
                
                Debug.Assert(alloc.AllocsCalls == 4, "No new allocations should happen");
            }
            
            Debug.Assert(alloc.CurrentBlocksAllocatedCount == 0, "Memory leak detected!");
            Debug.Assert(alloc.FreeCalls == alloc.AllocsCalls + alloc.ReallocsCalls, 
                "Free calls should match total allocations");
        }
    }
}