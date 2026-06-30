namespace Domain.MemberVouchers;

public record MemberVoucherId(Guid Value)
{
    public static MemberVoucherId New => new MemberVoucherId(Guid.NewGuid());
}