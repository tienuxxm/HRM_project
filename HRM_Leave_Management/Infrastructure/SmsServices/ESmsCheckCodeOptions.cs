namespace Infrastructure.SmsServices;

public sealed class ESmsCheckCodeOptions : ESmsOptions, ICloneable
{
    public string Code { get; set; }
    public string PhoneNumber { get; set; }
    public string EndPoint { get; set; }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}