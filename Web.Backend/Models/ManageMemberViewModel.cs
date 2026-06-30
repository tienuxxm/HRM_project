namespace Web.Backend.Models;

public class ManageMemberViewModel
{
    public Guid? Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => FirstName + " " + LastName;
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string? BirthDate { get; set; }
    public int? DistrictId { get; set; }
    public int? ProvinceId { get; set; }
    public string? Note { get; set; }
    public string TitlePage { get; set; } = "Add Customer";
}