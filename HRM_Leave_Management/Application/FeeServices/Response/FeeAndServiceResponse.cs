using Domain.FreeServices;
using Domain.Shared;

namespace Application.FeeServices.Response;

public class FeeAndServiceResponse
{
    public Guid Id { get; set; }
    public string FeeAndServiceName { get; set; }
    public string FeeAndServiceValue { get; set; }
    public FeeType FeeType { get; set; }
    public Money? FeeAmount { get; set; }
    
    public float? FeePercent { get; set; }
    public bool IsActive { get; set; }
    public bool IsPercent { get; set; }
}