using Application.InvoiceHistories.Response;
using Application.MemberPointHistories.Response;
using Application.Members.Responses;
using Application.MemberVoucher.Response;
using Domain.Abstractions;

namespace Web.Backend.Models;

public class MemberDetailViewModel
{
    public MemberResponse Member { get; set; }
    public string Tab { get; set; }
    public PagedList<MemberVoucherResponse>? MemberVouchers { get; set; }
    public PagedList<MemberPointHistoryResponse>? PointHistories { get; set; }
    public PagedList<InvoiceHistoryResponse>? InvoiceHistories { get; set; }
}