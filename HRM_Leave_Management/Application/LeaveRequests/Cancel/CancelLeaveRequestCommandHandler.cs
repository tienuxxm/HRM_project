using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.LeaveRequests.Cancel;

internal sealed class CancelLeaveRequestCommandHandler : ICommandHandler<CancelLeaveRequestCommand, BooleanResponse>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CancelLeaveRequestCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin Employee từ UserContext
        var identityId = _userContext.IdentityId;
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);
        if (user == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        var employee = await _employeeRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.IsActive, cancellationToken);
        if (employee == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.EmployeeNotFound);
        }

        // 2. Tìm đơn nghỉ phép
        var leaveRequestId = new LeaveRequestId(request.LeaveRequestId);
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(leaveRequestId, cancellationToken);
        if (leaveRequest == null)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.NotFound);
        }

        // 3. Bảo mật: Chỉ cho phép nhân viên hủy đơn của chính mình
        if (leaveRequest.EmployeeId != employee.Id)
        {
            return Result.Failure<BooleanResponse>(LeaveRequestErrors.NotFound);
        }

        // 4. Validate trạng thái đang chờ duyệt
        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            return Result.Failure<BooleanResponse>(new Error(
                "LeaveRequest.InvalidStatusForCancel",
                "Only pending leave requests can be canceled."));
        }

        // 5. Tiến hành hủy đơn
        var utcNow = _dateTimeProvider.UtcNow;
        leaveRequest.Cancel(utcNow);

        _leaveRequestRepository.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Leave request canceled successfully."
        });
    }
}
