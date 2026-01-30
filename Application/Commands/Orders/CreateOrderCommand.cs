using Application.DTOs;
using MediatR;

namespace Application.Commands.Orders;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public CreateOrderDto Order { get; set; } = null!;
}
