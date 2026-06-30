using Application.Abstractions.Messaging;
using Application.MemberVoucher.Response;
using Domain.Abstractions;
using Domain.Members;
using Domain.MemberVouchers;

namespace Application.MemberVoucher.GetAllPaged;

public record GetAllMemberVoucherPagedCommand(MemberId? MemberId, MemberVoucherStatus? MemberVoucherStatus = null) :
    PagedQuery<Domain.MemberVouchers.MemberVoucher, MemberVoucherId>, ICommand<PagedList<MemberVoucherResponse>>;