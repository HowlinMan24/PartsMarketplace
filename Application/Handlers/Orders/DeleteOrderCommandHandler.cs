using Application.Commands.Orders;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Orders;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.Id);
        if (order == null)
            return false;

        await _unitOfWork.Orders.DeleteAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
