using Domain.Abstractions;

namespace Domain.ApprovalRouting;

public class ApprovalRouteRuleCandidate : Entity<ApprovalRouteRuleCandidateId>
{
    private ApprovalRouteRuleCandidate(
        ApprovalRouteRuleCandidateId id,
        ApprovalRouteRuleId approvalRouteRuleId,
        ApprovalRouteLevelId approvalRouteLevelId,
        int priorityOrder,
        bool isActive)
        : base(id)
    {
        ApprovalRouteRuleId = approvalRouteRuleId;
        ApprovalRouteLevelId = approvalRouteLevelId;
        PriorityOrder = priorityOrder;
        IsActive = isActive;
    }

    private ApprovalRouteRuleCandidate()
    {
    }

    public ApprovalRouteRuleId ApprovalRouteRuleId { get; private set; } = null!;
    public ApprovalRouteRule? ApprovalRouteRule { get; private set; }
    public ApprovalRouteLevelId ApprovalRouteLevelId { get; private set; } = null!;
    public ApprovalRouteLevel? ApprovalRouteLevel { get; private set; }
    public int PriorityOrder { get; private set; }
    public bool IsActive { get; private set; }

    public static ApprovalRouteRuleCandidate Create(
        ApprovalRouteRuleId ruleId,
        ApprovalRouteLevelId levelId,
        int priorityOrder)
    {
        if (ruleId == null)
            throw new ArgumentNullException(nameof(ruleId));

        if (levelId == null)
            throw new ArgumentNullException(nameof(levelId));

        if (priorityOrder < 1)
            throw new ArgumentOutOfRangeException(nameof(priorityOrder), "PriorityOrder must be >= 1.");

        return new ApprovalRouteRuleCandidate(
            ApprovalRouteRuleCandidateId.New(),
            ruleId,
            levelId,
            priorityOrder,
            isActive: true);
    }

    public void UpdatePriority(int priorityOrder)
    {
        if (priorityOrder < 1)
            throw new ArgumentOutOfRangeException(nameof(priorityOrder), "PriorityOrder must be >= 1.");

        PriorityOrder = priorityOrder;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
