using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Service.Contract
{
    public interface IResponseCacheService
    {
        Task CachResponseAsync(string cacheKey , object response , TimeSpan timeToLive);
        Task<string?> GetCachedReponseAsync(string cacheKey);
    }
}
