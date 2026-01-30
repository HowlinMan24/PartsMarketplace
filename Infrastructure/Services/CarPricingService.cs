using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class CarPricingService : ICarPricingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CarPricingService> _logger;
    private const decimal VAT_RATE = 0.18m;
    private const string EXCHANGE_RATE_API = "https://api.exchangerate-api.com/v4/latest/USD";

    private static decimal? _cachedExchangeRate;
    private static DateTime? _cacheExpiry;

    public CarPricingService(HttpClient httpClient, ILogger<CarPricingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> TransformPriceToMKDAsync(decimal originalPrice, string fromCurrency = "USD")
    {
        var result = await GetTransformedPriceWithTaxAsync(originalPrice, fromCurrency);
        return result.PriceInMKD;
    }

    public async Task<PriceTransformationResult> GetTransformedPriceWithTaxAsync(decimal originalPrice, string fromCurrency = "USD")
    {
        try
        {
            decimal exchangeRate = await GetExchangeRateAsync();

            decimal priceInMKD = originalPrice * exchangeRate;

            decimal taxAmount = priceInMKD * VAT_RATE;

            decimal totalPriceWithTax = priceInMKD + taxAmount;

            return new PriceTransformationResult
            {
                OriginalPrice = originalPrice,
                ExchangeRate = exchangeRate,
                PriceInMKD = priceInMKD,
                TaxAmount = taxAmount,
                TotalPriceWithTax = totalPriceWithTax,
                Currency = "MKD"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming price to MKD");
            decimal fallbackRate = 57m;
            decimal priceInMKD = originalPrice * fallbackRate;
            decimal taxAmount = priceInMKD * VAT_RATE;

            return new PriceTransformationResult
            {
                OriginalPrice = originalPrice,
                ExchangeRate = fallbackRate,
                PriceInMKD = priceInMKD,
                TaxAmount = taxAmount,
                TotalPriceWithTax = priceInMKD + taxAmount,
                Currency = "MKD"
            };
        }
    }

    private async Task<decimal> GetExchangeRateAsync()
    {
        if (_cachedExchangeRate.HasValue && _cacheExpiry.HasValue && DateTime.UtcNow < _cacheExpiry.Value)
        {
            return _cachedExchangeRate.Value;
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(EXCHANGE_RATE_API);

            if (response?.Rates != null && response.Rates.TryGetValue("MKD", out var rate))
            {
                _cachedExchangeRate = rate;
                _cacheExpiry = DateTime.UtcNow.AddHours(1);
                return rate;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch exchange rate from API, using fallback");
        }

        return 57m;
    }

    private class ExchangeRateResponse
    {
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
