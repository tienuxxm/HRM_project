namespace Domain.Districts;

public interface IDistrictRepository
{
    public IQueryable<District> GetEntitiesAsQueryable();
}