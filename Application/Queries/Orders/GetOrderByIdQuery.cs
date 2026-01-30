using Application.DTOs;
using MediatR;

namespace Application.Queries.Orders;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public int Id { get; set; }
}
