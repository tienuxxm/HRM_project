using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Employees;
using Domain.Users;
using Domain.Roles;
using Domain.UserToRoles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Employees.ProvisionAccount;

internal sealed class ProvisionEmployeeAccountCommandHandler : ICommandHandler<ProvisionEmployeeAccountCommand, BooleanResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserToRoleRepository _userToRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ProvisionEmployeeAccountCommandHandler(
        IEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IUserToRoleRepository userToRoleRepository,
        IRoleRepository roleRepository,
        IAuthenticationService authenticationService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _userToRoleRepository = userToRoleRepository;
        _roleRepository = roleRepository;
        _authenticationService = authenticationService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(
        ProvisionEmployeeAccountCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get employee and check existence
        var employeeId = new EmployeeId(request.EmployeeId);
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Result.Failure<BooleanResponse>(EmployeeErrors.NotFound);
        }

        // 2. Guard: Check if employee already has a linked user
        if (employee.UserId is not null)
        {
            return Result.Failure<BooleanResponse>(EmployeeErrors.AlreadyLinkedToUser);
        }

        // 3. Database uniqueness check for User Email and Username
        var emailObj = new Email(request.Email.ToLower());
        var usernameObj = new Username(request.Username.ToLower());

        var duplicateEmail = await _userRepository.GetEntitiesAsQueryable()
            .AnyAsync(x => x.Email == emailObj && !(x.IsDeleted == true), cancellationToken);
        if (duplicateEmail)
        {
            return Result.Failure<BooleanResponse>(UserErrors.DuplicateEmail);
        }

        var duplicateUsername = await _userRepository.GetEntitiesAsQueryable()
            .AnyAsync(x => x.Username == usernameObj && !(x.IsDeleted == true), cancellationToken);
        if (duplicateUsername)
        {
            return Result.Failure<BooleanResponse>(UserErrors.DuplicateUsername);
        }

        // 3.1 Validate RoleIds exist in database
        var roleIds = new List<RoleId>();
        if (request.RoleIds != null)
        {
            foreach (var rId in request.RoleIds.Distinct())
            {
                roleIds.Add(new RoleId(rId));
            }
        }

        if (roleIds.Count > 0)
        {
            var roles = await _roleRepository.GetByIdsAsync(roleIds, cancellationToken);
            if (roles is null || roles.Count != roleIds.Count)
            {
                return Result.Failure<BooleanResponse>(RoleErrors.NotFound);
            }
        }

        // 4. Create User entity (without Keycloak ID first)
        var user = User.Create(
            new Name(employee.FullName),
            emailObj,
            null, // PhoneNumber
            usernameObj,
            null, // UserToRoles
            _dateTimeProvider.UtcNow);

        // 5. Register in Keycloak
        var identityIdResult = await _authenticationService.RegisterAsync(
            user,
            request.Password,
            cancellationToken);

        if (identityIdResult.IsFailure)
        {
            return Result.Failure<BooleanResponse>(identityIdResult.Error);
        }

        var identityId = identityIdResult.Value;
        user.SetIdentityId(identityId);

        // 6. Link User to Employee
        var linkResult = employee.LinkUser(user.Id);
        if (linkResult.IsFailure)
        {
            // Compensating action: Delete from Keycloak if already linked domain-side
            await _authenticationService.DeleteUser(identityId, cancellationToken);
            return Result.Failure<BooleanResponse>(linkResult.Error);
        }

        _userRepository.Add(user);

        if (request.RoleIds != null)
        {
            var userToRoles = new List<UserToRole>();
            foreach (var roleId in request.RoleIds.Distinct())
            {
                var userToRole = UserToRole.Create(new RoleId(roleId), user.Id, _dateTimeProvider.UtcNow);
                userToRoles.Add(userToRole);
            }

            _userToRoleRepository.AddRange(userToRoles);
        }

        _employeeRepository.Update(employee);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Compensating action: Delete from Keycloak to maintain transactional integrity
            await _authenticationService.DeleteUser(identityId, cancellationToken);
            throw;
        }

        return Result.Success(new BooleanResponse
        {
            Result = true,
            Message = "Cấp tài khoản nhân viên thành công."
        });
    }
}
