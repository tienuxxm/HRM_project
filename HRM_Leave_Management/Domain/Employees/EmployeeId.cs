namespace Domain.Employees;

public record EmployeeId(Guid Value)
{
    public static EmployeeId New() => new(Guid.NewGuid());
}
