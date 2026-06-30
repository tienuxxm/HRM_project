namespace Infrastructure.SmsServices;

public sealed class ESmsGenCodeV4Options : ESmsOptions, ICloneable
{
    public string Phone { get; set; }
    public string TimeAlive { get; set; }
    public string NumCharOfCode { get; set; } = "6";
    public string BrandName { get; set; }
    public string message { get; set; }
    public string ApiKey { get; set; }
    public string SecretKey { get; set; }
    public string Type { get; set; } = "2";
    public string EndPoint { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}