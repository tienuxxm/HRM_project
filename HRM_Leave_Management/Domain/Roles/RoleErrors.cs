using Domain.Abstractions;

namespace Domain.Roles;

public class RoleErrors
{
    public static Error NotFound = new(
        "Role.NotFound",
        "The role was not found");
    
}