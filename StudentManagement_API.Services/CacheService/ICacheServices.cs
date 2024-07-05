using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Services.CacheService
{
    public interface ICacheServices
    {
        IList<T> GetListCachedResponse<T>(string cacheKey);

        T GetSingleCachedResponse<T>(string cacheKey);

        void SetListCachedResponse<T>(string cacheKey, IEnumerable<T> list);

        void SetSingleCachedResponse<T>(string cacheKey, T data);
    }
}
