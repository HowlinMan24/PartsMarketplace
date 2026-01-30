using Application.DTOs;
using MediatR;

namespace Application.Queries.Categories;

public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>
{
}
