using Microsoft.Extensions.Caching.Memory;

namespace WinterWay.Services
{
    public class RateLimiterService
    {
        private readonly IMemoryCache _memoryCache;

        public RateLimiterService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void SetLastRequestTime(string userId, string requestType)
        {
            _memoryCache.Set($"{userId}-{requestType}", DateTime.UtcNow);
        }

        public Boolean IsRequestAvailableAgain(string userId, string requestType, TimeSpan timeSpan)
        {
            var recordExists = _memoryCache.TryGetValue($"{userId}-{requestType}", out DateTime lastRequestTime);
            if (!recordExists)
            {
                return true;
            }
            return lastRequestTime + timeSpan < DateTime.UtcNow;
        }
    }
}