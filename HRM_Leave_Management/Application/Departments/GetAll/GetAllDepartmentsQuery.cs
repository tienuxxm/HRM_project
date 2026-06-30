using Application.Abstractions.Messaging;
using Domain.Departments;

namespace Application.Departments.GetAll;

public sealed record GetAllDepartmentsQuery : IQuery<List<Department>>;
