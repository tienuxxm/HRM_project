using Application.Abstractions.Messaging;
using Application.Categories.GetOne;

namespace Application.Categories.GetAll;

public record GetAllCategoryCommand() : ICommand<List<CategoryResponse>>;