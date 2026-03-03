using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using DODLocator.Curves;
using DODLocator.IdGen;
using DODLocator.Interfaces;

namespace DODLocator
{
    public unsafe sealed class StructArray<T> : IDisposable
        where T : unmanaged
    {
#region Fields
        /// <summary>
        /// Virtual address for instances
        /// </summary>
        private readonly SparseSet _vaddress;
        /// <summary>
        /// Disposing pattern
        /// </summary>
        private bool _needDispose = true;
        /// <summary>
        /// Start capacity
        /// </summary>
        private int _startCapacity;
        /// <summary>
        /// Count of struct fields
        /// </summary>
        private int _fieldsCount;
        /// <summary>
        /// Size of concrete field of index
        /// </summary>
        private int[] _size;
        /// <summary>
        /// Field type of index
        /// </summary>
        private Type[] _type;
        /// <summary>
        /// Structure of arrays data storage
        /// </summary>
        private void **_data = (void **)0;

        /// <summary>
        /// Curve of grow memory
        /// </summary>
        private IMemoryGrowCurve _curve;
        /// <summary>
        /// Id generator
        /// </summary>
        private IIdentifierGenerator _idGen;
#endregion // Fields
        
#region  .ctor
        public StructArray([NotNullWhen(true)]IMemoryGrowCurve curve, [NotNullWhen(true)]IIdentifierGenerator gen, int startCapacity = 64)
        {
            if (!StructFieldsAnalyzer<T>.IsValid)
                throw new InvalidOperationException("Unexpected struct fields type");
            _startCapacity = startCapacity;
            _fieldsCount = StructFieldsAnalyzer<T>.Size.Count;
            _data = (void **) Marshal.AllocHGlobal(
                sizeof(void *) * _fieldsCount
            );

            if (_data == (void **)0)
                throw new OutOfMemoryException();

            _size = new int[_fieldsCount];
            _type = new Type[_fieldsCount];
            _vaddress = new SparseSet(_startCapacity);

            var names = StructFieldsAnalyzer<T>.Size.Keys;

            foreach (var name in names)
            {
                int index = StructFieldsAnalyzer<T>.Identifier[name];
                int size = StructFieldsAnalyzer<T>.Size[name];
                Type fieldType = StructFieldsAnalyzer<T>.FieldType[name];

                _size[index] = size;
                _type[index] = fieldType;
                void *ptr = (void *) Marshal.AllocHGlobal(size * _startCapacity);
                ThrowIfOOM(ptr);
                *(_data + index) = ptr;
            }
            _curve = curve;
            _idGen = gen;
        }

        public StructArray(int startCapacity = 64) : this(new MemoryGrowCurveX2(), new IdentifierIterator(), startCapacity) {}
        public StructArray(IIdentifierGenerator idgen, int startCapacity = 64) : this(new MemoryGrowCurveX2(), idgen, startCapacity) {}
        public StructArray(IMemoryGrowCurve curve, int startCapacity = 64) : this(curve, new IdentifierIterator(), startCapacity) {}

#endregion // .ctor

#region Lifecycle

        public int Instantiate()
        {
            EnsureCapacity(_vaddress.Count + 1);
            return CreateInstance();
        }

        private int CreateInstance()
        {
            int id = _idGen.Next();
            _vaddress.AddKey(id);
            return id;
        }

        public void InstantiateRange(Span<int> ids)
        {
            EnsureCapacity(_vaddress.Count+ ids.Length);
            for (int i =  0; i < ids.Length; i++)
                ids[i] = CreateInstance();
        }

        private void MoveFromEnd(int idx)
        {
            int max = _size.Max();
            for (int i = 0; i < _fieldsCount; i++)
            {
                byte *ptr = (byte *) *(_data + i);
                Span<byte> bytes = new Span<byte>(ptr + _vaddress.Count * _size[i], _size[i]);
                Span<byte> target = new Span<byte>(ptr + idx * _size[i], _size[i]);
                bytes.CopyTo(target);
                bytes.Fill(0);
            }
        }

        public bool Destroy(int id)
        {
            if (!_vaddress.HasKey(id))
                return false;
            int deleteTarget = _vaddress.GetDense(id);
            _vaddress.RemoveKey(id);
            _idGen.Return(id);
            if (deleteTarget != _vaddress.Count)
                MoveFromEnd(deleteTarget);
            return true;
        }

        public void DestroyRange(Span<int> ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                if (_vaddress.HasKey(ids[i]))
                {
                    int idx = _vaddress.GetDense(ids[i]);
                    _vaddress.RemoveKey(ids[i]);
                    _idGen.Return(idx);
                    if (idx != _vaddress.Count)
                        MoveFromEnd(idx);
                }
            }
        }
#endregion // Lifecycle

        
#region DataProcessor
        public delegate void RawDataHandler(Span<byte> data, int dataIdentifier, Type dataType);
        public delegate void DataHandler<T1>(Span<T1> data);

        public void ProcessRawData(RawDataHandler handler)
        {
            for (int i = 0; i < _fieldsCount; i++)
            {
                byte *ptr = (byte *) *(_data + i);
                handler(new Span<byte> (ptr, _vaddress.Count * _size[i]), i, _type[i]);
            }
        }

        

        public void ProcessData<T1>(DataHandler<T1> handler, int dataIdentifier) where T1 : unmanaged
        {
            CheckIdentifier(dataIdentifier);
            CheckCast<T1>(_type[dataIdentifier]);

            T1 *ptr = (T1 *) *(_data + dataIdentifier);

            handler(new Span<T1>(ptr, _vaddress.Count));
        }
#endregion // DataProcessor
#region Utils
        private void EnsureCapacity(int targetCapacity)
        {
            if (targetCapacity >= _vaddress.Size)
            {
                int newcap = _curve.Grow(_vaddress.Size, targetCapacity);
                _vaddress.Resize(newcap);


                for (int i = 0; i < _fieldsCount; i++)
                {
                    void *mem = *(_data + i);
                    void *newMem = (void *) Marshal.ReAllocHGlobal( (IntPtr) mem, (IntPtr) (newcap * _size[i]));
                    ThrowIfOOM(newMem);
                    *(_data + i) = newMem;
                }
            }
        }
        [Conditional("Debug")]
        private void CheckCast<T1>(Type type)
        {
            if (typeof(T1) != type)
                throw new InvalidCastException($"Try cast {typeof(T1).FullName} => {type.FullName}");
        }

        [Conditional("Debug")]
        private void CheckIdentifier(int id)
        {
            if (id <= 0 || id >= _vaddress.Count)
                throw new InvalidOperationException($"Is no identifier of struct {typeof(T).FullName}");
        }
        /// <summary>
        /// Throw if out of memory after allocate memory
        /// </summary>
        /// <param name="ptr"> Pointer to memory </param>
        /// <exception cref="OutOfMemoryException"> When <paramref name="ptr"/> is nullptr </exception>
        private void ThrowIfOOM(void *ptr)
        {
            if (ptr == (void *)0)
                throw new OutOfMemoryException();
        }
#endregion // Utils

        public void Dispose()
        {
            if (_needDispose)
            {
                if (_data != (void **)0)
                {
                    for (int i = 0; i < _fieldsCount; i++)
                    {
                        void *ptr = *(_data + i);
                        if (ptr != (void *)0)
                            Marshal.FreeHGlobal((IntPtr)ptr);
                        *(_data + i) = (void *)0;
                    }

                    _data = (void **)0;
                }
                _needDispose = false;
            }
        }

        ~StructArray() => Dispose();
    }
}