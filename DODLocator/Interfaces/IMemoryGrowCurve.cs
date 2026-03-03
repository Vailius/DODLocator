namespace DODLocator.Interfaces
{
    /// <summary>
    /// Interface for growing memory strategy
    /// </summary>
    public interface IMemoryGrowCurve
    {
        /// <summary>
        /// Grow memory size
        /// </summary>
        /// <param name="size">Current size</param>
        /// <param name="targetSize">Needable size</param>
        /// <returns>New size for memory</returns>
        int Grow(int size, int targetSize);
    }
}