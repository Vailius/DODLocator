using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DODLocator.Curves;
using DODLocator.IdGen;
using DODLocator.Interfaces;

namespace DODLocator
{
    /// <summary>
    /// Configuration Structure of Array
    /// </summary>
    public class SoAConfig
    {
        /// <summary>
        /// Memory grow strategy
        /// </summary>
        public readonly IMemoryGrowCurve MemoryGrow;
        /// <summary>
        /// Identificator create strategy
        /// </summary>
        public readonly IIdentifierGenerator IdGenerator;
        /// <summary>
        /// Memory allocation strategy
        /// </summary>
        public readonly IMemoryAllocator Allocator;
        /// <summary>
        /// Initial capacity of fields
        /// </summary>
        public readonly int StartCapacity;

        public SoAConfig([NotNullWhen(true)]IMemoryGrowCurve curve, 
            [NotNullWhen(true)]IIdentifierGenerator idgen, 
            [NotNullWhen(true)]IMemoryAllocator alloc, 
            int startCapacity = 64)
        {
            Debug.Assert(curve != null, $"Null reference of {nameof(curve)}");
            Debug.Assert(idgen != null, $"Null reference of {nameof(idgen)}");
            Debug.Assert(alloc != null, $"Null reference of {nameof(alloc)}");
            Debug.Assert(startCapacity > 0, $"{nameof(startCapacity)} <= 0");
            MemoryGrow = curve;
            IdGenerator = idgen;
            Allocator = alloc;
            StartCapacity = startCapacity;
        }

        public SoAConfig([NotNullWhen(true)]IIdentifierGenerator idgen, [NotNullWhen(true)]IMemoryAllocator alloc, int startCapacity = 64)
            : this(new MemoryGrowCurveX2(), idgen, alloc, startCapacity) {}
        public SoAConfig([NotNullWhen(true)]IMemoryGrowCurve curve, [NotNullWhen(true)]IMemoryAllocator alloc, int startCapacity = 64)
            : this(curve, new IdentifierIterator(), alloc, startCapacity) {}
        public SoAConfig([NotNullWhen(true)]IMemoryGrowCurve curve, [NotNullWhen(true)]IIdentifierGenerator idgen, int startCapacity = 64)
            : this(curve, idgen, new DefaultAllocator(), startCapacity) {}
        
        public SoAConfig([NotNullWhen(true)]IMemoryGrowCurve curve, int startCapacity = 64)
            : this(curve, new IdentifierIterator(), startCapacity) {}
        public SoAConfig([NotNullWhen(true)]IIdentifierGenerator idgen, int startCapacity = 64)
            : this(idgen, new DefaultAllocator(), startCapacity) {}
        public SoAConfig([NotNullWhen(true)]IMemoryAllocator alloc, int startCapacity = 64)
            : this(new IdentifierIterator(), alloc, startCapacity) {}
        
        public SoAConfig(int startCapacity = 64)
            : this (new DefaultAllocator(), startCapacity) {}
    }
}