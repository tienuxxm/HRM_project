using Domain.Abstractions;

namespace Domain.LeaveBalances;

public static class LeaveBalanceErrors
{
    public static Error NotFound = new(
        "LeaveBalance.NotFound",
        "The leave balance with the specified identifier was not found");

    public static Error LeaveBalanceExisted = new(
        "LeaveBalance.Existed",
        "A leave balance for this employee, leave type, and year already exists");

    public static Error InvalidYear = new(
        "LeaveBalance.InvalidYear",
        "The leave balance year must be within current year - 1 to current year + 1");

    public static Error EmployeeNotFound = new(
        "LeaveBalance.EmployeeNotFound",
        "The specified employee was not found");

    public static Error LeaveTypeNotFound = new(
        "LeaveBalance.LeaveTypeNotFound",
        "The specified leave type was not found");
}
