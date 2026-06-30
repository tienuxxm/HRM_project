using Domain.Abstractions;

namespace Domain.Vouchers;

public class VoucherErrors
{
    public static Error NotFound = new(
        "Voucher.NotFound",
        "Voucher không tồn tại");

    public static Error NotEnoughPoint = new(
        "Voucher.NotEnoughtPoint",
        "Bạn không đủ Points để đổi voucher này");

    public static Error VoucherExpired = new(
        "Voucher.Expired",
        "Voucher đã hết hạn");

    public static Error VoucherOutOfRange = new(
        "Voucher.OutOfRange",
        "Voucher đã hết số lượng quy đổi");

    public static Error InvalidVoucher = new(
        "Voucher.Invalid",
        "Voucher không tồn tại");
}