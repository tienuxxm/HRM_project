using Application.Abstractions.Messaging;
using Application.Roles.GetOne;

namespace Application.Permissions.GetAll;

public record GetAllPermissionCommand(int? Take, int? Skip, string? Search) :  ICommand<List<PermissionResponse>?>;

public sealed class PermissionResponse{
 
    public Guid Id { get; init; }
    
    public string ResourceName { get; init; }
    
    public string DisplayName{ get; init; }
    
    public DateTime CreatedDate{ get; init; }

}