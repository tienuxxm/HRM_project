using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Login;

internal sealed class AdminLoginCommandHandler : ICommandHandler<AdminLoginCommand, AccessTokenResponse>
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;

    public AdminLoginCommandHandler(IJwtService jwtService, IUserRepository userRepository)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    public async Task<Result<AccessTokenResponse>> Handle(AdminLoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = (await _userRepository.GetEntitiesAsQueryable()
                    .ToListAsync(cancellationToken))
                .FirstOrDefault(x =>
                    x.Username.Value.ToLower() == request.username.ToLower() &&
                    !(x.IsDeleted.HasValue && x.IsDeleted.Value));
            if (user is null)
                return Result.Failure<AccessTokenResponse>(UserErrors.NotFound);
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