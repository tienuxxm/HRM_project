using Application.Abstractions.Messaging;
using Application.ApprovalRouting.Commands.ReassignPendingLeaveRequests;
using Application.ApprovalRouting.Services;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Commands.UnassignApprovalLevel;

internal sealed class UnassignApprovalLevelCommandHandler
    : ICommandHandler<UnassignApprovalLevelCommand, UnassignApprovalLevelResponse>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IApprovalReassignmentService _reassignmentService;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignApprovalLevelCommandHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IApprovalReassignmentService reassignmentService,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _reassignmentService = reassignmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UnassignApprovalLevelResponse>> Handle(
        UnassignApprovalLevelCommand request,
        CancellationToken cancellationToken)
    {
        var businessToday = DateOnly.FromDateTime(DateTime.UtcNow);

        // Enforce Immediate-only unassign semantics
        if (request.EffectiveToDate.HasValue && request.EffectiveToDate.Value > businessToday)
        {
            return Result.Failure<UnassignApprovalLevelResponse>(
                new Error("ApprovalRouting.FutureUnassignNotSupported",
                    "Scheduled future unassignment is not supported. EffectiveToDate must be today or null for immediate unassignment."));
        }

        var targetAssignmentId = new ApprovalRouteLevelAssignmentId(request.LevelAssignmentId);
        var policies = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        ApprovalRouteLevelAssignment? targetAssignment = null;
        ApprovalRouteLevel? targetLevel = null;
        ApprovalRoutePolicy? targetPolicy = null;

        foreach (var policy in policies)
        {
            foreach (var level in policy.Levels)
            {
                targetAssignment = level.Assignments.FirstOrDefault(a => a.Id == targetAssignmentId);
                if (targetAssignment != null)
                {
                    targetLevel = level;
                    targetPolicy = policy;
                    break;
                }
            }
            if (targetAssignment != null) break;
        }

        if (targetAssignment == null || targetLevel == null || targetPolicy == null)
        {
            return Result.Failure<UnassignApprovalLevelResponse>(
                new Error("ApprovalRouting.LevelAssignmentNotFound", "The specified level assignment was not found."));
        }

        var unassignedEmployeeId = targetAssignment.AssignedEmployeeId;
        var deactivatedDate = businessToday;

        // 1. Deactivate target level assignment in memory (EF Change Tracker sets IsActive = false)
        targetAssignment.Deactivate(deactivatedDate, $"Unassigned: {request.Reason}");

        // 2. Reassign/reroute pending requests affected ONLY by this level slot assignment via service (No SaveChanges inside service)
        var reassignCommand = new ReassignPendingLeaveRequestsCommand(
            unassignedEmployeeId.Value,
            NewApproverEmployeeId: request.NewApproverEmployeeId,
            AutoRerouteUsingResolver: request.AutoRerouteUsingResolver,
            Reason: $"Unassigning level slot assignment ID {request.LevelAssignmentId}: {request.Reason}",
            TargetLevelAssignmentId: request.LevelAssignmentId,
            TargetPolicyId: targetPolicy.Id.Value,
            TargetLevelId: targetLevel.Id.Value);

        var reassignResult = await _reassignmentService.ExecuteReassignmentAsync(reassignCommand, cancellationToken);
        if (reassignResult.IsFailure)
        {
            return Result.Failure<UnassignApprovalLevelResponse>(reassignResult.Error);
        }

        // 3. Single Atomic UnitOfWork commit as Sole Commit Owner for Deactivation + Reassignment + Audit Logs
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UnassignApprovalLevelResponse(
            targetAssignment.Id.Value,
            Unassigned: true,
            deactivatedDate,
            request.Reason));
    }
}
