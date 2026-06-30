using System.Security.Claims;
using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class MemberContext : IMemberContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MemberContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string IdentityId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Claims
            .SingleOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?
            .Value ??
        throw new ApplicationException("Member context is unavailable");
}