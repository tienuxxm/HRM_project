using Domain.Abstractions;
using Domain.Departments;
using Domain.Positions;
using Domain.Users;

namespace Domain.Employees;

public class Employee : Entity<EmployeeId>
{
    private Employee(
        EmployeeId id,
        string fullName,
        string employeeCode,
        DepartmentId? departmentId,
        UserId? userId,
        PositionId? positionId,
        DateTime joinDate,
        EmployeeId? managerId,
        bool isActive,
        DateTime createdDate)
    {
        Id = id;
        FullName = fullName;
        EmployeeCode = employeeCode;
        DepartmentId = departmentId;
        UserId = userId;
        PositionId = positionId;
        JoinDate = joinDate;
        ManagerId = managerId;
        IsActive = isActive;
        CreatedDate = createdDate;
    }

    private Employee()
    {
    }

    public string FullName { get; private set; }
    public string EmployeeCode { get; private set; }
    public DepartmentId? DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public UserId? UserId { get; private set; }
    public User? User { get; private set; }
    public PositionId? PositionId { get; private set; }
    public Position? Position { get; private set; }
    public DateTime JoinDate { get; private set; }
    public EmployeeId? ManagerId { get; private set; }
    public Employee? Manager { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public static Employee Create(
        string fullName,
        string employeeCode,
        DepartmentId? departmentId,
        UserId? userId,
        PositionId? positionId,
        DateTime joinDate,
        EmployeeId? managerId)
    {
        return new Employee(
            EmployeeId.New(),
            fullName,
            employeeCode,
            departmentId,
            userId,
            positionId,
            joinDate,
            managerId,
            isActive: true,
            createdDate: DateTime.UtcNow);
    }

    public void Update(
        string fullName,
        string employeeCode,
        DepartmentId? departmentId,
        PositionId? positionId,
        DateTime joinDate,
        EmployeeId? managerId)
    {
        FullName = fullName;
        EmployeeCode = employeeCode;
        DepartmentId = departmentId;
        PositionId = positionId;
        JoinDate = joinDate;
        ManagerId = managerId;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public Result LinkUser(UserId userId)
    {
        if (UserId is not null)
        {
            return Result.Failure(EmployeeErrors.AlreadyLinkedToUser);
        }

        UserId = userId;
        return Result.Success();
    }
}

