namespace DODLocator.Interfaces
{
    public interface IMemoryGrowCurve
    {
        int Grow(int size, int targetSize);
    }
}