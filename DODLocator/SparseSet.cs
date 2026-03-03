using System;
using Debug = System.Diagnostics.Debug;
using Conditional = System.Diagnostics.ConditionalAttribute;
using System.Collections.Generic;

namespace DODLocator
{
    /// <summary>
    /// A sparse set represented as two arrays of type int
    /// </summary>
    internal class SparseSet
    {
        public int Size { get; private set; }
        public int Count { get; private set; } = 0;

        private int[] _sparse, _dense;

        protected int[] sparse => _sparse;
        
        public SparseSet(int size)
        {
            Size = size;
            _sparse = new int[size];
            _dense = new int[size];
            Array.Fill(_sparse, -1);
        }

        [Conditional("Debug")]
        private void DebugOutOfSet(int key) =>
            Debug.Assert(key >= 0 && key < Size,
                "The key of a sparse set must be part of that set");

        /// <summary>
        /// Checking if an key is present in a set
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>true if key present in a set, else false</returns>
        public bool HasKey(int key)
        {
            Debug.Assert(key >= 0 && key < Size);
            return _dense[_sparse[key]] == key;
        }

        /// <summary>
        /// Add key to SparseSet
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>true if key added, else returns false</returns>
        public bool AddKey(int key)
        {
            DebugOutOfSet(key);
            if (HasKey(key) || key >= Size)
                return false;
            _sparse[key] = Count++;
            _dense[_sparse[key]] = key;
            return true;
        }

        /// <summary>
        /// Remove key from SparseSet
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>true if key removed, else returns false</returns>
        public bool RemoveKey(int key)
        {
            DebugOutOfSet(key);
            if (HasKey(key))
            {
                int toRemove = _sparse[key];
                int last = Count - 1;

                int lastDenseIdx = _dense[last];

                _sparse[lastDenseIdx] = toRemove;
                _sparse[key] = -1;
                _dense[toRemove] = lastDenseIdx;
                Count = last;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns index in dense array
        /// </summary>
        /// <param name="key">Ket</param>
        /// <returns>Index in dense array, if <paramref name="key"/> not present in set, returns -1 </returns>
        public int GetDense(int key)
        {
            if (HasKey(key))
                return _sparse[key];
            return -1;
        }

        /// <summary>
        /// Clear the set
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                _sparse[_dense[i]] = -1;
                _dense[i] = -1;
            }
            Count = 0;
        }

        /// <summary>
        /// Resize set
        /// </summary>
        /// <param name="newSize">New size of set</param>
        public virtual void Resize(int newSize)
        {
            Array.Resize(ref _sparse, newSize);
            Array.Resize(ref _dense, newSize);
        }
    }

    /// <summary>
    /// Sparse set modified by a universal type array
    /// </summary>
    /// <typeparam name="T">The type of additional set used as values</typeparam>
    internal class SparseSet<T> : SparseSet
    {
        private T[] _values;

        public Span<T> Values => new Span<T>(_values, 0, Count);

        public T this[int key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }
        public SparseSet(int size) : base(size)
        {
            _values = new T[size];
        }
        
        /// <summary>
        /// Adds a value to a set by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>true if value added on key, else returns false</returns>
        public bool AddValue(int key, T value)
        {
            Debug.Assert(HasKey(key), "Key is already contains in set");
            if (HasKey(key) || !AddKey(key))
                return false;
            _values[sparse[key]] = value;
            return true;
        }

        /// <summary>
        /// Set value of key in set
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>true if value setted, else returns false</returns>
        public bool SetValue(int key, T value)
        {
            if (HasKey(key))
            {
                _values[sparse[key]] = value;
                return true;
            }
            Debug.Assert(false, "Key is not contains in set");
            return false;
        }

        /// <summary>
        /// Get value of key in set
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>value of key in set</returns>
        /// <exception cref="KeyNotFoundException">If key not contains in set</exception>
        public T GetValue(int key)
        {
            if (HasKey(key))
            {
                return _values[sparse[key]];
            }
            throw new KeyNotFoundException("Key is not contains in set");
        }

        public override void Resize(int newSize)
        {
            base.Resize(newSize);
            Array.Resize(ref _values, newSize);
        }

        /// <summary>
        /// Clears the sparse set and resets all values to the specified clear value.
        /// </summary>
        /// <param name="clearValue">The value to set for all cleared entries.</param>
        /// <remarks>
        /// This method first clears all values by setting them to <paramref name="clearValue"/>,
        /// then calls the parameterless Clear() to reset the sparse and dense arrays.
        /// </remarks>
        public void Clear(T clearValue)
        {
            for (int i = 0; i < Count; i++)
                _values[i] = clearValue;
            Clear();
        }
    }
}