namespace Application.DTOs;

public class CreateOrderDto
{
    public string UserId { get; set; } = string.Empty; // Changed to string to match IdentityUser
    public List<OrderItemCreateDto> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; } = "North Macedonia";
}

public class OrderItemCreateDto
{
    public int ListingId { get; set; }
    public int Quantity { get; set; }
}
