using System;

namespace massive_moose.services.caching
{
    public class CacheEntry<T> : ICacheEntry where T : new()
    {
        public T Item { get; private set; }
        public DateTimeOffset DateTimeOffset { get; set; }

        public CacheEntry()
        { }

        public CacheEntry(T item, int itemLifetimeInSeconds=30)
        {
            Item = item;
            DateTimeOffset = DateTimeOffset.Now.AddSeconds(itemLifetimeInSeconds);
        }
    }
}