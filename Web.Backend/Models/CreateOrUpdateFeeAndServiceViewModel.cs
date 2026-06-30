using Application.Extensions;
using Domain.FreeServices;

namespace Web.Backend.Models;

public class CreateOrUpdateFeeAndServiceViewModel
{
    public List<CreateFeeAndServiceModel> CreateFeeAndServiceModels { get; set; }

    public Dictionary<int, string> FeeTypes = Enum.GetValues(typeof(FeeType)).Cast<FeeType>()
        .ToDictionary(t => (int)t, t => t.GetDescription());
}
public class UpdateFeeAndServiceViewModel
{
    public UpdateFeeAndServiceModel UpdateFeeAndServiceModel { get; set; }

    public Dictionary<int, string> FeeTypes = Enum.GetValues(typeof(FeeType)).Cast<FeeType>()
        .ToDictionary(t => (int)t, t => t.GetDescription());
}

public class CreateFeeAndServiceModel
{
    public Guid? Id { get; set; } 
    public string FeeAndServiceName { get; set; }
    public decimal FeeValue { get; set; }
    public bool IsPercent { get; set; }
    public bool IsActive { get; set; }
    public int FeeType { get; set; }
}
public class UpdateFeeAndServiceModel
{
    public Guid? Id { get; set; } 
    public string FeeAndServiceName { get; set; }
    public float? FeePercent { get; set; }
    public Decimal? FeeValue { get; set; }
    public Decimal? FeeAmount { get; set; }
    
    public bool IsPercent { get; set; }
    public bool IsActive { get; set; }
    public int FeeType { get; set; }
}