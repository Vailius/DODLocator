using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace DODLocator
{
    /// <summary>
    /// Class makes a Structure of Arrays from <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Target unmanaged structure</typeparam>
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
        /// Configuration of SoA - Array
        /// </summary>
        private SoAConfig _config;
#endregion // Fields
        
#region  .ctor
        public StructArray([NotNullWhen(true)] SoAConfig cfg)
        {
            if (!StructFieldsAnalyzer<T>.IsValid)
                throw new InvalidOperationException("Unexpected struct fields type");
            
            _startCapacity = cfg.StartCapacity;
            _fieldsCount = StructFieldsAnalyzer<T>.Size.Count;
            
            void** tempData = null;
            void** fieldBuffers = stackalloc void*[_fieldsCount];
            int allocatedCount = 0;
            
            try
            {
                tempData = (void**)cfg.Allocator.Alloc(sizeof(void*) * _fieldsCount);
                if (tempData == null) throw new OutOfMemoryException();
                
                var names = StructFieldsAnalyzer<T>.Size.Keys;
                foreach (var name in names)
                {
                    int index = StructFieldsAnalyzer<T>.Identifier[name];
                    int size = StructFieldsAnalyzer<T>.Size[name];
                    Type fieldType = StructFieldsAnalyzer<T>.FieldType[name];
                    
                    void* ptr = cfg.Allocator.Alloc(size * _startCapacity);
                    if (ptr == null) throw new OutOfMemoryException();
                    
                    tempData[index] = ptr;
                    allocatedCount++;
                }
                
                _data = tempData;
                _size = new int[_fieldsCount];
                _type = new Type[_fieldsCount];
                _vaddress = new SparseSet(_startCapacity);
                _config = cfg;
                
                foreach (var name in names)
                {
                    int index = StructFieldsAnalyzer<T>.Identifier[name];
                    _size[index] = StructFieldsAnalyzer<T>.Size[name];
                    _type[index] = StructFieldsAnalyzer<T>.FieldType[name];
                }
            }
            catch
            {
                if (tempData != null)
                {
                    for (int i = 0; i < allocatedCount; i++)
                        if (tempData[i] != null) cfg.Allocator.Free(tempData[i]);
                    cfg.Allocator.Free(tempData);
                }
                throw;
            }
        }

#endregion // .ctor

#region Lifecycle

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/>
        /// </summary>
        /// <returns>Identifier of new instance</returns>
        public int Instantiate()
        {
            if (!_needDispose)
                throw new ObjectDisposedException(nameof(StructArray<T>));
            EnsureCapacity(_vaddress.Count + 1);
            return CreateInstance();
        }

        /// <summary>
        /// Create a new instance of <typeparamref name="T"/>
        /// </summary>
        /// <returns>Identifier of new instance</returns>
        private int CreateInstance()
        {
            int id = _config.IdGenerator.Next();
            bool isAdded = _vaddress.AddKey(id);
            Debug.Assert(isAdded);
            ClearData(id);
            return id;
        }

        /// <summary>
        /// Create a new instances of <typeparamref name="T"/>
        /// </summary>
        /// <remarks>
        /// <paramref name="ids"/> is a buffer, all values in it will be
        /// overwritten with new identifiers of instantiated structs
        /// </remarks>
        /// <param name="ids">Output identifiers buffer</param>
        public void InstantiateRange(Span<int> ids)
        {
            if (!_needDispose)
                throw new ObjectDisposedException(nameof(StructArray<T>));
            EnsureCapacity(_vaddress.Count + ids.Length);
            for (int i =  0; i < ids.Length; i++)
                ids[i] = CreateInstance();
        }

        /// <summary>
        /// Move instance from <see cref="_vaddress.Count"/> to <paramref name="idx"/>
        /// </summary>
        /// <param name="idx"></param>
        private void MoveFromEnd(int idx)
        {
            for (int i = 0; i < _fieldsCount; i++)
            {
                byte *ptr = (byte *) *(_data + i);
                Span<byte> bytes = new Span<byte>(ptr + _vaddress.Count * _size[i], _size[i]);
                Span<byte> target = new Span<byte>(ptr + idx * _size[i], _size[i]);
                bytes.CopyTo(target);
                bytes.Fill(0);
            }
        }

        /// <summary>
        /// Destroy instance from array
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>true if the instance with the <paramref name="id"/> was contained in the array, otherwise false</returns>
        public bool Destroy(int id)
        {
            if (!_needDispose)
                throw new ObjectDisposedException(nameof(StructArray<T>));
            if (!_vaddress.HasKey(id))
                return false;
            int deleteTarget = _vaddress.GetDense(id);
            _vaddress.RemoveKey(id);
            _config.IdGenerator.Return(id);
            if (deleteTarget != _vaddress.Count)
                MoveFromEnd(deleteTarget);
            return true;
        }

        /// <summary>
        /// Dstroy indices in <paramref name="ids"/>, where was contained in the array
        /// </summary>
        /// <param name="ids">Indices</param>
        public void DestroyRange(Span<int> ids)
        {
            if (!_needDispose)
                throw new ObjectDisposedException(nameof(StructArray<T>));
            for (int i = 0; i < ids.Length; i++)
            {
                if (_vaddress.HasKey(ids[i]))
                {
                    int idx = _vaddress.GetDense(ids[i]);
                    _vaddress.RemoveKey(ids[i]);
                    _config.IdGenerator.Return(ids[i]); 
                    if (idx != _vaddress.Count)
                        MoveFromEnd(idx);
                }
            }
        }
#endregion // Lifecycle

        
#region DataProcessor
        public delegate void RawDataHandler(Span<byte> data, int dataIdentifier, Type dataType);
        public delegate void DataHandler<T1>(Span<T1> data);

        /// <summary>
        /// Processing of SoA fields
        /// </summary>
        /// <param name="handler">Method for processing</param>
        public void ProcessRawData(RawDataHandler handler)
        {
            if (!_needDispose)
                throw new ObjectDisposedException(nameof(StructArray<T>));
            for (int i = 0; i < _fieldsCount; i++)
            {
                byte *ptr = (byte *) *(_data + i);
                handler(new Span<byte> (ptr, _vaddress.Count * _size[i]), i, _type[i]);
            }
        }

        
        /// <summary>
        /// Processing data with type <typeparamref name="T1"/> and index of <paramref name="dataIdentifier"/>
        /// </summary>
        /// <typeparam name="T1">Type of field</typeparam>
        /// <param name="handler">Method for processing</param>
        /// <param name="dataIdentifier">Identifier of field</param>
        public void ProcessData<T1>(DataHandler<T1> handler, int dataIdentifier) where T1 : unmanaged
        {
            if (!_needDispose) throw new ObjectDisposedException(nameof(StructArray<T>));
            if (handler is null) throw new ArgumentNullException(nameof(handler));
            
            ValidateDataIdentifier(dataIdentifier);
            ValidateType<T1>(dataIdentifier);
            
            T1* ptr = (T1*)*(_data + dataIdentifier);
            handler(new Span<T1>(ptr, _vaddress.Count));
        }
#endregion // DataProcessor
#region Utils

        /// <summary>
        /// Clear fields of instance
        /// </summary>
        /// <param name="id"> Instance identifier </param>
        private void ClearData(int id)
        {
            int dense_id = _vaddress.GetDense(id);
            for (int i = 0; i < _fieldsCount; i++)
            {
                byte *data = (byte *) *(_data + i);
                int size = StructFieldsAnalyzer<T>.Size[
                    StructFieldsAnalyzer<T>.NameOfIdentifier[i]
                ];
                int offset = size * dense_id;

                for (int j = 0; j < size; j++)
                    data[j + offset] = 0;
            }
        }

        /// <summary>
        /// Validate data id
        /// </summary>
        /// <param name="id">field identifier</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// On argument out of range [0, <see cref="DODLocator.StructArray{T}._fieldsCount"/>]
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If sparse set (<see cref="DODLocator.StructArray{T}._vaddress"/> no has 
        /// <paramref name="id"/>
        /// </exception>
        private void ValidateDataIdentifier(int id)
        {
            if (id < 0 || id >= _fieldsCount)
                throw new ArgumentOutOfRangeException(nameof(id), $"Field identifier {id} is out of range [0, {_fieldsCount})");
            if (_vaddress.Count == 0)
                throw new InvalidOperationException($"No active instance with identifier {id}");
        }

        /// <summary>
        /// Check type of data of dataIdentifier
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="dataIdentifier"></param>
        /// <exception cref="InvalidCastException"></exception>
        private void ValidateType<T1>(int dataIdentifier) where T1 : unmanaged
        {
            if (typeof(T1) != _type[dataIdentifier])
                throw new InvalidCastException($"Cannot cast {typeof(T1).FullName} to {_type[dataIdentifier].FullName}");
        }

        /// <summary>
        /// Get id from index of array
        /// </summary>
        /// <param name="index">index in array</param>
        /// <returns>Identifier of index, if incorrect index, then returns -1</returns>
        public int GetIdOfIndex(int index)
        {
            if (!_needDispose)
                throw new ObjectDisposedException(nameof(StructArray<T>));
            Debug.Assert(index >= 0 && index < _vaddress.Count);
            if (index < 0 || index >= _vaddress.Count)
                return -1;
            return _vaddress.Dense[index];
        }
        /// <summary>
        /// Ensure growed capacity for SoA
        /// </summary>
        /// <param name="targetCapacity">Target capacity</param>
        private void EnsureCapacity(int targetCapacity)
        {
            if (targetCapacity > _vaddress.Size)
            {
                int newcap = _config.MemoryGrow.Grow(_vaddress.Size, targetCapacity);

                for (int i = 0; i < _fieldsCount; i++)
                {
                    void *mem = *(_data + i);
                    void *newMem = _config.Allocator.Realloc( mem, newcap * _size[i]);
                    ThrowIfOOM(newMem);
                    *(_data + i) = newMem;
                }
                _vaddress.Resize(newcap);
            }
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
                            _config.Allocator.Free(ptr);
                        *(_data + i) = (void *)0;
                    }

                    _config.Allocator.Free( _data);

                    _data = (void **)0;
                }
                _needDispose = false;
                GC.SuppressFinalize(this);
            }
        }

        ~StructArray() => Dispose();
    }
}