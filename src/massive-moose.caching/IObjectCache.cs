namespace massive_moose.caching
{
    public interface IObjectCache<T> where T : new()
    {
        void Set(string key, T item);
        T Get(string key);
    }
}