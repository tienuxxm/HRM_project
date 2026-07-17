using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.GetUserInfo;

public class GetUserInfoCommandHandler : ICommandHandler<GetUserInfoCommand, UserInfoResponse>
{
    private readonly IUserRepository _repository;
    private readonly IUserContext _userContext;
    private readonly IEmployeeRepository _employeeRepository;

    public GetUserInfoCommandHandler(IUserRepository repository, IUserContext userContext, IEmployeeRepository employeeRepository)
    {
        _repository = repository;
        _userContext = userContext;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<UserInfoResponse>> Handle(GetUserInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_userContext.IdentityId))
                return Result.Failure<UserInfoResponse>(UserErrors.InvalidCredentials);
            var user = await _repository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(x => x.IdentityId.Equals(new IdentityId(_userContext.IdentityId)),
                    cancellationToken);
            if (user is null || (user.IsDeleted.HasValue && user.IsDeleted.Value))
                return Result.Failure<UserInfoResponse>(UserErrors.InvalidCredentials);

            var employee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
            if (employee is not null && !employee.IsActive)
                return Result.Failure<UserInfoResponse>(UserErrors.InvalidCredentials);

            return Result.Success(new UserInfoResponse()
            {
                Id = user.Id.Value,
                Username = user.Username.Value
            });
        }
        catch (Exception)
        {
            return Result.Failure<UserInfoResponse>(UserErrors.InvalidCredentials);
        }
    }
}