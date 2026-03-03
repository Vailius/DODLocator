using DODLocator.Interfaces;

namespace DODLocator.Curves
{
    public class MemoryGrowCurveX2 : IMemoryGrowCurve
    {
        public int Grow(int size, int targetSize)
        {
            int r = size * 2;
            while (r < targetSize)
                r *= 2;
            return r;
        }
    }
}