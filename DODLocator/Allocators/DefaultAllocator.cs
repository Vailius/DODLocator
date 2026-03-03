using System;
using System.Runtime.InteropServices;
using DODLocator.Interfaces;

namespace DODLocator
{
    public class DefaultAllocator : IMemoryAllocator
    {
        public unsafe void* Alloc(int bytes)
        {
            return (void *) Marshal.AllocHGlobal(bytes);
        }

        public unsafe void Free(void* mem)
        {
            Marshal.FreeHGlobal((IntPtr) mem);
        }

        public unsafe void* Realloc(void* mem, int newSize)
        {
            return (void *) Marshal.ReAllocHGlobal((IntPtr) mem, (IntPtr) newSize);
        }
    }
}