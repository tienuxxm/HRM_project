using Application.Abstractions.Messaging;
using Domain.Employees;

namespace Application.Employees.GetAll;

public sealed record GetAllEmployeesQuery : IQuery<List<Employee>>;
