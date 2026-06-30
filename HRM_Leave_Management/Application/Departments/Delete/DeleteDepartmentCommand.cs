using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Departments.Delete;

public sealed record DeleteDepartmentCommand : ICommand<BooleanResponse>
{
    public required Guid Id { get; set; }
}
