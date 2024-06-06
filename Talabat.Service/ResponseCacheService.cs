using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Core.Service.Contract;

namespace Talabat.Service
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDatabase _database;
        public ResponseCacheService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
        public async Task CachResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if (response is null) return;

            var serializeOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var serializedResponse = JsonSerializer.Serialize(response, serializeOptions);

            await _database.StringSetAsync(cacheKey, serializedResponse, timeToLive);
        }

        public async Task<string?> GetCachedReponseAsync(string cacheKey)
        {
            var cachedResponse = await _database.StringGetAsync(cacheKey);

            if(cachedResponse.IsNullOrEmpty) return null;

            return cachedResponse;
        }
    }
}
