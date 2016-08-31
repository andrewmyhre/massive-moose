using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace massive_moose.caching
{

    public class RedisCache<T> : IObjectCache<T> where T : new()
    {
        public T Get(string key)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, T item)
        {
            throw new NotImplementedException();
        }
    }
}
