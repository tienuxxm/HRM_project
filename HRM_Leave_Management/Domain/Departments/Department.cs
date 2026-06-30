using Domain.Abstractions;

namespace Domain.Departments;

public class Department : Entity<DepartmentId>
{
    private Department(
        DepartmentId id,
        string name,
        string code,
        string? description,
        DepartmentId? parentDepartmentId,
        bool isActive,
        DateTime createdDate)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
        ParentDepartmentId = parentDepartmentId;
        IsActive = isActive;
        CreatedDate = createdDate;
    }

    private Department()
    {
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public DepartmentId? ParentDepartmentId { get; private set; }
    public Department? ParentDepartment { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public static Department Create(
        string name,
        string code,
        string? description,
        DepartmentId? parentDepartmentId)
    {
        return new Department(
            DepartmentId.New(),
            name,
            code,
            description,
            parentDepartmentId,
            isActive: true,
            createdDate: DateTime.UtcNow);
    }

    public void Update(string name, string code, string? description, DepartmentId? parentDepartmentId)
    {
        Name = name;
        Code = code;
        Description = description;
        ParentDepartmentId = parentDepartmentId;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
