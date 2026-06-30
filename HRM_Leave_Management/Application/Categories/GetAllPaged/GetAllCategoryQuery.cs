using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Categories;

namespace Application.Categories.GetAllPaged;

public sealed record GetAllCategoryQuery() : PagedQuery<Category, CategoryId>, IQuery<GetAllCategoryResponse>;