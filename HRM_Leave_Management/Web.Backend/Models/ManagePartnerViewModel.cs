namespace Web.Backend.Models;

public class ManagePartnerViewModel
{
    public Guid? id { get; set; }
    public string title { get; set; }
    public string PartnerName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}