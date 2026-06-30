using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Categories.Delete;

public sealed record DeleteCategoryCommand : ICommand<BooleanResponse>
{
    public required Guid Id { get; set; }
}