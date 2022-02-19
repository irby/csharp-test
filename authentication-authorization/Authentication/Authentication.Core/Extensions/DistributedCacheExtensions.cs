using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Authentication.Core.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            await distributedCache.SetStringAsync(CreateFullKey<T>(key), JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), options, token);
        }
  
        public static async Task<T> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default) where T : class
        {
            var result = await distributedCache.GetStringAsync(CreateFullKey<T>(key), token);

            return string.IsNullOrWhiteSpace(result) ? null : JsonConvert.DeserializeObject<T>(result);
        }
        
        public static async Task RemoveAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default) where T : class
        {
            await distributedCache.RemoveAsync(CreateFullKey<T>(key), token);
        }

        private static string CreateFullKey<T>(string key)
        {
            return $"{typeof(T).Name}:{key}";
        }
    }
}