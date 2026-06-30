using Domain.Abstractions;
using Domain.Members;
using Domain.Vouchers;

namespace Domain.MemberVouchers;

public class MemberVoucher : Entity<MemberVoucherId>
{
    private MemberVoucher()
    {
    }

    private MemberVoucher(MemberVoucherId id, MemberId memberId, VoucherId voucherId) : base(id)
    {
        MemberId = memberId;
        VoucherId = voucherId;
    }

    private MemberVoucher(MemberVoucherId id, MemberId memberId, Voucher voucher) : base(id)
    {
        MemberId = memberId;
        Voucher = voucher;
        VoucherId = voucher.Id;
    }

    public MemberVoucherStatus MemberVoucherStatus
    {
        get
        {
            if (IsUsed)
                return MemberVoucherStatus.Used;
            if (IsVoucherExpired)
                return MemberVoucherStatus.Expired;
            return MemberVoucherStatus.Available;
        }
    }

    public bool IsVoucherExpired => DateTime.UtcNow.Date > Voucher.EndedDate.Date;

    public bool IsUsed { get; private set; }
    public MemberId MemberId { get; private set; }
    public VoucherId VoucherId { get; private set; }
    public Voucher Voucher { get; }
    public Member Member { get; }

    public static MemberVoucher Create(MemberId memberId, VoucherId voucherId)
    {
        return new MemberVoucher(MemberVoucherId.New, memberId, voucherId);
    }

    public static MemberVoucher Create(MemberId memberId, Voucher voucher)
    {
        return new MemberVoucher(MemberVoucherId.New, memberId, voucher);
    }

    public Result UseVoucher()
    {
        if (IsVoucherExpired)
            return Result.Failure(MemberVoucherErrors.Expired);
        IsUsed = true;
        return Result.Success();
    }
}