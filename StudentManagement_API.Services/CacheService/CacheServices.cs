using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Services.CacheService
{
    public class CacheServices : ICacheServices
    {

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly IMemoryCache _memoryCache;
        public CacheServices(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
       
        public IList<T> GetListCachedResponse<T>(string cacheKey)
        {
            bool isAvaiable = _memoryCache.TryGetValue(cacheKey, out List<T> list);
            if (isAvaiable) return list;
            try
            {
                semaphore.Wait();
                isAvaiable = _memoryCache.TryGetValue(cacheKey, out list);
              
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }
            return list;
        }


        public T GetSingleCachedResponse<T>(string cacheKey)
        {
            bool isAvaiable = _memoryCache.TryGetValue(cacheKey, out T data);
            if (isAvaiable) return data;
            try
            {
                semaphore.Wait();
                isAvaiable = _memoryCache.TryGetValue(cacheKey, out data);
                if (isAvaiable) return data;
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }
            return data;
        }

        public void SetListCachedResponse<T>(string cacheKey, IEnumerable<T> list)
        {
            bool isAvaiable = _memoryCache.TryGetValue(cacheKey, out IEnumerable<T> cachelist);
            if (!isAvaiable)
            {
                cachelist = list;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2),
                    Size = 1024,
                };
                _memoryCache.Set(cacheKey, cachelist, cacheEntryOptions);
            }
         
        }

        public void SetSingleCachedResponse<T>(string cacheKey, T data)
        {
            bool isAvaiable = _memoryCache.TryGetValue(cacheKey, out T cacheData);
            if (!isAvaiable)
            {
                cacheData = data;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2),
                    Size = 1024,
                };
                _memoryCache.Set(cacheKey, cacheData, cacheEntryOptions);
            }

        }
    }
}
