using Application.DTOs;
using Application.Queries.Pricing;
using Infrastructure.Services;
using MediatR;

namespace Application.Handlers.Pricing;

public class GetTransformedPriceQueryHandler : IRequestHandler<GetTransformedPriceQuery, PriceTransformationDto>
{
    private readonly ICarPricingService _carPricingService;

    public GetTransformedPriceQueryHandler(ICarPricingService carPricingService)
    {
        _carPricingService = carPricingService;
    }

    public async Task<PriceTransformationDto> Handle(GetTransformedPriceQuery request, CancellationToken cancellationToken)
    {
        var result = await _carPricingService.GetTransformedPriceWithTaxAsync(request.OriginalPrice, request.FromCurrency);
        
        return new PriceTransformationDto
        {
            OriginalPrice = result.OriginalPrice,
            ExchangeRate = result.ExchangeRate,
            PriceInMKD = result.PriceInMKD,
            TaxAmount = result.TaxAmount,
            TotalPriceWithTax = result.TotalPriceWithTax,
            Currency = result.Currency
        };
    }
}
