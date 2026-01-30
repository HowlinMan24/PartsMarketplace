using Application.DTOs;
using MediatR;

namespace Application.Commands.Listings;

public class CreateListingCommand : IRequest<ListingDto>
{
    public CreateListingDto Listing { get; set; } = null!;
}
