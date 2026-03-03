using DODLocator.Interfaces;

namespace DODLocator.Curves
{
    /// <summary>
    /// Memory size multiplier on 2
    /// </summary>
    public class MemoryGrowCurveX2 : IMemoryGrowCurve
    {
        /// <summary>
        /// Multiple <paramref name="size"/> on 2 while <paramref name="size"/> less than <paramref name="targetSize"/>
        /// </summary>
        /// <param name="size">Current size</param>
        /// <param name="targetSize">Need size</param>
        /// <returns>New memory size</returns>
        public int Grow(int size, int targetSize)
        {
            int r = size * 2;
            while (r < targetSize)
                r *= 2;
            return r;
        }
    }
}