using Domain.Abstractions;

namespace Domain.LeaveTypes;

public class LeaveType : Entity<LeaveTypeId>
{
    private LeaveType(
        LeaveTypeId id,
        string name,
        string code,
        int defaultDays,
        string? description,
        bool isActive,
        DateTime createdDate)
    {
        Id = id;
        Name = name;
        Code = code;
        DefaultDays = defaultDays;
        Description = description;
        IsActive = isActive;
        CreatedDate = createdDate;
    }

    private LeaveType()
    {
        Name = null!;
        Code = null!;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public int DefaultDays { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public static LeaveType Create(
        string name,
        string code,
        int defaultDays,
        string? description)
    {
        return new LeaveType(
            LeaveTypeId.New(),
            name,
            code,
            defaultDays,
            description,
            isActive: true,
            createdDate: DateTime.UtcNow);
    }

    public void Update(string name, string code, int defaultDays, string? description)
    {
        Name = name;
        Code = code;
        DefaultDays = defaultDays;
        Description = description;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
