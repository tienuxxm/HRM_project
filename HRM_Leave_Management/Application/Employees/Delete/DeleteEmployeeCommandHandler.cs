using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;

namespace Application.Employees.Delete;

internal sealed class DeleteEmployeeCommandHandler : ICommandHandler<DeleteEmployeeCommand, BooleanResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(new EmployeeId(request.Id));
        if (employee is null)
            return Result.Failure<BooleanResponse>(EmployeeErrors.NotFound);

        // Check if employee has subordinates
        var hasSubordinates = await _employeeRepository.IsExistedAsync(
            x => x.ManagerId == employee.Id);
        if (hasSubordinates)
            return Result.Failure<BooleanResponse>(EmployeeErrors.HasSubordinates);

        _employeeRepository.Remove(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BooleanResponse
            { Result = true, Message = $"{request.Id} DELETED" });
    }
}
