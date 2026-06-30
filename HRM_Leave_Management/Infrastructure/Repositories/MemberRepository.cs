using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberRepository : Repository<Member, MemberId>, IMemberRepository
{
    public MemberRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Member?> GetByIdentityAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Member>()
            .FirstOrDefaultAsync(x => x.IdentityId == id && x.IsActive, cancellationToken);
    }

    public async Task<Member?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await DbContext.Set<Member>()
                .FirstOrDefaultAsync(m => m.IsActive && m.PhoneNumber.Equals(new PhoneNumber(phoneNumber)),
                    cancellationToken);
            return member;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Member>?> Search(string searchValue, CancellationToken cancellationToken)
    {
        var members = DbContext.Set<Member>()
            .AsEnumerable()
            .Where(x => x.PhoneNumber.Value.Contains(searchValue) ||
                        x.FullName.ToUpper().Contains(searchValue.ToUpper()))
            .ToList();
        return members;
    }

    public async Task<string?> GetIdentityById(MemberId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Member>().Where(x => x.Id.Equals(id)).Select(x => x.IdentityId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsEmailExisted(Domain.Members.Email email, MemberId? memberId,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<Member>().Where(x => x.Email.Equals(email)).AsQueryable();
        if (memberId is not null) query = query.Where(x => !x.Id.Equals(memberId) && x.IsActive);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> IsPhoneExisted(PhoneNumber phoneNumber, MemberId? memberId,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<Member>().Where(x => x.PhoneNumber.Equals(phoneNumber)).AsQueryable();
        if (memberId is not null) query = query.Where(x => !x.Id.Equals(memberId) && x.IsActive);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<string> GetNextMemberCode(CancellationToken cancellationToken = default)
    {
        var latestMember = await GetLatestByProperty(x => x.MemberCode, cancellationToken);
        var latestBookingCode = latestMember != null ? latestMember.MemberCode.Value : string.Empty;
        var code = string.IsNullOrEmpty(latestBookingCode) ? "0".PadLeft(5, '0') : latestBookingCode.Remove(0, 2);
        var newCode = "KH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
        return newCode;
    }
}