
using Microsoft.Extensions.DependencyInjection;

namespace EuroNoteExplorer.Shared.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInMemoryCache(this IServiceCollection services)
        {
            services.AddSingleton<ICache, InMemoryCache>();
        }

		public static void AddRedisCache(this IServiceCollection services, RedisOptions redisOpts)
		{
            services.AddSingleton(redisOpts);
			services.AddSingleton<ICache, RedisCache>();
		}
	}
}
