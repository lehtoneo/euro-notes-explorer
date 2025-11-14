using EuroNoteExplorer.UI.Options;
using EuroNoteExplorer.UI.Services.Interfaces;

namespace EuroNoteExplorer.UI.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEuroNoteAPIClient(this IServiceCollection services, EuroNoteAPIClientOptions opts)
        {
            services.AddHttpClient();
            services.AddSingleton(opts);
            services.AddSingleton<IEuroNoteAPIClient, EuroNoteAPIClient>();
        }
    }
}
