using Domain.Abstractions;

namespace Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    Task<List<User>?> GetByIdsAsync(List<UserId> ids, CancellationToken cancellationToken = default);

    Task<List<User>?> Pagination(int take, int skip, CancellationToken cancellationToken = default);
    Task<User?> FindUniqEmail(UserId id, Email email, CancellationToken cancellationToken = default);

    void Add(User user);

    void Update(User user);

    void AddRange(List<User> users);
    void Remove(User user);

    void RemoveRange(List<User> usersList);

    Task<PagedList<User>> GetAllPaged(PagedQuery<User, UserId> request,
        IQueryable<User>? queryable = null);

    IQueryable<User> GetEntitiesAsQueryable();
}