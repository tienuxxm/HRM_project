using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveTypes;
using Domain.LeaveBalances;

namespace Application.LeaveBalances.Create;

internal sealed class CreateLeaveBalanceCommandHandler : ICommandHandler<CreateLeaveBalanceCommand, BooleanResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveBalanceCommandHandler(
        IEmployeeRepository employeeRepository,
        ILeaveTypeRepository leaveTypeRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra năm hợp lệ: Year phải từ currentYear - 1 đến currentYear + 1
        int currentYear = DateTime.UtcNow.Year;
        if (request.Year < currentYear - 1 || request.Year > currentYear + 1)
        {
            return Result.Failure<BooleanResponse>(LeaveBalanceErrors.InvalidYear);
        }

        // 2. Kiểm tra Employee tồn tại
        var employeeId = new EmployeeId(request.EmployeeId);
        var employeeExists = await _employeeRepository.IsExistedAsync(e => e.Id == employeeId, cancellationToken);
        if (!employeeExists)
        {
            return Result.Failure<BooleanResponse>(LeaveBalanceErrors.EmployeeNotFound);
        }

        // 3. Kiểm tra LeaveType tồn tại và đang active
        var leaveTypeId = new LeaveTypeId(request.LeaveTypeId);
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId, cancellationToken);
        if (leaveType == null || !leaveType.IsActive)
        {
            return Result.Failure<BooleanResponse>(LeaveBalanceErrors.LeaveTypeNotFound);
        }

        // 4. Kiểm tra trùng lặp bản ghi active
        var isExisted = await _leaveBalanceRepository.IsExistedAsync(lb => 
            lb.EmployeeId == employeeId && 
            lb.LeaveTypeId == leaveTypeId && 
            lb.Year == request.Year && 
            lb.IsActive, 
            cancellationToken);

        if (isExisted)
        {
            return Result.Failure<BooleanResponse>(LeaveBalanceErrors.LeaveBalanceExisted);
        }

        // 5. Tạo LeaveBalance mới
        var leaveBalance = LeaveBalance.Create(
            employeeId,
            leaveTypeId,
            request.Year,
            request.AllocatedDays,
            request.UsedDays);

        _leaveBalanceRepository.Add(leaveBalance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave balance allocated successfully."
        });
    }
}
