using Application.Partners.GetOne;
using Domain.Abstractions;

namespace Application.Partners.GetAllPaged;

public class GetAllPartnerPagedResponse : PagedList<PartnerResponse>
{
    public GetAllPartnerPagedResponse(List<PartnerResponse> items, int count, int pageNumber, int pageSize) : base(
        items, count, pageNumber, pageSize)
    {
    }
}