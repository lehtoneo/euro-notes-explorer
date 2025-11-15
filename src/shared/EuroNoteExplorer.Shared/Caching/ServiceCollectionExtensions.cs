
using Microsoft.Extensions.DependencyInjection;

namespace EuroNoteExplorer.Shared.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInMemoryCache(this IServiceCollection services)
        {
            services.AddSingleton<ICache, InMemoryCache>();
        }
    }
}
