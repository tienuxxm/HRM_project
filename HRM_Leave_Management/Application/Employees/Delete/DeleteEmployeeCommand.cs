using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Employees.Delete;

public sealed record DeleteEmployeeCommand : ICommand<BooleanResponse>
{
    public required Guid Id { get; set; }
}
