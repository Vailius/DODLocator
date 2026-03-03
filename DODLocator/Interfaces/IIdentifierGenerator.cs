namespace DODLocator.Interfaces
{
    public interface IIdentifierGenerator
    {
        int Next();
        void Return(int id);
    }
}