using Application.Roles.GetOne;
using Application.Users.GetOne;
using Domain.Abstractions;

namespace Application.Roles.GetAllPaged;

public class GetAllRolePagedResponse : PagedList<RoleResponse>
{
    public GetAllRolePagedResponse(List<RoleResponse> items, int count, int pageNumber, int pageSize) : base(items,
        count, pageNumber, pageSize)
    {
    }
}