using Application.Abstractions.Messaging;
using Application.Response;
using System;
using System.Collections.Generic;

namespace Application.Employees.ProvisionAccount;

public sealed record ProvisionEmployeeAccountCommand(
    Guid EmployeeId,
    string Username,
    string Email,
    string Password,
    List<Guid> RoleIds) : ICommand<BooleanResponse>;
