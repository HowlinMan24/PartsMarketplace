using Application.DTOs;
using MediatR;

namespace Application.Commands.Listings;

public class UpdateListingCommand : IRequest<ListingDto>
{
    public int Id { get; set; }
    public CreateListingDto Listing { get; set; } = null!;
}
