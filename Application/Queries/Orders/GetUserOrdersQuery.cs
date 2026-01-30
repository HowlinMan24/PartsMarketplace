using Application.DTOs;
using MediatR;

namespace Application.Queries.Orders;

public class GetUserOrdersQuery : IRequest<IEnumerable<OrderDto>>
{
    public string UserId { get; set; } = string.Empty; // Changed to string to match IdentityUser
}
