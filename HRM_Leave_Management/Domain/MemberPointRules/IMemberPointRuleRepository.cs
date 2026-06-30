namespace Domain.MemberPointRules;

public interface IMemberPointRuleRepository
{
    
    IQueryable<MemberPointRule> GetEntitiesAsQueryable();
    void Update(MemberPointRule memberPointRule);
    
    Task<MemberPointRule?> GetByIdAsync(MemberPointRuleId id, CancellationToken cancellationToken = default);
    
}