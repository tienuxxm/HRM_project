using System.Runtime.Serialization;

namespace Domain.MemberVouchers;

public enum MemberVoucherStatus
{
    [EnumMember(Value = "Available")] Available,
    [EnumMember(Value = "Used")] Used,
    [EnumMember(Value = "Expired")] Expired,
}