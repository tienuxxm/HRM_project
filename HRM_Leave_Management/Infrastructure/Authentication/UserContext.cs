using System.Security.Claims;
using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string IdentityId
    {
        get
        {
            return _httpContextAccessor
                       .HttpContext?
                       .User
                       .Claims
                       .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?
                       .Value ??
                   throw new ApplicationException("Member context is unavailable");
        }
    }
}