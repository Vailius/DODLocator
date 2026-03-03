namespace DODLocator.Interfaces
{
    public unsafe interface IMemoryAllocator
    {
        void *Alloc(int bytes);
        void *Realloc(void *mem, int newSize);
        void Free(void *mem);
    }
}