using Domain.Abstractions;

namespace Domain.Positions;

public class Position : Entity<PositionId>
{
    private Position(
        PositionId id,
        string code,
        string name,
        int level,
        bool isActive,
        DateTime createdDate)
    {
        Id = id;
        Code = code;
        Name = name;
        Level = level;
        IsActive = isActive;
        CreatedDate = createdDate;
    }

    private Position()
    {
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public int Level { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public static Position Create(
        string code,
        string name,
        int level)
    {
        return new Position(
            PositionId.New(),
            code,
            name,
            level,
            isActive: true,
            createdDate: DateTime.UtcNow);
    }

    public void Update(string code, string name, int level)
    {
        Code = code;
        Name = name;
        Level = level;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
