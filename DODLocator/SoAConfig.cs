using System;
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

        public SoAConfig(IMemoryGrowCurve curve, 
            IIdentifierGenerator idgen, 
            IMemoryAllocator alloc, 
            int startCapacity = 64)
        {
            Debug.Assert(startCapacity > 0, $"{nameof(startCapacity)} <= 0");
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));
            if (idgen is null)
                throw new ArgumentNullException(nameof(idgen));
            if (alloc is null)
                throw new ArgumentNullException(nameof(alloc));
            MemoryGrow = curve;
            IdGenerator = idgen;
            Allocator = alloc;
            StartCapacity = startCapacity > 0 ? startCapacity : 64;
        }

        public SoAConfig(IIdentifierGenerator idgen, IMemoryAllocator alloc, int startCapacity = 64)
            : this(new MemoryGrowCurveX2(), idgen, alloc, startCapacity) {}
        public SoAConfig(IMemoryGrowCurve curve, IMemoryAllocator alloc, int startCapacity = 64)
            : this(curve, new IdentifierIterator(), alloc, startCapacity) {}
        public SoAConfig(IMemoryGrowCurve curve, IIdentifierGenerator idgen, int startCapacity = 64)
            : this(curve, idgen, new DefaultAllocator(), startCapacity) {}
        
        public SoAConfig(IMemoryGrowCurve curve, int startCapacity = 64)
            : this(curve, new IdentifierIterator(), startCapacity) {}
        public SoAConfig(IIdentifierGenerator idgen, int startCapacity = 64)
            : this(idgen, new DefaultAllocator(), startCapacity) {}
        public SoAConfig(IMemoryAllocator alloc, int startCapacity = 64)
            : this(new IdentifierIterator(), alloc, startCapacity) {}
        
        public SoAConfig(int startCapacity = 64)
            : this (new DefaultAllocator(), startCapacity) {}
    }
}