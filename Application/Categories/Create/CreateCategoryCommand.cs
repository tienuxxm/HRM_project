using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Categories.Create;

public sealed record CreateCategoryCommand(string Name, string Description, int? Index) : ICommand<BooleanResponse>;