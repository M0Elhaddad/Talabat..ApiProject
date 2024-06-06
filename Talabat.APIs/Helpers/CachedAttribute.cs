using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Talabat.Core.Service.Contract;

namespace Talabat.APIs.Helpers
{
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveInSeconds;

        public CachedAttribute(int timeToLiveInSeconds)
        {
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var responseCacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            // Ask CLR For Creation Object From "ResonseCacheService" Explicitly

            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

            var response = await responseCacheService.GetCachedReponseAsync(cacheKey);

            if(!string.IsNullOrEmpty(response))
            {
                var result = new ContentResult()
                {
                    Content = response,
                    ContentType = "application/json",
                    StatusCode = 200,
                };

                context.Result = result;

                return;
            } // Response Is Not Cached

            var executedActionContext = await next.Invoke(); // Will Execute The Next Action Filter Or The Action It Self

            if(executedActionContext.Result is OkObjectResult okObjectResult && okObjectResult.Value is not null)
            {
                await responseCacheService.CachResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_timeToLiveInSeconds));
            }
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            // {{url}}/api/products?pageIndex=1&pageSize=5&sort=name

            var keyBuilder = new StringBuilder();

            keyBuilder.Append(request.Path); // /api/products

            //pageIndex=1
            //pageSize=5
            //sort=name

            foreach (var (key,value) in request.Query.OrderBy(X => X.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
                // /api/products|pageIndex-1
                // /api/products|pageIndex-1|pageSize-5
                // /api/products|pageIndex-1|pageSize-5|sort=name
            }

            return keyBuilder.ToString();
        }
    }
}
