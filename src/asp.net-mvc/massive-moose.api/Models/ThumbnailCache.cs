using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using massive_moose.services.caching;

namespace massive_moose.api.Models
{
    public class ThumbnailCacheEntry : ICacheEntry
    {
        public byte[] Data { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }

        public ThumbnailCacheEntry(byte[] data)
        {
            DateTimeOffset = DateTimeOffset.Now.AddMinutes(1);
        }
    }
    public class ThumbnailCache
    {
        private ConcurrentDictionary<string,ThumbnailCacheEntry> _objects = new ConcurrentDictionary<string, ThumbnailCacheEntry>();

        public void Set(string key, byte[] data)
        {
            _objects[key] = new ThumbnailCacheEntry(data);
        }

        public byte[] Get(string key)
        {
            if (_objects.ContainsKey(key))
                return _objects[key].Data;

            return null;
        }

        private void PurgeStaleData()
        {
            var stale = _objects.Where(t => t.Value.DateTimeOffset < DateTimeOffset.Now).Select(t=>t.Key).ToArray();
            for(int i=0;i<stale.Length;i++)
            {
                ThumbnailCacheEntry t = null;
                _objects.TryRemove(stale[i], out t);
            }
        }
    }
}