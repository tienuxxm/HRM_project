using Application.Abstractions.Messaging;
using Application.Roles.GetOne;
using Application.Users.GetOne;
using Domain.Abstractions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.GetAllPaged;

public class GetAllUserPagedCommandHandler : ICommandHandler<GetAllUserPagedCommand, GetAllUserPagedResponse>
{
    private readonly IUserRepository _userRepository;

    public GetAllUserPagedCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GetAllUserPagedResponse>> Handle(GetAllUserPagedCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _userRepository.GetEntitiesAsQueryable()
                .Where(x => !x.IsDeleted.HasValue || !x.IsDeleted.Value)
                .Include(x => x.Roles)!.ThenInclude(x => x.Role).AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.AsEnumerable()
                    .Where(x => x.Email.Value.ToLower().Contains(request.SearchTerm.ToLower()) || x.Name.Value.ToLower()
                                    .Contains(request.SearchTerm.ToLower()) ||
                                x.Username.Value.ToLower().Contains(request.SearchTerm.ToLower()))
                    .AsQueryable();

            var users = await _userRepository.GetAllPaged(request, query);
            var usersDto = users.Data.Select(x => new UserResponse
            {
                Email = x.Email?.Value,
                Fullname = x.Name.Value,
                Id = x.Id.Value,
                Username = x.Username.Value,
                PhoneNumber = x.PhoneNumber?.Value,
                Roles = x.Roles?.Select(r => new RoleResponse
                {
                    Id = r.Id.Value,
                    DisplayName = r.Role.DisplayName.Value
                }).ToList()
            }).ToList();
            return Result.Success(
                new GetAllUserPagedResponse(usersDto, users.TotalCount, users.CurrentPage, users.PageSize));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}