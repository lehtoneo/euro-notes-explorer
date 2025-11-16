using EuroNoteExplorer.Api.Services.Interfaces;
using EuroNoteExplorer.Shared.Caching;
using EuroNoteExplorer.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class EuroNoteServiceOptions
{
    public required string BoFOpenAPIBaseUrl { get; set; }
}

public class EuroNoteService : IEuroNoteService
{
	private readonly IBoFOpenApiClient _apiClient;
	private readonly ICache _cache;
	private readonly ILogger<EuroNoteService> _logger;

	private static readonly IReadOnlyDictionary<string, int> Denominations =
		new Dictionary<string, int>
		{
			["B5"] = 5,
			["B10"] = 10,
			["B20"] = 20,
			["B50"] = 50,
			["B100"] = 100,
			["B200"] = 200,
			["B500"] = 500
		};

	public EuroNoteService(
		IBoFOpenApiClient apiClient,
		ICache cache,
		ILogger<EuroNoteService> logger)
	{
		_apiClient = apiClient;
		_cache = cache;
		_logger = logger;
	}

	public async Task<IEnumerable<BankNoteSummary>> GetNoteSummariesAsync(BankNoteFilters filters)
	{
		var today = DateTime.UtcNow.Date;

		var cacheKey = $"fx:{today:yyyyMMdd}";
		var exchangeRates = _cache.Get<Dictionary<string, string>>(cacheKey);

		if (exchangeRates is null)
		{
			exchangeRates = await _apiClient.GetDailyExchangeRatesAsync(today);
			_cache.Set(cacheKey, exchangeRates, TimeSpan.FromHours(24));
		}

		var summaries = new List<BankNoteSummary>();

		foreach (var (code, nominalValue) in Denominations)
		{
			try
			{
				var observations = await _apiClient.GetBanknotePiecesAsync(
					code, filters.StartPeriod, filters.EndPeriod);

				decimal count = observations.LastOrDefault()?.Value ?? 0;
				decimal totalValue = count * nominalValue;

				summaries.Add(_buildSummary(code, nominalValue, count, totalValue, exchangeRates));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching banknote data for {Code}", code);
			}
		}

		return summaries.OrderBy(s => s.Denomination);
	}

	private static BankNoteSummary _buildSummary(
		string code,
		int nominalValue,
		decimal count,
		decimal totalValue,
		Dictionary<string, string> fx
		)
	{
		return new BankNoteSummary
		{
			Denomination = nominalValue,
			DenominationCode = code,
			CurrencyCode = "EUR",
			Value = totalValue,
			Count = (long)count,
			CurrencyValues = fx.Select(r =>
			{
				decimal rate = _parseAPIDecimal(r.Value);

				decimal convertedValue = totalValue * rate;

				return new CurrencyValue
				{
					CurrencyCode = r.Key,
					ExchangeRate = rate.ToString(CultureInfo.InvariantCulture),
					Value = convertedValue.ToString(CultureInfo.InvariantCulture)
				};
			})
		};
	}

	private static decimal _parseAPIDecimal(string? str)
	{
		if (string.IsNullOrEmpty(str))
			return 0;

		return decimal.Parse(str, new NumberFormatInfo
		{
			NumberDecimalSeparator = ","
		});
	}

}


// ============ BoF API RESPONSE MODELS ============
public class BoFObservationsResponse
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<BoFSeriesItem>? Items { get; set; }
}

public class BoFSeriesItem
{
    public string Dataset { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<BoFObservation> Observations { get; set; } = new();
}

public class BoFObservation
{
    public string Period { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public decimal Value { get; set; }
}


public class ExchangeRateInfo
{
    public List<ObservationRate>? ExchangeRates { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencyDenom { get; set; } = string.Empty;
    public string CurrencyNameFi { get; set; } = string.Empty;
    public string CurrencyNameSe { get; set; } = string.Empty;
    public string CurrencyNameEn { get; set; } = string.Empty;
    public string ECBPublished { get; set; } = string.Empty;
}

public class ObservationRate
{
    public string ObservationDate { get; set; } = string.Empty;
    public string Value { get; set; }
}