using Application.DTOs;
using MediatR;

namespace Application.Commands.Orders;

public class UpdateOrderStatusCommand : IRequest<OrderDto>
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
