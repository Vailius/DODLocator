namespace DODLocator.Interfaces
{
    /// <summary>
    /// Allocator for unmanaged memory
    /// </summary>
    public unsafe interface IMemoryAllocator
    {
        /// <summary>
        /// Allocate new block of unmanaged memory
        /// </summary>
        /// <param name="bytes">size in bytes</param>
        /// <returns> pointer to allocated memory block</returns>
        void *Alloc(int bytes);
        /// <summary>
        /// Reallocate unmanaged memory
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="newSize"></param>
        /// <returns></returns>
        void *Realloc(void *mem, int newSize);
        /// <summary>
        /// Free unmanaged memory
        /// </summary>
        /// <param name="mem">pointer to memory</param>
        void Free(void *mem);
    }
}