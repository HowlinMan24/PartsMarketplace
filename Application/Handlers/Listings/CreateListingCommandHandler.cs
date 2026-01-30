using Application.Commands.Listings;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Listings;

public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, ListingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public CreateListingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<ListingDto> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var listing = _mapper.Map<Listing>(request.Listing);
        await _unitOfWork.Listings.AddAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        var createdListing = await _unitOfWork.Listings.GetByIdAsync(listing.Id);
        if (createdListing == null)
            throw new InvalidOperationException("Failed to create listing");

        return _mapper.Map<ListingDto>(createdListing);
    }
}
