using Application.Commands.Orders;
using Application.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Orders;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        order.Status = request.Status;
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (updatedOrder == null)
            throw new InvalidOperationException("Failed to update order");

        return _mapper.Map<OrderDto>(updatedOrder);
    }
}
