using Application.DTOs;
using Application.Queries.Orders;
using AutoMapper;
using Domain.Interfaces;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Orders;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public GetUserOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Listing)
            .Where(o => o.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}
