using Application.Abstractions.Messaging;
using Application.Response;

namespace Application.Employees.Create;

public sealed record CreateEmployeeCommand(
    string FullName,
    string? EmployeeCode,
    Guid? DepartmentId,
    Guid? UserId,
    Guid? PositionId,
    DateTime JoinDate,
    Guid? ManagerId) : ICommand<BooleanResponse>;
