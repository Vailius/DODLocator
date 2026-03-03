using DODLocator.Interfaces;

namespace DODLocator.IdGen
{
    public class IdentifierIterator : IIdentifierGenerator
    {
        private int _id = 0;

        public virtual int Next() => _id++;
        public virtual void Return(int id) {}
    }
}