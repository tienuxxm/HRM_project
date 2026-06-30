namespace Domain.PhoneValidationCheck;

public record SendCodeCount(int Value)
{
    public static SendCodeCount operator +(SendCodeCount first, SendCodeCount second)
    {
        return new SendCodeCount(first.Value + second.Value);
    }

    public static SendCodeCount operator ++(SendCodeCount first)
    {
        return new SendCodeCount(first.Value + 1);
    }
};