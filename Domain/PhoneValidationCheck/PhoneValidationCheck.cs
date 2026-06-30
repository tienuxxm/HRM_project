using Domain.Abstractions;

namespace Domain.PhoneValidationCheck;

public class PhoneValidationCheck : Entity<PhoneValidationCheckId>
{
    private readonly int LimitSendCount = 3;
    private readonly double ExpiedTimeSecond = 60;

    private PhoneValidationCheck()
    {
    }

    private PhoneValidationCheck(PhoneValidationCheckId id, PhoneNumber phoneNumber,
        DateTime lastSent, Code code) :
        base(id)
    {
        PhoneNumber = phoneNumber;
        Code = code;
        LastSent = lastSent;
        SendCodeCount++;
    }

    public PhoneNumber PhoneNumber { get; private set; }
    public SendCodeCount SendCodeCount { get; private set; } = new SendCodeCount(0);
    public DateTime LastSent { get; private set; }
    public Code Code { get; private set; }

    public static PhoneValidationCheck SendCode(PhoneNumber phoneNumber,
        DateTime lastSent)
    {
        return new PhoneValidationCheck(PhoneValidationCheckId.New, phoneNumber, lastSent, new Code(GetRandomCode()));
    }

    public Result<bool> IsCodeValid(DateTime time, Code code)
    {
        var diffTime = (time - LastSent).TotalSeconds;
        if (diffTime > ExpiedTimeSecond)
            return Result.Failure<bool>(new Error("ValidationCode.Expired", "Code has been expired"));
        return code != Code
            ? Result.Failure<bool>(new Error("ValidationCode.WrongCode", "Your validation code is wrong"))
            : Result.Success(code == Code);
    }

    public Result ResendCode(DateTime sentTime)
    {
        var diffTime = (sentTime - LastSent).TotalSeconds;
        if (diffTime <= ExpiedTimeSecond)
            return Result.Failure(new Error("SendCode.Fail",
                $"You cannot send code at this time, please retry after {ExpiedTimeSecond - diffTime} seconds"));
        LastSent = sentTime;
        Code = new Code(GetRandomCode());
        SendCodeCount++;
        return Result.Success();
    }

    public static string GetRandomCode()
    {
        var random = new Random();
        var codeNumber = random.Next(0, 1000000);
        return codeNumber.ToString("000000");
    }

    public void ResetSendCount()
    {
        SendCodeCount = new SendCodeCount(0);
    }
}