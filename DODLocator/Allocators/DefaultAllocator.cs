using System;
using System.Runtime.InteropServices;
using DODLocator.Interfaces;

namespace DODLocator
{
    /// <summary>
    /// Default allocator use Marshal
    /// </summary>
    public class DefaultAllocator : IMemoryAllocator
    {
        /// <summary>
        /// Allocate new block of unmanaged memory
        /// </summary>
        /// <param name="bytes">size block in bytes</param>
        /// <returns>pointer to allocated memory, if out of memory returns <see cref="IntPtr.Zero"/></returns>
        public unsafe void* Alloc(int bytes)
        {
            return (void *) Marshal.AllocHGlobal(bytes);
        }

        /// <summary>
        /// Free memory previously allocated unmanaged memory
        /// </summary>
        /// <param name="mem">pointer to unmanaged memory</param>
        public unsafe void Free(void* mem)
        {
            Marshal.FreeHGlobal((IntPtr) mem);
        }

        /// <summary>
        /// Resize block of unmanaged memory previously allocated with <see cref="DefaultAllocator.Alloc(int)"/>
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="newSize"></param>
        /// <returns>new ptr of resized block</returns>
        public unsafe void* Realloc(void* mem, int newSize)
        {
            return (void *) Marshal.ReAllocHGlobal((IntPtr) mem, (IntPtr) newSize);
        }
    }
}