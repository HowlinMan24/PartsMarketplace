using Application.DTOs;
using MediatR;

namespace Application.Queries.Orders;

public class GetAllOrdersQuery : IRequest<IEnumerable<OrderDto>>
{
}
