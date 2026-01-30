namespace Infrastructure.Services;

public interface ICarPricingService
{
    Task<decimal> TransformPriceToMKDAsync(decimal originalPrice, string fromCurrency = "USD");
    Task<PriceTransformationResult> GetTransformedPriceWithTaxAsync(decimal originalPrice, string fromCurrency = "USD");
}

public class PriceTransformationResult
{
    public decimal OriginalPrice { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal PriceInMKD { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPriceWithTax { get; set; }
    public string Currency { get; set; } = "MKD";
}
