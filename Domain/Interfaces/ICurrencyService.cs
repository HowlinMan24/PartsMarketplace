namespace Domain.Interfaces;

public interface ICurrencyService
{
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency = "USD");
}
