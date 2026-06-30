using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveTypes;

namespace Domain.LeaveBalances;

public class LeaveBalance : Entity<LeaveBalanceId>
{
    private LeaveBalance(
        LeaveBalanceId id,
        EmployeeId employeeId,
        LeaveTypeId leaveTypeId,
        int year,
        decimal allocatedDays,
        decimal usedDays,
        bool isActive,
        DateTime createdDate)
    {
        Id = id;
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        Year = year;
        AllocatedDays = allocatedDays;
        UsedDays = usedDays;
        IsActive = isActive;
        CreatedDate = createdDate;
    }

    private LeaveBalance()
    {
        // EF Core constructor
    }

    public EmployeeId EmployeeId { get; private set; } = null!;
    public Employee Employee { get; private set; } = null!;
    public LeaveTypeId LeaveTypeId { get; private set; } = null!;
    public LeaveType LeaveType { get; private set; } = null!;
    public int Year { get; private set; }
    public decimal AllocatedDays { get; private set; }
    public decimal UsedDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public decimal RemainingDays => AllocatedDays - UsedDays;

    public static LeaveBalance Create(
        EmployeeId employeeId,
        LeaveTypeId leaveTypeId,
        int year,
        decimal allocatedDays,
        decimal usedDays = 0)
    {
        return new LeaveBalance(
            LeaveBalanceId.New(),
            employeeId,
            leaveTypeId,
            year,
            allocatedDays,
            usedDays,
            isActive: true,
            createdDate: DateTime.UtcNow);
    }

    public void Update(decimal allocatedDays, decimal usedDays)
    {
        AllocatedDays = allocatedDays;
        UsedDays = usedDays;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
