using Application.Abstractions.ApprovalRouting;
using Application.Abstractions.Role;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

internal sealed class ApprovalRouteResolverService : IApprovalRouteResolverService
{
    private const string RouteNotConfiguredMessage = "Department approval policy or valid approver is not configured.";

    private readonly ApplicationDbContext _dbContext;
    private readonly IRoleService _roleService;

    public ApprovalRouteResolverService(
        ApplicationDbContext dbContext,
        IRoleService roleService)
    {
        _dbContext = dbContext;
        _roleService = roleService;
    }

    public async Task<ApprovalRouteResolutionResult> ResolveApproverAsync(
        Employee requester,
        CancellationToken cancellationToken = default,
        ApprovalRouteLevelAssignmentId? excludedLevelAssignmentId = null)
    {
        if (requester == null)
        {
            return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
        }

        // 1. Query Active Policy strictly based on requester department scope:
        //    - If requester has a DepartmentId: query policy matching p.DepartmentId == requester.DepartmentId.
        //    - If requester has DepartmentId == null (company-level scope): query policy where p.DepartmentId == null.
        //    - If no policy is found: FAIL immediately with ApprovalRouteNotConfigured. NO silent runtime fallbacks.
        ApprovalRoutePolicy? policy;
        if (requester.DepartmentId != null)
        {
            policy = await _dbContext.ApprovalRoutePolicies
                .Include(p => p.Rules)
                    .ThenInclude(r => r.Candidates)
                .FirstOrDefaultAsync(p =>
                    p.DepartmentId == requester.DepartmentId &&
                    p.IsActive, cancellationToken);
        }
        else
        {
            policy = await _dbContext.ApprovalRoutePolicies
                .Include(p => p.Rules)
                    .ThenInclude(r => r.Candidates)
                .FirstOrDefaultAsync(p =>
                    p.DepartmentId == null &&
                    p.IsActive, cancellationToken);
        }

        if (policy == null)
        {
            return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
        }

        // 2. Query Active Rule matching requester's position
        var rule = policy.Rules
            .FirstOrDefault(r =>
                r.IsActive &&
                r.RequesterPositionId == requester.PositionId);

        if (rule == null)
        {
            return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
        }

        // Blocker 2: Auto-approve is valid ONLY for company-level policy (policy.DepartmentId == null)
        // and company-level requester (requester.DepartmentId == null) with rule.IsAutoApprove == true.
        if (rule.IsAutoApprove)
        {
            if (policy.DepartmentId == null && requester.DepartmentId == null)
            {
                return ApprovalRouteResolutionResult.AutoApproved(policy.Id, rule.Id);
            }

            // If a department policy has IsAutoApprove = true, auto-approve is invalid and fails validation.
            return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
        }

        var utcNow = DateTime.UtcNow;
        var businessToday = DateOnly.FromDateTime(utcNow);

        // 3. SpecificApprover Override on Rule
        if (rule.SpecificApproverEmployeeId != null)
        {
            var specificApprover = await _dbContext.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Id == rule.SpecificApproverEmployeeId, cancellationToken);

            if (specificApprover != null && await ValidateApproverAsync(specificApprover, requester, cancellationToken))
            {
                return ApprovalRouteResolutionResult.Success(
                    specificApprover,
                    policy.Id,
                    rule.Id,
                    candidateId: null,
                    priorityOrder: 0,
                    levelId: null,
                    levelAssignmentId: null);
            }

            return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
        }

        // 4. Candidate Priority Sequence Evaluation
        var candidates = rule.Candidates
            .Where(c => c.IsActive)
            .OrderBy(c => c.PriorityOrder)
            .ToList();

        // If rule is not configured for AutoApprove and candidate list is empty -> FAIL with ApprovalRouteNotConfigured.
        if (!candidates.Any())
        {
            return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
        }

        foreach (var candidate in candidates)
        {
            if (candidate.ApprovalRouteLevelId != null)
            {
                var activeAssignments = await _dbContext.ApprovalRouteLevelAssignments
                    .Include(a => a.AssignedEmployee)
                    .Where(a =>
                        a.ApprovalRouteLevelId == candidate.ApprovalRouteLevelId &&
                        a.IsActive &&
                        a.EffectiveFrom <= businessToday &&
                        (a.EffectiveTo == null || a.EffectiveTo >= businessToday))
                    .ToListAsync(cancellationToken);

                activeAssignments = activeAssignments
                    .Where(a => a.IsActive && a.IsValidOnDate(businessToday) && (excludedLevelAssignmentId == null || a.Id != excludedLevelAssignmentId))
                    .ToList();

                if (activeAssignments.Count != 1)
                {
                    continue;
                }

                var assignment = activeAssignments[0];
                var candidateApprover = assignment.AssignedEmployee;

                if (candidateApprover != null && await ValidateApproverAsync(candidateApprover, requester, cancellationToken))
                {
                    return ApprovalRouteResolutionResult.Success(
                        candidateApprover,
                        policy.Id,
                        rule.Id,
                        candidate.Id,
                        candidate.PriorityOrder,
                        candidate.ApprovalRouteLevelId,
                        assignment.Id);
                }
            }
        }

        // 5. If all candidates fail validation or lack permission
        return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);
    }

    private async Task<bool> ValidateApproverAsync(
        Employee candidateApprover,
        Employee requester,
        CancellationToken cancellationToken)
    {
        if (candidateApprover == null || !candidateApprover.IsActive)
        {
            return false;
        }

        // Self-approval is strictly forbidden
        if (candidateApprover.Id == requester.Id)
        {
            return false;
        }

        // Approver must have a linked user account
        if (candidateApprover.UserId == null)
        {
            return false;
        }

        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == candidateApprover.UserId, cancellationToken);

        if (user == null || user.IsDeleted == true || user.IdentityId == null)
        {
            return false;
        }

        // Approver must possess the "APPROVE_LEAVE_REQUEST" permission
        var checkPerm = await _roleService.checkRoleExist(user.IdentityId.Value, "APPROVE_LEAVE_REQUEST", cancellationToken);
        if (checkPerm.IsFailure || !checkPerm.Value)
        {
            return false;
        }

        return true;
    }
}
