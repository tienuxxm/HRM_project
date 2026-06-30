using Domain.PhoneValidationCheck;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PhoneValidationCheckRepository : Repository<PhoneValidationCheck, PhoneValidationCheckId>,
    IPhoneValidationCheckRepository
{
    public PhoneValidationCheckRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PhoneValidationCheck?> GetByPhoneNumber(PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<PhoneValidationCheck>()
            .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
    }
}