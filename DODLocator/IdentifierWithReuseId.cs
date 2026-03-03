using System.Collections.Generic;

namespace DODLocator
{
    public class IdentifierWithReuseId : IdentifierIterator, IIdentifierGenerator
    {
        private readonly Queue<int> id = new Queue<int>();
        public override int Next()
        {
            if (id.TryDequeue(out int i))
                return i;
            return base.Next();
        }

        public override void Return(int i) => id.Enqueue(i);
    }
}