namespace Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Role { get; set; } = "User";

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
