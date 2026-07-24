using Application.Abstractions.Messaging;
using Application.ApprovalRouting.Commands.ReassignPendingLeaveRequests;
using Application.ApprovalRouting.Services;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Commands.InactivateEmployeeWithReassignment;

internal sealed class InactivateEmployeeWithReassignmentCommandHandler
    : ICommandHandler<InactivateEmployeeWithReassignmentCommand, InactivateEmployeeWithReassignmentResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IApprovalReassignmentService _reassignmentService;
    private readonly IUnitOfWork _unitOfWork;

    public InactivateEmployeeWithReassignmentCommandHandler(
        IEmployeeRepository employeeRepository,
        IApprovalRoutePolicyRepository policyRepository,
        IApprovalReassignmentService reassignmentService,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _policyRepository = policyRepository;
        _reassignmentService = reassignmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InactivateEmployeeWithReassignmentResponse>> Handle(
        InactivateEmployeeWithReassignmentCommand request,
        CancellationToken cancellationToken)
    {
        var employeeId = new EmployeeId(request.EmployeeId);
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            return Result.Failure<InactivateEmployeeWithReassignmentResponse>(EmployeeErrors.NotFound);
        }

        // 1. Inactivate employee
        employee.SetActive(false);
        _employeeRepository.Update(employee);

        // 2. Inactivate active level slots assigned to this employee using explicit eager loading
        var policies = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        int unassignedLevelSlots = 0;
        var businessToday = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var policy in policies)
        {
            foreach (var level in policy.Levels)
            {
                foreach (var assignment in level.Assignments.Where(a => a.AssignedEmployeeId == employeeId && a.IsActive))
                {
                    assignment.Deactivate(businessToday, $"Inactivating employee ID {request.EmployeeId}: {request.InactivateReason}");
                    unassignedLevelSlots++;
                }
            }
        }

        // 3. Reassign pending requests via IApprovalReassignmentService directly (no nested MediatR command call)
        var reassignCommand = new ReassignPendingLeaveRequestsCommand(
            request.EmployeeId,
            NewApproverEmployeeId: request.NewApproverEmployeeId,
            AutoRerouteUsingResolver: request.AutoReroutePendingRequests,
            Reason: $"Inactivating employee ID {request.EmployeeId}: {request.InactivateReason}");

        var reassignResult = await _reassignmentService.ExecuteReassignmentAsync(reassignCommand, cancellationToken);

        if (reassignResult.IsFailure)
        {
            return Result.Failure<InactivateEmployeeWithReassignmentResponse>(reassignResult.Error);
        }

        var reassignResponse = reassignResult.Value;

        // 4. Single atomic UnitOfWork commit as Sole Commit Owner for Employee Inactivation + Slot Deactivation + Reassignment + Audit Logs
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new InactivateEmployeeWithReassignmentResponse(
            employee.Id.Value,
            EmployeeInactivated: true,
            unassignedLevelSlots,
            reassignResponse.TotalProcessed,
            reassignResponse.ReassignedCount,
            reassignResponse.NeedsAdminAttentionCount));
    }
}
