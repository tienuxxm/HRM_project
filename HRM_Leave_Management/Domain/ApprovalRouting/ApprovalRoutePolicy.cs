using Domain.Abstractions;
using Domain.Departments;
using Domain.Employees;
using Domain.Positions;

namespace Domain.ApprovalRouting;

public class ApprovalRoutePolicy : Entity<ApprovalRoutePolicyId>
{
    private readonly List<ApprovalRouteLevel> _levels = new();
    private readonly List<ApprovalRouteRule> _rules = new();

    private ApprovalRoutePolicy(
        ApprovalRoutePolicyId id,
        DepartmentId? departmentId,
        string name,
        bool isActive,
        DateTime createdAt)
        : base(id)
    {
        DepartmentId = departmentId;
        Name = name;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    private ApprovalRoutePolicy()
    {
    }

    public DepartmentId? DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<ApprovalRouteLevel> Levels => _levels.AsReadOnly();
    public IReadOnlyCollection<ApprovalRouteRule> Rules => _rules.AsReadOnly();

    public static ApprovalRoutePolicy Create(
        DepartmentId? departmentId,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Policy name cannot be empty.", nameof(name));

        return new ApprovalRoutePolicy(
            ApprovalRoutePolicyId.New(),
            departmentId,
            name.Trim(),
            isActive: true,
            createdAt: DateTime.UtcNow);
    }

    public void Update(string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Policy name cannot be empty.", nameof(name));

        Name = name.Trim();
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public ApprovalRouteLevel AddLevel(string levelName, int levelRank, bool canApproveLeave = true)
    {
        if (_levels.Any(l => l.LevelRank == levelRank && l.IsActive))
        {
            throw new InvalidOperationException($"LevelRank {levelRank} already exists and is active in policy '{Name}'.");
        }

        var level = ApprovalRouteLevel.Create(Id, levelName, levelRank, canApproveLeave);
        _levels.Add(level);
        return level;
    }

    public void UpdateLevel(ApprovalRouteLevelId levelId, string levelName, int levelRank, bool canApproveLeave, bool isActive)
    {
        var level = _levels.FirstOrDefault(l => l.Id == levelId);
        if (level == null)
            throw new InvalidOperationException($"Level with ID {levelId.Value} does not exist in policy '{Name}'.");

        if (isActive && _levels.Any(l => l.Id != levelId && l.LevelRank == levelRank && l.IsActive))
        {
            throw new InvalidOperationException($"LevelRank {levelRank} is already assigned to another active level in policy '{Name}'.");
        }

        level.Update(levelName, levelRank, canApproveLeave, isActive);
    }

    public ApprovalRouteRule AddRule(PositionId requesterPositionId, EmployeeId? specificApproverEmployeeId = null, bool isAutoApprove = false)
    {
        if (requesterPositionId == null)
            throw new ArgumentNullException(nameof(requesterPositionId), "RequesterPositionId is required for approval route rule. Default/fallback rules are not supported.");

        if (_rules.Any(r => r.IsActive && r.RequesterPositionId == requesterPositionId))
        {
            throw new InvalidOperationException($"An active rule for PositionId '{requesterPositionId.Value}' already exists in policy '{Name}'.");
        }

        var rule = ApprovalRouteRule.Create(Id, requesterPositionId, specificApproverEmployeeId, isAutoApprove);
        _rules.Add(rule);
        return rule;
    }

    public ApprovalRouteRuleCandidate AddRuleCandidate(ApprovalRouteRuleId ruleId, ApprovalRouteLevelId levelId, int priorityOrder)
    {
        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule == null)
            throw new InvalidOperationException($"Rule with ID '{ruleId.Value}' does not exist in policy '{Name}'.");

        var level = _levels.FirstOrDefault(l => l.Id == levelId);
        if (level == null)
            throw new InvalidOperationException($"Level with ID '{levelId.Value}' does not exist in policy '{Name}'. Candidates must belong to the same policy.");

        return rule.AddCandidate(levelId, priorityOrder);
    }

    public void UpdateRuleCandidatePriority(ApprovalRouteRuleId ruleId, ApprovalRouteRuleCandidateId candidateId, int priorityOrder)
    {
        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule == null)
            throw new InvalidOperationException($"Rule with ID '{ruleId.Value}' does not exist in policy '{Name}'.");

        rule.UpdateCandidatePriority(candidateId, priorityOrder);
    }
}
