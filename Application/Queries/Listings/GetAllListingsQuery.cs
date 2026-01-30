using Application.DTOs;
using MediatR;

namespace Application.Queries.Listings;

public class GetAllListingsQuery : IRequest<IEnumerable<ListingDto>>
{
    public bool ActiveOnly { get; set; } = true;
    public string? Category { get; set; }
    public string? ListingType { get; set; }
    public string? UserId { get; set; }
}
