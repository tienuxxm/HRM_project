using Domain.Abstractions;

namespace Domain.LeaveApproverAssignments;

public static class LeaveApproverAssignmentErrors
{
    public static Error NotFound = new(
        "LeaveApproverAssignment.NotFound",
        "The leave approver assignment with the specified identifier was not found");

    public static Error DuplicateAssignment = new(
        "LeaveApproverAssignment.Duplicate",
        "An active assignment with the same approver, target department, and target position already exists");
}
