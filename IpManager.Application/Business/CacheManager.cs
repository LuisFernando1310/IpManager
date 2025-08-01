using System.Runtime.Caching;

namespace IpManager.Application.Business
{
    public class CacheManager
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;

        public static T GetOrAdd<T> (string key, Func<T> value, DateTimeOffset expiration)
        {
            if(Cache.Contains(key))
            {
                return (T)Cache[key];
            }
            else
            {
                var newValue = value();
                if (newValue !=null)
                    Cache.Set(key, newValue, expiration);

                return newValue;
            }
        }
    }
}
