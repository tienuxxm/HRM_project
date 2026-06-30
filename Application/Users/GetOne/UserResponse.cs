using Application.Roles.GetOne;

namespace Application.Users.GetOne;

public sealed class UserResponse{
    
    public Guid Id { get; init; }
    
    public string? Email { get; init; }
    
    public string Fullname { get; init; }
    
    public string? PhoneNumber { get; init; }
    
    public string Username { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public List<RoleResponse>? Roles  { get;  set; }
    
    public string RolesResponse => string.Join(",", Roles.Select(x => x.DisplayName));
    
    public string IdentityId { get; private set; } = string.Empty;
    
}

