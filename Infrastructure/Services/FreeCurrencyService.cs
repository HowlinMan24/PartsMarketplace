using System.Net.Http.Json;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FreeCurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FreeCurrencyService> _logger;
    private const string BaseUrl = "https://api.freecurrencyapi.com/v1/latest";

    private static readonly Dictionary<string, decimal> FallbackRatesVsUsd = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MKD"] = 57.0m,   // North Macedonian Denar
        ["EUR"] = 0.92m,
        ["GBP"] = 0.79m,
        ["CHF"] = 0.88m,
        ["CAD"] = 1.36m,
        ["AUD"] = 1.53m,
        ["JPY"] = 149.0m,
        ["RSD"] = 108.0m,  // Serbian Dinar
        ["BGN"] = 1.80m,   // Bulgarian Lev
        ["ALL"] = 95.0m,   // Albanian Lek
    };

    private static Dictionary<string, decimal>? _cachedRates;
    private static DateTime _cacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public FreeCurrencyService(HttpClient httpClient, IConfiguration configuration, ILogger<FreeCurrencyService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;

        var rates = await GetExchangeRatesAsync();
        var fromKey = fromCurrency.ToUpper();
        var toKey = toCurrency.ToUpper();

        decimal fromRate = 1m;
        decimal toRate = 1m;

        if (!string.Equals(fromCurrency, "USD", StringComparison.OrdinalIgnoreCase))
        {
            if (rates.TryGetValue(fromKey, out var r))
                fromRate = r;
            else if (FallbackRatesVsUsd.TryGetValue(fromKey, out var fb))
            {
                fromRate = fb;
                _logger.LogDebug("Using fallback rate for {Currency} (1 USD = {Rate})", fromCurrency, fb);
            }
            else
            {
                _logger.LogWarning("Currency rate not found for {From}, cannot convert from {From} to {To}", fromCurrency, fromCurrency, toCurrency);
                return amount;
            }
        }

        if (!string.Equals(toCurrency, "USD", StringComparison.OrdinalIgnoreCase))
        {
            if (rates.TryGetValue(toKey, out var r))
                toRate = r;
            else if (FallbackRatesVsUsd.TryGetValue(toKey, out var fb))
            {
                toRate = fb;
                _logger.LogDebug("Using fallback rate for {Currency} (1 USD = {Rate})", toCurrency, fb);
            }
            else
            {
                _logger.LogWarning("Currency rate not found for {To}, cannot convert from {From} to {To}", toCurrency, fromCurrency, toCurrency);
                return amount;
            }
        }

        if (fromRate == 0) return 0;
        return (amount / fromRate) * toRate;
    }

    public async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency = "USD")
    {
        if (_cachedRates != null && DateTime.UtcNow - _cacheTime < CacheDuration)
        {
            return _cachedRates;
        }

        try
        {
            var apiKey = _configuration["CurrencyApi:ApiKey"] ?? "fca_live_KEY8zcU6vV0SOrAWEZNVBrCj4T3yorm0UCDCg8Qk";
            var url = $"{BaseUrl}?apikey={apiKey}&base_currency=USD"; // Always fetch relative to USD for consistency
            
            var response = await _httpClient.GetFromJsonAsync<FreeCurrencyResponse>(url);
            
            if (response?.Data != null)
            {
                _cachedRates = response.Data;
                _cacheTime = DateTime.UtcNow;
                return _cachedRates;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exchange rates");
        }

        return _cachedRates ?? new Dictionary<string, decimal>();
    }

    private class FreeCurrencyResponse
    {
        public Dictionary<string, decimal>? Data { get; set; }
    }
}
