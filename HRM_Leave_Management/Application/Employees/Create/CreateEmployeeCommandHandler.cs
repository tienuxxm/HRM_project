using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Departments;
using Domain.Employees;
using Domain.Positions;
using Domain.Users;

namespace Application.Employees.Create;

internal sealed class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, BooleanResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var isCodeExisted = await _employeeRepository.IsExistedAsync(x => x.EmployeeCode == request.EmployeeCode);
        if (isCodeExisted)
            return Result.Failure<BooleanResponse>(EmployeeErrors.EmployeeCodeExisted);

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

        var employee = Employee.Create(
            request.FullName,
            request.EmployeeCode,
            departmentId,
            userId,
            positionId,
            DateTime.SpecifyKind(request.JoinDate, DateTimeKind.Utc),
            managerId);

        _employeeRepository.Add(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
            { Result = true, Message = $"Employee: {request.FullName} has been created" });
    }
}
