using Application.DTOs;

namespace Web.ViewModels;

public class ListingIndexItemViewModel
{
    public ListingDto Listing { get; set; } = null!;
    public PriceTransformationDto PriceTransformation { get; set; } = null!;
}
