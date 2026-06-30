using Domain.Abstractions;
using Domain.Provinces;
using Domain.Wards;

namespace Domain.Districts;

public class District : Entity<DistrictId>
{
    public ProvinceId ProvinceId { get; set; }
    public Province Province { get; set; }
    public string Name { get; set; }
    public List<Ward> Wards { get; set; }
}