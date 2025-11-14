using EuroNoteExplorer.Shared.DTOs;
using EuroNoteExplorer.UI.Options;
using EuroNoteExplorer.UI.Services.Interfaces;

namespace EuroNoteExplorer.UI.Services
{
    public class EuroNoteAPIClient : IEuroNoteAPIClient
    {
        public EuroNoteAPIClientOptions _options { get; set; }
        private readonly HttpClient _httpClient;
        public EuroNoteAPIClient(EuroNoteAPIClientOptions options, IHttpClientFactory httpClientFactory) {
            _options = options;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_options.EuroNoteAPIBaseUrl);
        }
        public async Task<IEnumerable<BankNoteSummary>> GetBankNoteSummaryAsync(BankNoteFilters filters)
        {
            var url = $"api/v1/euronotes?startPeriod={filters.StartPeriod:yyyy-MM-dd}&endPeriod={filters.EndPeriod:yyyy-MM-dd}";

            IEnumerable<BankNoteSummary> result = await _httpClient.GetFromJsonAsync<IEnumerable<BankNoteSummary>>(url);

            return result;

        }
    }
}
