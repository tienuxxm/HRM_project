using System.Linq.Expressions;

namespace Domain.MembershipClasses;

public interface IMembershipClassRepository
{
    void Add(MembershipClass membershipClass);
    void AddRange(List<MembershipClass> membershipClasses);
    bool HasData();
    void Remove(MembershipClass membershipClass);

    Task<MembershipClass?> GetByIdAsync(MembershipClassId id,
        CancellationToken cancellationToken = default);

    Task<List<MembershipClass>?> GetAll(CancellationToken cancellationToken = default);

    Task<bool> IsExistedAsync(Expression<Func<MembershipClass, bool>> expression,
        CancellationToken cancellationToken = default);

    IQueryable<MembershipClass> GetEntitiesAsQueryable();

    void Update(MembershipClass entity);
    Task<MembershipClass?> GetLowestMembershipClass(CancellationToken cancellationToken = default);
}