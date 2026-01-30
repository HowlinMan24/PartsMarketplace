using Application.DTOs;
using Application.Queries.Listings;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Listings;

public class GetAllListingsQueryHandler : IRequestHandler<GetAllListingsQuery, IEnumerable<ListingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllListingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ListingDto>> Handle(GetAllListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await _unitOfWork.Listings.GetAllAsync();
        
        var filteredListings = listings.AsEnumerable();

        if (!string.IsNullOrEmpty(request.UserId))
        {
            filteredListings = filteredListings.Where(l => l.UserId == request.UserId);
        }
        
        if (request.ActiveOnly)
        {
            filteredListings = filteredListings.Where(l => l.IsActive);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            filteredListings = filteredListings.Where(l => l.Category?.Name == request.Category);
        }

        if (!string.IsNullOrEmpty(request.ListingType))
        {
            filteredListings = filteredListings.Where(l => l.ListingType == request.ListingType);
        }

        return _mapper.Map<IEnumerable<ListingDto>>(filteredListings.ToList());
    }
}
