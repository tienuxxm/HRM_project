namespace Infrastructure.SmsServices;

public class ESmsOptions
{
    public string ApiKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string ApiUrl { get; init; } = string.Empty;
    public string Brandname { get; init; } = string.Empty;
}