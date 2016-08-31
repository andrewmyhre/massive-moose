using System;

namespace massive_moose.services.caching
{
    public interface ICacheEntry
    {
        DateTimeOffset DateTimeOffset { get; set; }
    }
}