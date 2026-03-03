namespace DODLocator.Interfaces
{
    /// <summary>
    /// Generator for ID
    /// </summary>
    public interface IIdentifierGenerator
    {
        /// <summary>
        /// Generate ID
        /// </summary>
        /// <returns>New Identifier</returns>
        int Next();
        /// <summary>
        /// Return unused ID
        /// </summary>
        /// <param name="id">Unused Identifier</param>
        void Return(int id);
    }
}