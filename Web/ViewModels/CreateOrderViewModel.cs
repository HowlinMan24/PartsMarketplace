using Application.DTOs;

namespace Web.ViewModels;

public class CreateOrderViewModel
{
    public int ListingId { get; set; }
    public int Quantity { get; set; } = 1;
    public ListingDto? Listing { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; } = "North Macedonia";

    public decimal? ConvertedPrice { get; set; }
    public string? ConvertedCurrency { get; set; }
}
