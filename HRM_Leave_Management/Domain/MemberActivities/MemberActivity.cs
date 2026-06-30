using Domain.Abstractions;
using Domain.Shared;

namespace Domain.MemberActivities;

public class MemberActivity : Entity<MemberActivityId>
{
    private MemberActivity()
    {
    }

    private MemberActivity(MemberActivityId id, MemberActivityType type, LogMessage message) : base(id)
    {
        Message = message;
        Type = type;
        CreatedDate = DateTime.UtcNow;
    }

    public MemberActivityType Type { get; private set; }
    public LogMessage Message { get; private set; }
    public DateTime? CreatedDate { get; private set; }

    public static MemberActivity Create(MemberActivityType type, LogMessage message)
    {
        return new MemberActivity(MemberActivityId.New, type, message);
    }
}