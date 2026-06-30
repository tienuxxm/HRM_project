using Domain.Abstractions;
using Domain.Districts;

namespace Domain.Wards;

public class Ward : Entity<WardId>
{
    public string Name { get; set; }
    public DistrictId DistrictId { get; set; }
    public District District { get; set; }
}