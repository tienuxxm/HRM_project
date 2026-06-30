namespace Application.LeaveBalances;

public sealed class LeaveBalanceResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string EmployeeCode { get; set; } = null!;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = null!;
    public string LeaveTypeCode { get; set; } = null!;
    public int Year { get; set; }
    public decimal AllocatedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays => AllocatedDays - UsedDays;
    public decimal PendingDays { get; set; }
    public decimal AvailableDays => AllocatedDays - UsedDays - PendingDays;
}
