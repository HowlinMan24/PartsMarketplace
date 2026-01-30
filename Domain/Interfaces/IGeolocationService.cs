namespace Domain.Interfaces;

public interface IGeolocationService
{
    Task<string> GetCountryCodeAsync(string ipAddress);
    Task<string> GetCurrencyCodeAsync(string ipAddress);
}
