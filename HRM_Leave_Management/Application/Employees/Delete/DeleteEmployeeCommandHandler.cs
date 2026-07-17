using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveBalances;
using Domain.LeaveRequests;
using Domain.LeaveApproverAssignments;
using Domain.WorkCalendars;
using Domain.Users;
using Application.Abstractions.Authentication;
using Microsoft.Extensions.Logging;

namespace Application.Employees.Delete;

internal sealed class DeleteEmployeeCommandHandler : ICommandHandler<DeleteEmployeeCommand, BooleanResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveApproverAssignmentRepository _leaveApproverAssignmentRepository;
    private readonly ILeaveRequestRecalculationAuditRepository _leaveRequestRecalculationAuditRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEmployeeCommandHandler> _logger;

    public DeleteEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveApproverAssignmentRepository leaveApproverAssignmentRepository,
        ILeaveRequestRecalculationAuditRepository leaveRequestRecalculationAuditRepository,
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _leaveApproverAssignmentRepository = leaveApproverAssignmentRepository;
        _leaveRequestRecalculationAuditRepository = leaveRequestRecalculationAuditRepository;
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(new EmployeeId(request.Id));
        if (employee is null)
            return Result.Failure<BooleanResponse>(EmployeeErrors.NotFound);

        // 1. If active subordinates exist: fail, no DB changes, no Keycloak delete
        var hasActiveSubordinates = await _employeeRepository.IsExistedAsync(
            x => x.ManagerId == employee.Id && x.IsActive, cancellationToken);

        if (hasActiveSubordinates)
        {
            return Result.Failure<BooleanResponse>(EmployeeErrors.HasActiveSubordinates);
        }

        // 2. Check HRM history/dependencies
        // Note: Inactive subordinates are treated as historical links and do not block offboarding,
        // but they are considered HRM history which requires deactivation instead of hard-deletion.
        var hasInactiveSubordinates = await _employeeRepository.IsExistedAsync(
            x => x.ManagerId == employee.Id && !x.IsActive, cancellationToken);

        var hasLeaveBalances = await _leaveBalanceRepository.IsExistedAsync(
            x => x.EmployeeId == employee.Id, cancellationToken);

        var hasLeaveRequests = await _leaveRequestRepository.IsExistedAsync(
            x => x.EmployeeId == employee.Id, cancellationToken);

        var hasApproverAssignments = await _leaveApproverAssignmentRepository.IsExistedAsync(
            x => x.ApproverEmployeeId == employee.Id, cancellationToken);

        var hasRecalculationAudits = await _leaveRequestRecalculationAuditRepository.IsExistedAsync(
            x => x.EmployeeId == employee.Id, cancellationToken);

        var hasHrmHistory = hasInactiveSubordinates || hasLeaveBalances || hasLeaveRequests || hasApproverAssignments || hasRecalculationAudits;

        // 3. Load linked User if any
        User? linkedUser = null;
        if (employee.UserId is not null)
        {
            linkedUser = await _userRepository.GetByIdAsync(employee.UserId, cancellationToken);
        }

        // 4. Capture local variables before mutation
        string employeeId = employee.Id.Value.ToString();
        string? userId = employee.UserId?.Value.ToString();
        string? identityId = linkedUser?.IdentityId?.Value;
        string? username = linkedUser?.Username?.Value;
        string? email = linkedUser?.Email?.Value;

        // 5. Apply database mutations
        if (hasHrmHistory)
        {
            employee.SetActive(false);
            _employeeRepository.Update(employee);
        }
        else
        {
            _employeeRepository.Remove(employee);
        }

        if (linkedUser is not null)
        {
            linkedUser.Delete();
            _userRepository.Update(linkedUser);
        }

        // 6. Persist database changes first
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Revoke Keycloak login access after successful DB commit
        bool wasKeycloakRevoked = false;
        if (!string.IsNullOrEmpty(identityId))
        {
            var deleteUserResult = await _authenticationService.DeleteUser(identityId, cancellationToken);
            if (deleteUserResult.IsFailure)
            {
                _logger.LogError("Keycloak user revocation failed for EmployeeId: {EmployeeId}, UserId: {UserId}, IdentityId: {IdentityId}, Username: {Username}, Email: {Email}. Error: {ErrorCode} - {ErrorMessage}", 
                    employeeId, 
                    userId ?? "N/A", 
                    identityId ?? "N/A", 
                    username ?? "N/A", 
                    email ?? "N/A",
                    deleteUserResult.Error.Code,
                    deleteUserResult.Error.Name);

                return Result.Failure<BooleanResponse>(EmployeeErrors.KeycloakRevokeFailed);
            }
            wasKeycloakRevoked = true;
        }

        string message;
        if (wasKeycloakRevoked)
        {
            message = hasHrmHistory
                ? "Employee deactivated and login access revoked."
                : "Employee deleted and login access revoked.";
        }
        else
        {
            message = hasHrmHistory
                ? "Employee deactivated."
                : "Employee deleted successfully.";
        }

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = message
        });
    }
}
