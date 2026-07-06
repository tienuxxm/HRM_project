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
}
