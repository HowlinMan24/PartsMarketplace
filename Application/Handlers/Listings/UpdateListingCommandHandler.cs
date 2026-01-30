using Application.Commands.Listings;
using Application.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Listings;

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand, ListingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateListingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ListingDto> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _unitOfWork.Listings.GetByIdAsync(request.Id);
        if (listing == null)
            throw new InvalidOperationException($"Listing with ID {request.Id} not found");

        _mapper.Map(request.Listing, listing);
        await _unitOfWork.Listings.UpdateAsync(listing);
        await _unitOfWork.SaveChangesAsync();

        var updatedListing = await _unitOfWork.Listings.GetByIdAsync(request.Id);
        if (updatedListing == null)
            throw new InvalidOperationException("Failed to update listing");

        return _mapper.Map<ListingDto>(updatedListing);
    }
}
