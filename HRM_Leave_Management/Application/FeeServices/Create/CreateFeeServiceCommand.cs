using Application.Abstractions.Messaging;
using Domain.FreeServices;

namespace Application.FeeServices.Create;

public record CreateFeeServiceCommand(List<CreateFeeServiceRequest> FeeServiceRequests) : ICommand;

public record CreateFeeServiceRequest
{
    public string FeeName { get; set; }
    public decimal? FeeAmount { get; set; }
    public float? FeePercent { get; set; }
    public bool IsPercent { get; set; }
    public FeeType FeeType { get; set; }
    public bool IsActive { get; set; }
}