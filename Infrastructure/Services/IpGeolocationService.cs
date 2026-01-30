using System.Net.Http.Json;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class IpGeolocationService : IGeolocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IpGeolocationService> _logger;
    private const string BaseUrl = "http://ip-api.com/json";

    public IpGeolocationService(HttpClient httpClient, IConfiguration configuration, ILogger<IpGeolocationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetCountryCodeAsync(string ipAddress)
    {
        var data = await GetGeolocationDataAsync(ipAddress);
        return data?.CountryCode2 ?? data?.CountryCode ?? "MK";
    }

    public async Task<string> GetCurrencyCodeAsync(string ipAddress)
    {
        var data = await GetGeolocationDataAsync(ipAddress);
        return data?.CurrencyCode ?? data?.Currency?.Code ?? "MKD";
    }

    private async Task<IpGeolocationResponse?> GetGeolocationDataAsync(string ipAddress)
    {
        try
        {
            string url;
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                 url = $"{BaseUrl}";
            }
            else
            {
                 url = $"{BaseUrl}/{ipAddress}";
            }

            _logger.LogInformation("Calling geolocation API: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<IpGeolocationResponse>(url);

            _logger.LogInformation("Geolocation API response for IP {IP}: CountryCode={CountryCode}, Currency={Currency}",
                ipAddress,
                response?.CountryCode ?? "(null)",
                response?.CurrencyCode ?? response?.Currency?.Code ?? "(null)");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching geolocation data");
            return null;
        }
    }

    private class IpGeolocationResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("country_code2")]
        public string? CountryCode2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("currency")]
        public string? CurrencyCode { get; set; }

        public CurrencyInfo? Currency { get; set; }
    }

    private class CurrencyInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}
