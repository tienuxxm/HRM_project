namespace Application.LeaveRequests.Get;

public sealed class LeaveRequestResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string EmployeeCode { get; set; } = null!;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StartDayPart { get; set; } = null!;
    public string EndDayPart { get; set; } = null!;
    public decimal Duration { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
    public string? Comment { get; set; }
}
