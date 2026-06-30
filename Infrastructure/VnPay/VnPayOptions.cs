namespace Infrastructure.VnPay;

public class VnPayOptions
{
    public string TmnCode { get; set; }
    public string SecureHash { get; set; }
    public string ReturnUrl { get; set; }
    public string Url { get; set; }
    public string Version { get; set; }
}