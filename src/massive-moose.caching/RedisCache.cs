using log4net;
using System;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace massive_moose.caching
{
    public class RedisCache<T> : IObjectCache<T> where T : new()
    {
        private readonly IRedis _redis;
        private readonly ConnectionMultiplexer _conn;
        private readonly ILog _log;
        private readonly IDatabase _db;

        public RedisCache(ConnectionMultiplexer conn, ILog log)
        {
            _conn = conn;
            _log = log;
            _db = conn.GetDatabase();
        }
        public T Get(string key)
        {
            var serializedValue = _db.StringGet(key);
            if (!serializedValue.HasValue)
            { 
                return default(T);
                _log.DebugFormat("Cache miss:{0}", key);
            }

            _log.DebugFormat("Cache returned:{0}", serializedValue.ToString().Length);
            return JsonConvert.DeserializeObject<T>(serializedValue);
        }

        public void Set(string key, T item)
        {
            _db.KeyDelete(key);
            var serializedValue = JsonConvert.SerializeObject(item);
            _log.DebugFormat("Cache store:{0}", serializedValue);
            _db.StringSet(key, serializedValue);
        }
    }
}
