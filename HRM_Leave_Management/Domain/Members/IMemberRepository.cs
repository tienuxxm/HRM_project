using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Members;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken = default);
    Task<Member?> GetByIdentityAsync(string id, CancellationToken cancellationToken = default);
    void Add(Member user);
    Task<Member?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

    Task<bool> IsExistedAsync(Expression<Func<Member, bool>> expression,
        CancellationToken cancellationToken = default);

    IQueryable<Member> GetEntitiesAsQueryable();

    Task<List<Member>?> GetAll(
        CancellationToken cancellationToken = default);

    Task<List<Member>?> Search(string searchValue, CancellationToken cancellationToken);
    void Update(Member member);

    Task<Member?> GetLatestByProperty(Expression<Func<Member, dynamic>> expression,
        CancellationToken cancellationToken = default);

    Task<PagedList<Member>> GetAllPaged(PagedQuery<Member, MemberId> request,
        IQueryable<Member>? queryable = null);

    Task<string?> GetIdentityById(MemberId id, CancellationToken cancellationToken = default);

    Task<bool> IsEmailExisted(Email email, MemberId? memberId, CancellationToken cancellationToken = default);

    Task<bool> IsPhoneExisted(PhoneNumber phoneNumber, MemberId? memberId,
        CancellationToken cancellationToken = default);

    Task<string> GetNextMemberCode(CancellationToken cancellationToken = default);


    void UpdateRange(List<Member> members);
}