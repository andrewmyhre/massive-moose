namespace massive_moose.services
{
    public interface IFileStorage
    {
        bool Exists(string filePath);
        void Delete(string filePath);
        void Store(string filePath, byte[] data, bool overwrite = false);
        byte[] Get(string filePath);
        void EnsureDirectoryExists(string containerName);
    }
}