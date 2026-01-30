using Application.DTOs;
using Application.Queries.Listings;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Listings;

public class GetListingsByCategoryQueryHandler : IRequestHandler<GetListingsByCategoryQuery, IEnumerable<ListingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetListingsByCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ListingDto>> Handle(GetListingsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var listings = await _unitOfWork.Listings.FindAsync(l => l.CategoryId == request.CategoryId && l.IsActive);
        return _mapper.Map<IEnumerable<ListingDto>>(listings);
    }
}
