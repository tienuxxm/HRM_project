namespace Infrastructure.Authentication;

public class GoogleAuth2Options
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUrl { get; set; }
    public string TokenUrl { get; set; }
}