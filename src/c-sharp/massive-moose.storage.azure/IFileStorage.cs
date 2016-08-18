namespace massive_moose.storage.azure
{
    public interface IFileStorage
    {
        bool Exists(string filePath);
        void Delete(string filePath);
        void Store(string filePath, byte[] data, bool overwrite = false);
        byte[] Get(string filePath);
    }
}