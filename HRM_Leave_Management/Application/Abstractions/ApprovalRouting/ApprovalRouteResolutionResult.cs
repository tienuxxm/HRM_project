using Domain.ApprovalRouting;
using Domain.Employees;

namespace Application.Abstractions.ApprovalRouting;

public sealed class ApprovalRouteResolutionResult
{
    public bool IsSuccess { get; }
    public bool IsAutoApproved { get; }
    public string? ErrorMessage { get; }
    public Employee? AssignedApprover { get; }
    public ApprovalRoutePolicyId? PolicyId { get; }
    public ApprovalRouteRuleId? RuleId { get; }
    public ApprovalRouteRuleCandidateId? CandidateId { get; }
    public int PriorityOrder { get; }
    public ApprovalRouteLevelId? LevelId { get; }
    public ApprovalRouteLevelAssignmentId? LevelAssignmentId { get; }

    private ApprovalRouteResolutionResult(
        bool isSuccess,
        bool isAutoApproved,
        string? errorMessage,
        Employee? assignedApprover,
        ApprovalRoutePolicyId? policyId,
        ApprovalRouteRuleId? ruleId,
        ApprovalRouteRuleCandidateId? candidateId,
        int priorityOrder,
        ApprovalRouteLevelId? levelId,
        ApprovalRouteLevelAssignmentId? levelAssignmentId)
    {
        IsSuccess = isSuccess;
        IsAutoApproved = isAutoApproved;
        ErrorMessage = errorMessage;
        AssignedApprover = assignedApprover;
        PolicyId = policyId;
        RuleId = ruleId;
        CandidateId = candidateId;
        PriorityOrder = priorityOrder;
        LevelId = levelId;
        LevelAssignmentId = levelAssignmentId;
    }

    public static ApprovalRouteResolutionResult Success(
        Employee assignedApprover,
        ApprovalRoutePolicyId policyId,
        ApprovalRouteRuleId ruleId,
        ApprovalRouteRuleCandidateId? candidateId,
        int priorityOrder,
        ApprovalRouteLevelId? levelId,
        ApprovalRouteLevelAssignmentId? levelAssignmentId)
    {
        return new ApprovalRouteResolutionResult(
            isSuccess: true,
            isAutoApproved: false,
            errorMessage: null,
            assignedApprover,
            policyId,
            ruleId,
            candidateId,
            priorityOrder,
            levelId,
            levelAssignmentId);
    }

    public static ApprovalRouteResolutionResult AutoApproved(
        ApprovalRoutePolicyId? policyId = null,
        ApprovalRouteRuleId? ruleId = null)
    {
        return new ApprovalRouteResolutionResult(
            isSuccess: true,
            isAutoApproved: true,
            errorMessage: null,
            assignedApprover: null,
            policyId,
            ruleId,
            candidateId: null,
            priorityOrder: 0,
            levelId: null,
            levelAssignmentId: null);
    }

    public static ApprovalRouteResolutionResult Failure(string errorMessage)
    {
        return new ApprovalRouteResolutionResult(
            isSuccess: false,
            isAutoApproved: false,
            errorMessage,
            assignedApprover: null,
            policyId: null,
            ruleId: null,
            candidateId: null,
            priorityOrder: 0,
            levelId: null,
            levelAssignmentId: null);
    }
}
