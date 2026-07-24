using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveRequests;

namespace Domain.ApprovalRouting;

public class LeaveRequestApprovalAssignment : Entity<LeaveRequestApprovalAssignmentId>
{
    private LeaveRequestApprovalAssignment(
        LeaveRequestApprovalAssignmentId id,
        LeaveRequestId leaveRequestId,
        EmployeeId? assignedApproverEmployeeId,
        ApprovalAssignmentStatus assignmentStatus,
        ApprovalAssignmentReason assignmentReason,
        ApprovalRoutePolicyId? snapshotPolicyId,
        ApprovalRouteRuleId? snapshotRuleId,
        ApprovalRouteRuleCandidateId? snapshotCandidateId,
        ApprovalRouteLevelAssignmentId? snapshotLevelAssignmentId,
        DateTime assignedAt)
        : base(id)
    {
        LeaveRequestId = leaveRequestId;
        AssignedApproverEmployeeId = assignedApproverEmployeeId;
        AssignmentStatus = assignmentStatus;
        AssignmentReason = assignmentReason;
        SnapshotPolicyId = snapshotPolicyId;
        SnapshotRuleId = snapshotRuleId;
        SnapshotCandidateId = snapshotCandidateId;
        SnapshotLevelAssignmentId = snapshotLevelAssignmentId;
        AssignedAt = assignedAt;
    }

    private LeaveRequestApprovalAssignment()
    {
    }

    public LeaveRequestId LeaveRequestId { get; private set; } = null!;
    public LeaveRequest? LeaveRequest { get; private set; }
    public EmployeeId? AssignedApproverEmployeeId { get; private set; }
    public Employee? AssignedApprover { get; private set; }
    public ApprovalAssignmentStatus AssignmentStatus { get; private set; }
    public ApprovalAssignmentReason AssignmentReason { get; private set; }

    // Metadata Snapshot for Audit
    public ApprovalRoutePolicyId? SnapshotPolicyId { get; private set; }
    public ApprovalRouteRuleId? SnapshotRuleId { get; private set; }
    public ApprovalRouteRuleCandidateId? SnapshotCandidateId { get; private set; }
    public ApprovalRouteLevelAssignmentId? SnapshotLevelAssignmentId { get; private set; }

    public DateTime AssignedAt { get; private set; }

    public static LeaveRequestApprovalAssignment CreateAssigned(
        LeaveRequestId leaveRequestId,
        EmployeeId approverEmployeeId,
        ApprovalAssignmentReason reason,
        ApprovalRoutePolicyId? policyId = null,
        ApprovalRouteRuleId? ruleId = null,
        ApprovalRouteRuleCandidateId? candidateId = null,
        ApprovalRouteLevelAssignmentId? levelAssignmentId = null)
    {
        if (leaveRequestId == null)
            throw new ArgumentNullException(nameof(leaveRequestId));

        if (approverEmployeeId == null)
            throw new ArgumentNullException(nameof(approverEmployeeId));

        return new LeaveRequestApprovalAssignment(
            LeaveRequestApprovalAssignmentId.New(),
            leaveRequestId,
            approverEmployeeId,
            ApprovalAssignmentStatus.Assigned,
            reason,
            policyId,
            ruleId,
            candidateId,
            levelAssignmentId,
            assignedAt: DateTime.UtcNow);
    }

    public static LeaveRequestApprovalAssignment CreateNeedsAttention(
        LeaveRequestId leaveRequestId,
        ApprovalAssignmentReason reason,
        ApprovalRoutePolicyId? policyId = null)
    {
        if (leaveRequestId == null)
            throw new ArgumentNullException(nameof(leaveRequestId));

        return new LeaveRequestApprovalAssignment(
            LeaveRequestApprovalAssignmentId.New(),
            leaveRequestId,
            assignedApproverEmployeeId: null,
            ApprovalAssignmentStatus.NeedsAdminAttention,
            reason,
            policyId,
            snapshotRuleId: null,
            snapshotCandidateId: null,
            snapshotLevelAssignmentId: null,
            assignedAt: DateTime.UtcNow);
    }

    public void Reassign(
        EmployeeId newApproverEmployeeId,
        ApprovalAssignmentReason reason,
        ApprovalRoutePolicyId? policyId = null,
        ApprovalRouteRuleId? ruleId = null,
        ApprovalRouteRuleCandidateId? candidateId = null,
        ApprovalRouteLevelAssignmentId? levelAssignmentId = null)
    {
        if (newApproverEmployeeId == null)
            throw new ArgumentNullException(nameof(newApproverEmployeeId));

        AssignedApproverEmployeeId = newApproverEmployeeId;
        AssignmentStatus = ApprovalAssignmentStatus.Assigned;
        AssignmentReason = reason;
        SnapshotPolicyId = policyId ?? SnapshotPolicyId;
        SnapshotRuleId = ruleId ?? SnapshotRuleId;
        SnapshotCandidateId = candidateId ?? SnapshotCandidateId;
        SnapshotLevelAssignmentId = levelAssignmentId ?? SnapshotLevelAssignmentId;
        AssignedAt = DateTime.UtcNow;
    }

    public void MarkNeedsAttention(ApprovalAssignmentReason reason)
    {
        AssignedApproverEmployeeId = null;
        AssignmentStatus = ApprovalAssignmentStatus.NeedsAdminAttention;
        AssignmentReason = reason;
        AssignedAt = DateTime.UtcNow;
    }
}
