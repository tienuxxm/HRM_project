namespace Domain.Provinces;

public interface IProvinceRepository
{
    public IQueryable<Province> GetEntitiesAsQueryable();
}