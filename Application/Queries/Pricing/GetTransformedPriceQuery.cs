using Application.DTOs;
using MediatR;

namespace Application.Queries.Pricing;

public class GetTransformedPriceQuery : IRequest<PriceTransformationDto>
{
    public decimal OriginalPrice { get; set; }
    public string FromCurrency { get; set; } = "USD";
}
