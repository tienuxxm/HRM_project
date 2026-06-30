using Domain.Abstractions;

namespace Infrastructure.SmsServices;

internal sealed class ESmsErrors
{
    public static Error SendCodeFail = new("Esms.SendCode.Fail", "Fail to send code");
}