using Domain.Abstractions;

namespace Domain.Employees;

public static class EmployeeErrors
{
    public static Error NotFound = new(
        "Employee.NotFound",
        "The employee with the specified identifier was not found");

    public static Error EmployeeCodeExisted = new(
        "Employee.CodeExisted",
        "An employee with the same code already exists");

    public static Error HasSubordinates = new(
        "Employee.HasSubordinates",
        "Cannot delete employee that has subordinates");

    public static Error AlreadyLinkedToUser = new(
        "Employee.AlreadyLinkedToUser",
        "Employee already has a linked user account");

    public static Error HasLeaveBalances = new(
        "Employee.HasLeaveBalances",
        "Cannot delete employee that has active leave balances");

    public static Error HasLeaveRequests = new(
        "Employee.HasLeaveRequests",
        "Cannot delete employee that has leave requests");

    public static Error HasApproverAssignments = new(
        "Employee.HasApproverAssignments",
        "Cannot delete employee that has active leave approver assignments");

    public static Error HasRecalculationAudits = new(
        "Employee.HasRecalculationAudits",
        "Cannot delete employee that has leave request recalculation audits");

    public static Error HasActiveSubordinates = new(
        "Employee.HasActiveSubordinates",
        "Cannot deactivate this employee because active subordinates are still assigned. Reassign them before deleting the employee.");

    public static Error KeycloakRevokeFailed = new(
        "Employee.KeycloakRevokeFailed",
        "Employee was updated, but login access could not be revoked. Please retry or contact an administrator.");
}

