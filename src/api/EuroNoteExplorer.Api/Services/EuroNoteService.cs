using EuroNoteExplorer.Api.Services.Interfaces;
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
    private readonly EuroNoteServiceOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    // Bank of Finland dataset & series names
    private const string Dataset = "BOF_BKN1_PUBL";

    // Setelien nimellisarvot ja niiden tunnisteet
    private static readonly Dictionary<string, int> Denominations = new()
    {
        ["B5"] = 5,
        ["B10"] = 10,
        ["B20"] = 20,
        ["B50"] = 50,
        ["B100"] = 100,
        ["B200"] = 200,
        ["B500"] = 500
    };

    public EuroNoteService(EuroNoteServiceOptions options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<BankNoteSummary>> GetNoteSummariesAsync(BankNoteFilters filters)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var summaries = new List<BankNoteSummary>();



        var exchangeRates = await GetExchangeRatesAsync(DateTime.UtcNow);

        foreach (var (denomCode, nominalValue) in Denominations)
        {
            try
            {

                // Hae kappalemäärät (PN = Pieces)
                var countSeriesName = $"M.FI.NC.BN.EUR.{denomCode}.ALL.PN.ST.F.XX";
                var countData = await GetObservationsAsync(
                    httpClient,
                    countSeriesName,
                    filters.StartPeriod,
                    filters.EndPeriod
                );

                decimal totalCount = countData.LastOrDefault()?.Value ?? 0;
                var totalValue = totalCount * nominalValue;

                // Luo yhteenveto tälle seteliarvalle
                var summary = new BankNoteSummary
                {
                    Denomination = nominalValue,
                    DenominationCode = denomCode,
                    CurrencyCode = "EUR",
                    Value = totalValue,
                    Count = (long)(totalCount),
                    CurrencyValues = exchangeRates.Keys.Select(currencyCode =>
                    {
                        string exchangeRate = exchangeRates[currencyCode];
                        decimal exchangeRateDecimal = _getDecimalFromAPIStr(exchangeRate);

                        decimal val = totalValue * exchangeRateDecimal;
                        string valStr = val.ToString(CultureInfo.InvariantCulture);

						return new CurrencyValue {
                            CurrencyCode = currencyCode,
                            Value = valStr,
                            ExchangeRate = exchangeRateDecimal.ToString(CultureInfo.InvariantCulture),
                        };
                    })
                };

                summaries.Add(summary);
            }
            catch (Exception ex)
            {
                // Logita virhe, mutta jatka muiden seteliarvojen hakua
                Console.WriteLine($"Error fetching data for {denomCode}: {ex.Message}");
            }
        }

        return summaries.OrderBy(s => s.Denomination);
    }

    private async Task<List<BankNoteObservation>> GetObservationsAsync(
        HttpClient httpClient,
        string seriesName,
        DateTime startPeriod,
        DateTime endPeriod)
    {
        var url = $"{_options.BoFOpenAPIBaseUrl}/v4/observations/{Dataset}" +
                  $"?seriesName={Uri.EscapeDataString(seriesName)}" +
                  $"&startPeriod={startPeriod.ToString("o", CultureInfo.InvariantCulture)}" +
                  $"&endPeriod={endPeriod.ToString("o", CultureInfo.InvariantCulture)}" +
                  $"&pageSize=1000";

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BoFObservationsResponse>();

        var observations = result?.Items?.FirstOrDefault()?.Observations ?? new List<BoFObservation>();


        return observations.Select(obs => new BankNoteObservation
        {
            Period = DateTime.Parse(obs.Period),
            PeriodCode = obs.PeriodCode,
            Value = obs.Value
        }).ToList();
    }

    public async Task<Dictionary<string, string>> GetExchangeRatesAsync(DateTime date, string[]? currencies = null)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var currenciesParam = currencies != null && currencies.Length > 0
            ? string.Join(",", currencies)
            : "";

        var url = $"{_options.BoFOpenAPIBaseUrl}/referencerates/v2/api/V2" +
                  $"?startDate={date:yyyy-MM-dd}" +
                  $"&endDate={date:yyyy-MM-dd}";

        if (!string.IsNullOrEmpty(currenciesParam))
        {
            url += $"&currencies={currenciesParam}";
        }

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<ExchangeRateInfo>>();

        return result!.ToDictionary(r => r.Currency, r => r.ExchangeRates?.FirstOrDefault()?.Value ?? "0");
        
    }
    private decimal _getDecimalFromAPIStr(string? str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        return decimal.Parse(str, new NumberFormatInfo() { NumberDecimalSeparator = "," });
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