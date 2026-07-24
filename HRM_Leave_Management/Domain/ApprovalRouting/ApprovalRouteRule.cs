using Domain.Abstractions;
using Domain.Employees;
using Domain.Positions;

namespace Domain.ApprovalRouting;

public class ApprovalRouteRule : Entity<ApprovalRouteRuleId>
{
    private readonly List<ApprovalRouteRuleCandidate> _candidates = new();

    private ApprovalRouteRule(
        ApprovalRouteRuleId id,
        ApprovalRoutePolicyId policyId,
        PositionId requesterPositionId,
        EmployeeId? specificApproverEmployeeId,
        bool isAutoApprove,
        bool isActive)
        : base(id)
    {
        PolicyId = policyId;
        RequesterPositionId = requesterPositionId;
        SpecificApproverEmployeeId = specificApproverEmployeeId;
        IsAutoApprove = isAutoApprove;
        IsActive = isActive;
    }

    private ApprovalRouteRule()
    {
    }

    public ApprovalRoutePolicyId PolicyId { get; private set; } = null!;
    public ApprovalRoutePolicy? Policy { get; private set; }
    public PositionId RequesterPositionId { get; private set; } = null!;
    public Position? RequesterPosition { get; private set; }
    public EmployeeId? SpecificApproverEmployeeId { get; private set; }
    public Employee? SpecificApprover { get; private set; }
    public bool IsAutoApprove { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ApprovalRouteRuleCandidate> Candidates => _candidates.AsReadOnly();

    public static ApprovalRouteRule Create(
        ApprovalRoutePolicyId policyId,
        PositionId requesterPositionId,
        EmployeeId? specificApproverEmployeeId = null,
        bool isAutoApprove = false)
    {
        if (policyId == null)
            throw new ArgumentNullException(nameof(policyId));

        if (requesterPositionId == null)
            throw new ArgumentNullException(nameof(requesterPositionId), "RequesterPositionId is required for approval route rule. Default/fallback rules are not supported.");

        if (isAutoApprove && specificApproverEmployeeId != null)
            throw new InvalidOperationException("Auto-approve rule cannot have a specific approver.");

        return new ApprovalRouteRule(
            ApprovalRouteRuleId.New(),
            policyId,
            requesterPositionId,
            specificApproverEmployeeId,
            isAutoApprove,
            isActive: true);
    }

    internal ApprovalRouteRuleCandidate AddCandidate(ApprovalRouteLevelId levelId, int priorityOrder)
    {
        if (IsAutoApprove)
        {
            throw new InvalidOperationException("Cannot add candidates to an auto-approve rule.");
        }

        if (_candidates.Any(c => c.PriorityOrder == priorityOrder && c.IsActive))
        {
            throw new InvalidOperationException($"PriorityOrder {priorityOrder} is already assigned to another active candidate in this rule.");
        }

        if (_candidates.Any(c => c.ApprovalRouteLevelId == levelId && c.IsActive))
        {
            throw new InvalidOperationException($"LevelId {levelId.Value} is already added as a candidate in this rule.");
        }

        var candidate = ApprovalRouteRuleCandidate.Create(Id, levelId, priorityOrder);
        _candidates.Add(candidate);
        return candidate;
    }

    internal void UpdateCandidatePriority(ApprovalRouteRuleCandidateId candidateId, int priorityOrder)
    {
        var candidate = _candidates.FirstOrDefault(c => c.Id == candidateId);
        if (candidate == null)
            throw new InvalidOperationException($"Candidate with ID {candidateId.Value} does not exist in this rule.");

        if (_candidates.Any(c => c.Id != candidateId && c.PriorityOrder == priorityOrder && c.IsActive))
        {
            throw new InvalidOperationException($"PriorityOrder {priorityOrder} is already assigned to another candidate in this rule.");
        }

        candidate.UpdatePriority(priorityOrder);
    }

    public void SetSpecificApprover(EmployeeId? specificApproverEmployeeId)
    {
        if (IsAutoApprove && specificApproverEmployeeId != null)
        {
            throw new InvalidOperationException("Cannot assign a specific approver to an auto-approve rule.");
        }

        SpecificApproverEmployeeId = specificApproverEmployeeId;
    }

    public void SetAutoApprove(bool isAutoApprove)
    {
        if (isAutoApprove && (SpecificApproverEmployeeId != null || _candidates.Any()))
        {
            throw new InvalidOperationException("Cannot set auto-approve on a rule that has candidates or a specific approver.");
        }

        IsAutoApprove = isAutoApprove;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
