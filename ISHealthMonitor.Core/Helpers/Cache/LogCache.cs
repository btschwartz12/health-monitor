using Microsoft.Extensions.Caching.Memory;


namespace ISHealthMonitor.Core.Helpers.Cache
{
    public class LogCache
    {
        public MemoryCache Cache { get; set; }
        public LogCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024
            });
        }

    }
}
