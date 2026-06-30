using Application.Vouchers.GetOne;
using Domain.Abstractions;

namespace Application.Vouchers.GetAllPaged;

public class GetAllVoucherPagedResposne : PagedList<VoucherResponse>
{
    public GetAllVoucherPagedResposne(List<VoucherResponse> items, int count, int pageNumber, int pageSize) : base(
        items, count, pageNumber, pageSize)
    {
    }
}