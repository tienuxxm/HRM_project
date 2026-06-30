using Domain.Abstractions;

namespace Domain.Departments;

public static class DepartmentErrors
{
    public static Error NotFound = new(
        "Department.NotFound",
        "The department with the specified identifier was not found");

    public static Error DepartmentExisted = new(
        "Department.Existed",
        "A department with the same code already exists");

    public static Error HasChildren = new(
        "Department.HasChildren",
        "Cannot delete department that has child departments");
}
