using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Departments;
using Domain.Employees;
using Domain.Positions;
using Domain.Users;

namespace Application.Employees.Update;

internal sealed class UpdateEmployeeCommandHandler : ICommandHandler<UpdateEmployeeCommand, Employee>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Employee>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(new EmployeeId(request.Id));
        if (employee is null)
        {
            return Result.Failure<Employee>(EmployeeErrors.NotFound);
        }

        // Check duplicate code (exclude self)
        var isDuplicate = await _employeeRepository.IsExistedAsync(
            x => x.EmployeeCode == request.EmployeeCode && x.Id != new EmployeeId(request.Id));
        if (isDuplicate)
            return Result.Failure<Employee>(EmployeeErrors.EmployeeCodeExisted);

        DepartmentId? departmentId = request.DepartmentId.HasValue
            ? new DepartmentId(request.DepartmentId.Value)
            : null;

        UserId? userId = request.UserId.HasValue
            ? new UserId(request.UserId.Value)
            : null;

        EmployeeId? managerId = request.ManagerId.HasValue
            ? new EmployeeId(request.ManagerId.Value)
            : null;

        PositionId? positionId = request.PositionId.HasValue
            ? new PositionId(request.PositionId.Value)
            : null;

        employee.Update(request.FullName, request.EmployeeCode, departmentId, userId,
            positionId, DateTime.SpecifyKind(request.JoinDate, DateTimeKind.Utc), managerId);

        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee;
    }
}
