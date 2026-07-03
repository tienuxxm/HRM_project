using Domain.Abstractions;
using Domain.Employees;
using Domain.Departments;
using Domain.Positions;

namespace Domain.LeaveApproverAssignments;

public class LeaveApproverAssignment : Entity<LeaveApproverAssignmentId>
{
    private LeaveApproverAssignment(
        LeaveApproverAssignmentId id,
        EmployeeId approverEmployeeId,
        DepartmentId? targetDepartmentId,
        PositionId? targetPositionId,
        bool isActive,
        DateOnly? effectiveFrom,
        DateOnly? effectiveTo,
        DateTime createdDate)
    {
        Id = id;
        ApproverEmployeeId = approverEmployeeId;
        TargetDepartmentId = targetDepartmentId;
        TargetPositionId = targetPositionId;
        IsActive = isActive;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        CreatedDate = createdDate;
    }

    private LeaveApproverAssignment()
    {
    }

    public EmployeeId ApproverEmployeeId { get; private set; }
    public Employee? Approver { get; private set; }
    
    public DepartmentId? TargetDepartmentId { get; private set; }
    public Department? TargetDepartment { get; private set; }
    
    public PositionId? TargetPositionId { get; private set; }
    public Position? TargetPosition { get; private set; }

    public bool IsActive { get; private set; }
    public DateOnly? EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public static LeaveApproverAssignment Create(
        EmployeeId approverEmployeeId,
        DepartmentId? targetDepartmentId,
        PositionId? targetPositionId,
        DateOnly? effectiveFrom,
        DateOnly? effectiveTo)
    {
        return new LeaveApproverAssignment(
            LeaveApproverAssignmentId.New(),
            approverEmployeeId,
            targetDepartmentId,
            targetPositionId,
            isActive: true,
            effectiveFrom,
            effectiveTo,
            createdDate: DateTime.UtcNow);
    }

    public void Update(
        EmployeeId approverEmployeeId,
        DepartmentId? targetDepartmentId,
        PositionId? targetPositionId,
        DateOnly? effectiveFrom,
        DateOnly? effectiveTo)
    {
        ApproverEmployeeId = approverEmployeeId;
        TargetDepartmentId = targetDepartmentId;
        TargetPositionId = targetPositionId;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
