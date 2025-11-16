using EuroNoteExplorer.Api.Options;
using EuroNoteExplorer.Api.Services.Interfaces;

namespace EuroNoteExplorer.Api.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEuroNoteService(this IServiceCollection serviceCollection, EuroNoteServiceOptions options)
        {
            // needed for the EuroNoteService
            serviceCollection.AddHttpClient();
            serviceCollection.AddSingleton(options);

            serviceCollection.AddTransient<IBoFOpenApiClient, BoFOpenApiClient>();
            serviceCollection.AddTransient<IEuroNoteService, EuroNoteService>();
        }
    }
}
