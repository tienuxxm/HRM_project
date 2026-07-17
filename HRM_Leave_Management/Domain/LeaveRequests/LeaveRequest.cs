using Domain.Abstractions;
using Domain.Employees;
using Domain.LeaveTypes;

namespace Domain.LeaveRequests;

public class LeaveRequest : Entity<LeaveRequestId>
{
    private LeaveRequest(
        LeaveRequestId id,
        EmployeeId employeeId,
        LeaveTypeId leaveTypeId,
        DateOnly startDate,
        DateOnly endDate,
        LeaveDayPart startDayPart,
        LeaveDayPart endDayPart,
        decimal duration,
        string reason,
        LeaveRequestStatus status,
        DateTime createdAt)
    {
        Id = id;
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        StartDate = startDate;
        EndDate = endDate;
        StartDayPart = startDayPart;
        EndDayPart = endDayPart;
        Duration = duration;
        Reason = reason;
        Status = status;
        CreatedAt = createdAt;
    }

    private LeaveRequest()
    {
        // EF Core constructor
    }

    public EmployeeId EmployeeId { get; private set; } = null!;
    public Employee Employee { get; private set; } = null!;
    public LeaveTypeId LeaveTypeId { get; private set; } = null!;
    public LeaveType LeaveType { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public LeaveDayPart StartDayPart { get; private set; }
    public LeaveDayPart EndDayPart { get; private set; }
    public decimal Duration { get; private set; }
    public string Reason { get; private set; } = null!;
    public LeaveRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public Guid? ProcessedBy { get; private set; }
    public string? Comment { get; private set; }

    public static LeaveRequest Create(
        EmployeeId employeeId,
        LeaveTypeId leaveTypeId,
        DateOnly startDate,
        DateOnly endDate,
        LeaveDayPart startDayPart,
        LeaveDayPart endDayPart,
        decimal duration,
        string reason,
        DateTime createdAt)
    {
        return new LeaveRequest(
            LeaveRequestId.New(),
            employeeId,
            leaveTypeId,
            startDate,
            endDate,
            startDayPart,
            endDayPart,
            duration,
            reason,
            LeaveRequestStatus.Pending,
            createdAt);
    }

    public void Cancel(DateTime processedAt)
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending leave requests can be canceled.");
        }

        Status = LeaveRequestStatus.Canceled;
        ProcessedAt = processedAt;
    }

    public void Approve(Guid approvedBy, DateTime processedAt, string? comment = null)
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending leave requests can be approved.");
        }

        Status = LeaveRequestStatus.Approved;
        ProcessedBy = approvedBy;
        ProcessedAt = processedAt;
        Comment = comment;
    }

    public void Reject(Guid rejectedBy, DateTime processedAt, string? comment = null)
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending leave requests can be rejected.");
        }

        Status = LeaveRequestStatus.Rejected;
        ProcessedBy = rejectedBy;
        ProcessedAt = processedAt;
        Comment = comment;
    }

    public void SetApprovedForCeo(DateTime utcNow)
    {
        Status = LeaveRequestStatus.Approved;
        ProcessedBy = null;
        ProcessedAt = utcNow;
        Comment = "Auto approved for CEO";
    }

    public void UpdateDurationOnly(decimal newDuration)
    {
        Duration = newDuration;
    }

    public void ReopenToPending(decimal newDuration)
    {
        Status = LeaveRequestStatus.Pending;
        Duration = newDuration;
        ProcessedBy = null;
        ProcessedAt = null;
    }
}

