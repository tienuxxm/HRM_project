using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Departments.Create;

public sealed record CreateDepartmentCommand(
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId) : ICommand<BooleanResponse>;
