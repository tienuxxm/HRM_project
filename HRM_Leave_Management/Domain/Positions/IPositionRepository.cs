using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Positions;

public interface IPositionRepository
{
    Task<Position?> GetByIdAsync(PositionId id, CancellationToken cancellationToken = default);

    void Add(Position position);

    void Update(Position position);

    void Remove(Position position);

    IQueryable<Position> GetEntitiesAsQueryable();

    Task<PagedList<Position>> GetAllPaged(PagedQuery<Position, PositionId> request,
        IQueryable<Position>? queryable = null);

    Task<bool> IsExistedAsync(Expression<Func<Position, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<List<Position>?> GetAll(CancellationToken cancellationToken = default);
}
