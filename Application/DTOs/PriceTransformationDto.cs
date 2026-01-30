namespace Application.DTOs;

public class PriceTransformationDto
{
    public decimal OriginalPrice { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal PriceInMKD { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPriceWithTax { get; set; }
    public string Currency { get; set; } = "MKD";
}
