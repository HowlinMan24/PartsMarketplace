namespace Application.DTOs;

public class ListingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ListingType { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Condition { get; set; }
    public string? ImageUrl { get; set; }
    public string UserId { get; set; } = string.Empty; // Changed to string to match IdentityUser
    public string UserName { get; set; } = string.Empty;
    public string SellerFirstName { get; set; } = string.Empty;
    public string SellerLastName { get; set; } = string.Empty;
    public string? SellerPhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
