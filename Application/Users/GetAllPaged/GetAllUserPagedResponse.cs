using Application.Users.GetOne;
using Domain.Abstractions;

namespace Application.Users.GetAllPaged;

public class GetAllUserPagedResponse : PagedList<UserResponse>
{
    public GetAllUserPagedResponse(List<UserResponse> items, int count, int pageNumber, int pageSize) : base(items,
        count, pageNumber, pageSize)
    {
    }
}