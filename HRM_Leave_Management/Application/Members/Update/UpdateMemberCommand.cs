using Application.Abstractions.Messaging;

namespace Application.Members.Update;

public record UpdateMemberCommand : ICommand
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? DistrictId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Note { get; set; }
}