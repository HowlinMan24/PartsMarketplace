using Application.DTOs;
using MediatR;

namespace Application.Queries.Listings;

public class GetListingsByCategoryQuery : IRequest<IEnumerable<ListingDto>>
{
    public int CategoryId { get; set; }
}
