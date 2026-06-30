namespace Domain.MemberActivities;

public interface IMemberActivityRepository
{
    void Add(MemberActivity memberActivity);
    void AddRange(List<MemberActivity> memberActivities);
}