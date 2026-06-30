using Domain.Abstractions;
using Domain.Members;
using Domain.Shared;

namespace Domain.MemberPointHistories;

public class MemberPointHistory : Entity<MemberPointHistoryId>
{
    public MemberId MemberId { get; private set; }
    public MemberPoint MemberPoint { get; private set; }
    public PointType PointType { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Title Title { get; private set; }


    private MemberPointHistory(MemberPointHistoryId id, MemberId memberId, MemberPoint memberPoint, PointType pointType,
        Title title, DateTime createdDate) : base(id)
    {
        MemberId = memberId;
        MemberPoint = memberPoint;
        PointType = pointType;
        CreatedDate = createdDate;
        Title = title;
    }

    public static MemberPointHistory Create(MemberId memberId, MemberPoint memberPoint, PointType pointType,
        Title title, DateTime createdDate)
    {
        var memberPointHistory = new MemberPointHistory(MemberPointHistoryId.New(), memberId, memberPoint, pointType,
            title, createdDate);
        return memberPointHistory;
    }
}