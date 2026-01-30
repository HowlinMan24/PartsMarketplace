using MediatR;

namespace Application.Commands.Listings;

public class DeleteListingCommand : IRequest<bool>
{
    public int Id { get; set; }
}
