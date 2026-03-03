using DODLocator.Interfaces;

namespace DODLocator.IdGen
{
    /// <summary>
    /// Simple iterator
    /// </summary>
    public class IdentifierIterator : IIdentifierGenerator
    {
        private int _id = 0;

        /// <summary>
        /// Iterate to new ID
        /// </summary>
        /// <returns>New ID</returns>
        public virtual int Next() => _id++;
        /// <summary>
        /// Return ID to Iterator
        /// </summary>
        /// <remarks>
        /// Iterator ignore this method
        /// </remarks>
        /// <param name="id">Unused Identifier</param>
        public virtual void Return(int id) {}
    }
}