using Domain.Abstractions;
using Domain.Districts;

namespace Domain.Provinces;

public class Province : Entity<ProvinceId>
{
    public string Name { get; set; }
    public List<District> Districts { get; set; }
}