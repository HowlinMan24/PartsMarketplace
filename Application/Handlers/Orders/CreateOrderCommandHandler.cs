using Application.Commands.Orders;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Services;
using MediatR;

namespace Application.Handlers.Orders;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            UserId = request.Order.UserId,
            ShippingAddress = request.Order.ShippingAddress,
            ShippingCity = request.Order.ShippingCity,
            ShippingPostalCode = request.Order.ShippingPostalCode,
            ShippingCountry = request.Order.ShippingCountry,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        decimal totalAmount = 0;
        var listingsToDeactivate = new List<Listing>();

        foreach (var itemDto in request.Order.Items)
        {
            var listing = await _unitOfWork.Listings.GetByIdAsync(itemDto.ListingId);
            if (listing == null)
                throw new InvalidOperationException($"Listing with ID {itemDto.ListingId} not found");

            if (!listing.IsActive)
                throw new InvalidOperationException($"Listing with ID {itemDto.ListingId} is not active");

            var orderItem = new OrderItem
            {
                ListingId = itemDto.ListingId,
                Quantity = itemDto.Quantity,
                UnitPrice = listing.Price,
                Subtotal = listing.Price * itemDto.Quantity
            };

            totalAmount += orderItem.Subtotal;
            order.OrderItems.Add(orderItem);
            listingsToDeactivate.Add(listing);
        }

        order.TotalAmount = totalAmount;

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        foreach (var listing in listingsToDeactivate)
        {
            listing.IsActive = false;
            await _unitOfWork.Listings.UpdateAsync(listing);
        }
        await _unitOfWork.SaveChangesAsync();

        var createdOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
        if (createdOrder == null)
            throw new InvalidOperationException("Failed to create order");

        return _mapper.Map<OrderDto>(createdOrder);
    }
}
