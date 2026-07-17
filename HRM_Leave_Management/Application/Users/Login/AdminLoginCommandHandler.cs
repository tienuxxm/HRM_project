using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Login;

internal sealed class AdminLoginCommandHandler : ICommandHandler<AdminLoginCommand, AccessTokenResponse>
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public AdminLoginCommandHandler(
        IJwtService jwtService,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<AccessTokenResponse>> Handle(AdminLoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TECHNICAL DEBT: Loading all users into memory via ToListAsync() and filtering in-memory
            // is retained here to preserve the original case-insensitive username comparison behavior.
            // Converting this directly to a database-level query (e.g., EF.Functions.Like or ToLower() on database side)
            // requires thorough verification of DB collation and username casing standards, which is outside the current scope.
            var user = (await _userRepository.GetEntitiesAsQueryable()
                    .ToListAsync(cancellationToken))
                .FirstOrDefault(x =>
                    x.Username.Value.ToLower() == request.username.ToLower() &&
                    !(x.IsDeleted.HasValue && x.IsDeleted.Value));
            if (user is null)
                return Result.Failure<AccessTokenResponse>(UserErrors.NotFound);

            var employee = await _employeeRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
            if (employee is not null && !employee.IsActive)
            {
                return Result.Failure<AccessTokenResponse>(UserErrors.InvalidCredentials);
            }

            var accessToken = await _jwtService.GetAccessTokenAsync(user.Username.Value, request.password, cancellationToken);
            if (accessToken.IsFailure && user.Username.Value != user.Email.Value)
            {
                accessToken = await _jwtService.GetAccessTokenAsync(user.Email.Value, request.password, cancellationToken);
            }
            return accessToken.IsFailure
                ? Result.Failure<AccessTokenResponse>(UserErrors.InvalidCredentials)
                : Result.Success(new AccessTokenResponse(accessToken.Value));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}