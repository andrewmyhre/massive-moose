using log4net;
using System;
using System.Collections.Concurrent;
using System.Linq;
using massive_moose.caching;

namespace massive_moose.services.caching
{
    public class ObjectCache<T> : IObjectCache<T> where T : new()
    {
        private ILog _log = LogManager.GetLogger(typeof(ObjectCache<T>));
        private readonly int _itemLifetimeInSeconds;
        private ConcurrentDictionary<string, CacheEntry<T>> _objects = new ConcurrentDictionary<string, CacheEntry<T>>();

        public ObjectCache(int itemLifetimeInSeconds=30)
        {
            _itemLifetimeInSeconds = itemLifetimeInSeconds;
        }

        public void Set(string key, T item)
        {
            _objects[key] = new CacheEntry<T>(item, _itemLifetimeInSeconds);
            _log.DebugFormat("Added to cache: {0}", key);
        }

        public T Get(string key)
        {
            if (_objects.ContainsKey(key))
            {
                _log.DebugFormat("Cache hit: {0}", key);
                return _objects[key].Item;
            }

            _log.DebugFormat("Cache miss: {0}", key);
            return default(T);
        }

        private void PurgeStaleData()
        {
            var stale = _objects.Where(t => t.Value.DateTimeOffset < DateTimeOffset.Now).Select(t => t.Key).ToArray();
            for (int i = 0; i < stale.Length; i++)
            {
                CacheEntry<T> o = default(CacheEntry<T>);
                _objects.TryRemove(stale[i], out o);
                _log.DebugFormat("Purged from cache: {0}", stale[i]);
            }
        }
    }
}