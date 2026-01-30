using Application.DTOs;
using MediatR;

namespace Application.Queries.Listings;

public class GetListingByIdQuery : IRequest<ListingDto?>
{
    public int Id { get; set; }
}
