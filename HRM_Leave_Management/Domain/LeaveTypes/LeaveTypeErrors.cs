using Domain.Abstractions;

namespace Domain.LeaveTypes;

public static class LeaveTypeErrors
{
    public static Error NotFound = new(
        "LeaveType.NotFound",
        "The leave type with the specified identifier was not found");

    public static Error LeaveTypeExisted = new(
        "LeaveType.Existed",
        "A leave type with the same code already exists");

    public static Error HasLeaveRequests = new(
        "LeaveType.HasLeaveRequests",
        "Cannot delete leave type that has associated leave requests");
}
