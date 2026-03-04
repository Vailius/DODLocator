using System.Diagnostics;
using System.Runtime.InteropServices;
using DODLocator.Tests.Mocks;

namespace DODLocator.Tests
{
    public static class MocksTests
    {
        public static void TestMemoryGrow()
        {
            MockMemoryCurve curve = new();

            int sz = 64, newsz = 96;
            int r = curve.Grow(sz, newsz);
            Debug.Assert(r == sz * 2, "Curve calc error");
            Debug.Assert(curve.Grow(sz, newsz * 2) == sz * 4, "Curve calc error");
        }
        public static void TestIdGenerator()
        {
            MockIdGenerator gen = new();
            HashSet<int> ids = new();

            for (int i = 0; i < 1024; i++)
            {
                Debug.Assert(ids.Add(gen.Next()), "Repeated ID");
            }

            foreach (var id in ids)
            {
                gen.Return(id);
            }

            try
            {
                gen.Return(ids.First());
                throw new Exception("Double return ID");
            }
            catch (InvalidOperationException) {}

            ids.Clear();
        }
        public static unsafe void TestAllocator()
        {
            var alloc = new MockAllocator();
            
            void* mem = alloc.Alloc(4);
            alloc.Free(mem);
            Debug.Assert(alloc.AllocatedBlocks.Count == 0);
            
            void* ptr1 = alloc.Alloc(4);
            void* ptr2 = alloc.Realloc(ptr1, 8);
            Debug.Assert(!alloc.AllocatedBlocks.Contains((nint)ptr1));
            Debug.Assert(alloc.AllocatedBlocks.Contains((nint)ptr2));
            Debug.Assert(alloc.AllocatedBlocks.Count == 1);
            alloc.Free(ptr2);
            
            void* testPtr = alloc.Alloc(4);
            alloc.Free(testPtr);
            try
            {
                alloc.Free(testPtr);
                Debug.Assert(false, "Double free should throw");
            }
            catch (InvalidOperationException) {}
            
            var ptrs = new List<nint>();
            for (int i = 0; i < 100; i++)
                ptrs.Add((nint)alloc.Alloc(4));
            
            Debug.Assert(alloc.AllocatedBlocks.Count == 100);
            
            foreach (var ptr in ptrs)
                alloc.Free((void*)ptr);
            
            Debug.Assert(alloc.AllocatedBlocks.Count == 0);
            
            void* memCheck = alloc.Alloc(4);
            Marshal.WriteByte((nint)memCheck, 0, 42);
            alloc.Free(memCheck);
            
            try
            {
                Marshal.ReadByte((nint)memCheck);
                Debug.Assert(false, "Memory was not actually freed");
            }
            catch (AccessViolationException) {}
        }
    }
}