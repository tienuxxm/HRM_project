using Domain.Abstractions;
using Domain.Employees;

namespace Domain.ApprovalRouting;

public class ApprovalRouteLevelAssignment : Entity<ApprovalRouteLevelAssignmentId>
{
    private ApprovalRouteLevelAssignment(
        ApprovalRouteLevelAssignmentId id,
        ApprovalRouteLevelId approvalRouteLevelId,
        EmployeeId assignedEmployeeId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isActive,
        string? reason,
        Guid createdByUserId,
        DateTime createdAt)
        : base(id)
    {
        ApprovalRouteLevelId = approvalRouteLevelId;
        AssignedEmployeeId = assignedEmployeeId;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        IsActive = isActive;
        Reason = reason;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    private ApprovalRouteLevelAssignment()
    {
    }

    public ApprovalRouteLevelId ApprovalRouteLevelId { get; private set; } = null!;
    public ApprovalRouteLevel? ApprovalRouteLevel { get; private set; }
    public EmployeeId AssignedEmployeeId { get; private set; } = null!;
    public Employee? AssignedEmployee { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public string? Reason { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ApprovalRouteLevelAssignment Create(
        ApprovalRouteLevelId levelId,
        EmployeeId employeeId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid createdByUserId,
        string? reason = null)
    {
        if (levelId == null)
            throw new ArgumentNullException(nameof(levelId));

        if (employeeId == null)
            throw new ArgumentNullException(nameof(employeeId));

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new ArgumentException("EffectiveTo date cannot be earlier than EffectiveFrom date.", nameof(effectiveTo));

        return new ApprovalRouteLevelAssignment(
            ApprovalRouteLevelAssignmentId.New(),
            levelId,
            employeeId,
            effectiveFrom,
            effectiveTo,
            isActive: true,
            reason,
            createdByUserId,
            createdAt: DateTime.UtcNow);
    }

    public bool IsValidOnDate(DateOnly targetDate)
    {
        if (!IsActive) return false;
        if (targetDate < EffectiveFrom) return false;
        if (EffectiveTo.HasValue && targetDate > EffectiveTo.Value) return false;
        return true;
    }

    public bool OverlapsWith(DateOnly from, DateOnly? to)
    {
        if (!IsActive) return false;
        var endA = EffectiveTo ?? DateOnly.MaxValue;
        var endB = to ?? DateOnly.MaxValue;
        return EffectiveFrom <= endB && from <= endA;
    }

    public void Deactivate(string? deactivationReason = null)
    {
        Deactivate(DateOnly.FromDateTime(DateTime.UtcNow), deactivationReason);
    }

    public void Deactivate(DateOnly effectiveTo, string? deactivationReason = null)
    {
        EffectiveTo = effectiveTo;
        IsActive = false;
        if (!string.IsNullOrWhiteSpace(deactivationReason))
        {
            Reason = string.IsNullOrWhiteSpace(Reason)
                ? deactivationReason
                : $"{Reason} | Deactivated: {deactivationReason}";
        }
    }
}
