namespace DODLocator
{
    public interface IIdentifierGenerator
    {
        int Next();
        void Return(int id);
    }
}