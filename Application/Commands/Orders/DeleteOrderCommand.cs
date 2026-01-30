using MediatR;

namespace Application.Commands.Orders;

public class DeleteOrderCommand : IRequest<bool>
{
    public int Id { get; set; }
}
