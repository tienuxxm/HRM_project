using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Employees;

namespace Application.Employees.GetAll;

internal sealed class GetAllEmployeesQueryHandler : IQueryHandler<GetAllEmployeesQuery, List<Employee>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<List<Employee>>> Handle(GetAllEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetAll(cancellationToken);
        return employees ?? new List<Employee>();
    }
}
