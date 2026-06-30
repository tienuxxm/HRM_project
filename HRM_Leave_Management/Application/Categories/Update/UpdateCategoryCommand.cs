using Application.Abstractions.Messaging;
using Domain.Categories;

namespace Application.Categories.Update;

public record UpdateCategoryCommand(
    Guid id, string Name, string Description, int? Index) : ICommand<Category>;