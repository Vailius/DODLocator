using System.Collections.Generic;
using DODLocator.Interfaces;

namespace DODLocator.IdGen
{
    /// <summary>
    /// Identifier iterator with free queue
    /// </summary>
    public class IdentifierWithReuseId : IdentifierIterator, IIdentifierGenerator
    {
        private readonly Queue<int> _freeIndices = new Queue<int>();
        /// <summary>
        /// Generate ID
        /// </summary>
        /// <returns>If contains free index return free index, else iterate and return new index</returns>
        public override int Next()
        {
            if (_freeIndices.TryDequeue(out int i))
                return i;
            return base.Next();
        }

        /// <summary>
        /// Return ID to free queue
        /// </summary>
        /// <param name="i">Identifier</param>
        public override void Return(int i) => _freeIndices.Enqueue(i);
    }
}