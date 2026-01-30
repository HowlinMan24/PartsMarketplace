using Application.Commands.Listings;
using Domain.Interfaces;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Listings;

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public DeleteListingCommandHandler(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<bool> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _unitOfWork.Listings.GetByIdAsync(request.Id);
        if (listing == null)
            return false;

        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ListingId == request.Id, cancellationToken);
        if (hasOrders)
        {
            throw new InvalidOperationException("Cannot delete listing that is part of existing orders. Please deactivate it instead.");
        }

        await _unitOfWork.Listings.DeleteAsync(listing);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
