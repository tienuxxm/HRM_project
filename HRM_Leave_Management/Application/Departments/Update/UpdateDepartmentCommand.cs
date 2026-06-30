using Application.Abstractions.Messaging;
using Domain.Departments;

namespace Application.Departments.Update;

public record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId) : ICommand<Department>;
