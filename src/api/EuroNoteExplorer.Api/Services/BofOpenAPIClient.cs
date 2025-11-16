using System.Globalization;
using EuroNoteExplorer.Api.Services.Interfaces;
using EuroNoteExplorer.Shared.DTOs;

namespace EuroNoteExplorer.Api.Services
{
	public class BoFOpenApiClient : IBoFOpenApiClient
	{
		private const string Dataset = "BOF_BKN1_PUBL";

		private readonly IHttpClientFactory _clientFactory;
		private readonly EuroNoteServiceOptions _options;
		private readonly ILogger<BoFOpenApiClient> _logger;

		public BoFOpenApiClient(
			IHttpClientFactory clientFactory,
			EuroNoteServiceOptions options,
			ILogger<BoFOpenApiClient> logger)
		{
			_clientFactory = clientFactory;
			_options = options;
			_logger = logger;
		}

		public async Task<IReadOnlyList<BankNoteObservation>> GetBanknotePiecesAsync(
			string denominationCode,
			DateTime start,
			DateTime end)
		{
			var http = _clientFactory.CreateClient();

			// series format hidden inside the client
			string seriesName = BuildCountSeriesName(denominationCode);

			var url =
				$"{_options.BoFOpenAPIBaseUrl}/v4/observations/{Dataset}" +
				$"?seriesName={Uri.EscapeDataString(seriesName)}" +
				$"&startPeriod={start:o}&endPeriod={end:o}&pageSize=1000";

			var response = await http.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var dto = await response.Content.ReadFromJsonAsync<BoFObservationsResponse>();
			var observations = dto?.Items?.FirstOrDefault()?.Observations ?? new();

			return observations.Select(obs => new BankNoteObservation
			{
				Period = DateTime.Parse(obs.Period, CultureInfo.InvariantCulture),
				PeriodCode = obs.PeriodCode,
				Value = obs.Value
			}).ToList();
		}

		private static string BuildCountSeriesName(string code)
			=> $"M.FI.NC.BN.EUR.{code}.ALL.PN.ST.F.XX";

		public async Task<Dictionary<string, string>> GetDailyExchangeRatesAsync(
			DateTime date,
			string[]? currencies = null)
		{
			var http = _clientFactory.CreateClient();

			var url = $"{_options.BoFOpenAPIBaseUrl}/referencerates/v2/api/V2" +
					  $"?startDate={date:yyyy-MM-dd}&endDate={date:yyyy-MM-dd}";

			if (currencies?.Any() == true)
				url += $"&currencies={string.Join(",", currencies)}";

			var response = await http.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var dto = await response.Content.ReadFromJsonAsync<List<ExchangeRateInfo>>();

			return dto!.ToDictionary(
				r => r.Currency,
				r => r.ExchangeRates?.FirstOrDefault()?.Value ?? "0"
			);
		}
	}

}
