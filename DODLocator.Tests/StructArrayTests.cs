using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using DODLocator.Tests.Mocks;

namespace DODLocator.Tests
{
    public static class StructArrayTests
    {
        private static readonly Random _random = new();
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
            Debug.Assert(alloc.FreeCalls == alloc.AllocsCalls, 
                "Free calls should match total allocations");
        }

        public static void TestData()
        {
            const int cnt = 64, instances_count = 1024;
            SoAConfig cfg = TestHepers.CreateConfig(cnt);

            var soa = new StructArray<Transform> (cfg);

            int[] indices = new int[instances_count];

            soa.InstantiateRange(indices);

            void CheckSizeOfRawData(Span<byte> bytes, int fieldIndex, Type t)
            {
                int targetSize = ((t == typeof(Vector4)) ? Marshal.SizeOf<Vector4>() : Marshal.SizeOf<Vector3>()) * instances_count;
                Debug.Assert(
                    bytes.Length == targetSize,
                    $"Incorrect size of {t.Name}: {bytes.Length}/{targetSize}"
                );
            }

            soa.ProcessRawData(CheckSizeOfRawData);

            Vector3[] Positions = new Vector3[instances_count];
            Vector4[] Rotations = new Vector4[instances_count];
            Vector3[] Scales = new Vector3[instances_count];

            void WritePositions(Span<Vector3> vectors)
            {
                for (int i = 0; i < vectors.Length; i++)
                {
                    Positions[i] = vectors[i] = new(
                        _random.NextSingle(),
                        _random.NextSingle(),
                        _random.NextSingle()
                    );
                }
            }

            void WriteScales(Span<Vector3> vectors)
            {
                for (int i = 0; i < vectors.Length; i++)
                {
                    Scales[i] = vectors[i] = new(
                        _random.NextSingle(),
                        _random.NextSingle(),
                        _random.NextSingle()
                    );
                }
            }

            void WriteRotations(Span<Vector4> vectors)
            {
                for (int i = 0; i < vectors.Length; i++)
                {
                    Rotations[i] = vectors[i] = new(
                        _random.NextSingle(),
                        _random.NextSingle(),
                        _random.NextSingle(),
                        _random.NextSingle()
                    );
                }
            }

            Transform.StartProcessPositions(soa, WritePositions);
            Transform.StartProcessRotations(soa, WriteRotations);
            Transform.StartProcessScales(soa, WriteScales);

            Transform.StartProcessPositions(soa, (x) =>
            {
                for(int i = 0; i < x.Length; i++)
                    Debug.Assert(Positions[i] == x[i]);
            });
            Transform.StartProcessRotations(soa, (x) =>
            {
                for(int i = 0; i < x.Length; i++)
                    Debug.Assert(Rotations[i] == x[i]);
            });
            Transform.StartProcessScales(soa, (x) =>
            {
                for(int i = 0; i < x.Length; i++)
                    Debug.Assert(Scales[i] == x[i]);
            });
        }
    }
}