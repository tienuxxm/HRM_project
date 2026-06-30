using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Departments;

namespace Application.Departments.GetAll;

internal sealed class GetAllDepartmentsQueryHandler : IQueryHandler<GetAllDepartmentsQuery, List<Department>>
{
    private readonly IDepartmentRepository _departmentRepository;

    public GetAllDepartmentsQueryHandler(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<List<Department>>> Handle(GetAllDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.GetAll(cancellationToken);
        return departments ?? new List<Department>();
    }
}
