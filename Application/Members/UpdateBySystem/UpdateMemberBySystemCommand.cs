using Application.Abstractions.Messaging;
using Domain.Members;

namespace Application.Members.UpdateBySystem;

public record UpdateMemberBySystemCommand : ICommand
{
    public MemberId Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? DistrictId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Note { get; set; }
}