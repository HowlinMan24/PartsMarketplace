namespace Application.DTOs;

public class CreateListingDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int CategoryId { get; set; }
    public string ListingType { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Condition { get; set; }
    public string? ImageUrl { get; set; }
    public string UserId { get; set; } = string.Empty; // Changed to string to match IdentityUser
}
