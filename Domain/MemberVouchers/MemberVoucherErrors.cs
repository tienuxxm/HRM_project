using Domain.Abstractions;

namespace Domain.MemberVouchers;

public class MemberVoucherErrors
{
    public static Error Expired => new Error("MemberVoucher.Expired", "Voucher has been expired");
    public static Error VoucherExisted => new Error("MemberVoucher.Existed", "Bạn đã có voucher này");
}