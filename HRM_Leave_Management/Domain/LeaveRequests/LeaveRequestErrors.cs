using Domain.Abstractions;

namespace Domain.LeaveRequests;

public static class LeaveRequestErrors
{
    public static Error NotFound = new(
        "LeaveRequest.NotFound",
        "The leave request with the specified identifier was not found");

    public static Error EmployeeNotFound = new(
        "LeaveRequest.EmployeeNotFound",
        "The employee associated with the current user was not found");

    public static Error LeaveTypeNotFound = new(
        "LeaveRequest.LeaveTypeNotFound",
        "The specified leave type was not found or is inactive");

    public static Error NoLeaveBalance = new(
        "LeaveRequest.NoLeaveBalance",
        "No leave balance allocated for this employee, leave type, and year");

    public static Error DateOrderInvalid = new(
        "LeaveRequest.DateOrderInvalid",
        "The start date must be before or equal to the end date");

    public static Error DayPartMismatch = new(
        "LeaveRequest.DayPartMismatch",
        "Invalid session selection for a single-day request");

    public static Error PastDateNotAllowed = new(
        "LeaveRequest.PastDateNotAllowed",
        "Creating a leave request in the past is not allowed");

    public static Error InsufficientBalance = new(
        "LeaveRequest.InsufficientBalance",
        "Insufficient available leave days for this request");

    public static Error OverlapDetected = new(
        "LeaveRequest.OverlapDetected",
        "This request overlaps with an existing pending or approved leave request");

    public static Error DurationZeroOrNegative = new(
        "LeaveRequest.DurationZeroOrNegative",
        "Calculated leave duration must be greater than zero");

    public static Error CrossYearNotAllowed = new(
        "LeaveRequest.CrossYearNotAllowed",
        "A leave request cannot cross multiple calendar years");
}
