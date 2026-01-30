using Application.DTOs;
using Application.Queries.Listings;
using AutoMapper;
using Domain.Interfaces;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers.Listings;

public class GetListingByIdQueryHandler : IRequestHandler<GetListingByIdQuery, ListingDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetListingByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ListingDto?> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await _unitOfWork.Listings.GetByIdAsync(request.Id);
        if (listing == null)
            return null;

        var listingDto = _mapper.Map<ListingDto>(listing);

        var seller = await _userManager.FindByIdAsync(listing.UserId);
        if (seller != null)
        {
            listingDto.SellerFirstName = seller.FirstName;
            listingDto.SellerLastName = seller.LastName;
            listingDto.SellerPhoneNumber = seller.PhoneNumber;
        }

        return listingDto;
    }
}
