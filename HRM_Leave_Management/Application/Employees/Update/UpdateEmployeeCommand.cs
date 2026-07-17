using Application.Abstractions.Messaging;
using Domain.Employees;

namespace Application.Employees.Update;

public record UpdateEmployeeCommand(
    Guid Id,
    string FullName,
    string EmployeeCode,
    Guid? DepartmentId,
    Guid? PositionId,
    DateTime JoinDate,
    Guid? ManagerId) : ICommand<Employee>;
