using Domain.MemberPointHistories;

namespace Application.MemberPointHistories.Response;

public class MemberPointHistoryResponse
{
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Point { get; set; }
    public PointType PointType { get; set; }
}