using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Commands.AddApprovalRouteLevel;

internal sealed class AddApprovalRouteLevelCommandHandler : ICommandHandler<AddApprovalRouteLevelCommand, Guid>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public AddApprovalRouteLevelCommandHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IUserContext userContext,
        IRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _userContext = userContext;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddApprovalRouteLevelCommand request, CancellationToken cancellationToken)
    {
        var checkPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkPerm.Value)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.NoPermission);
        }

        if (string.IsNullOrWhiteSpace(request.LevelName))
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.LevelNameRequired);
        }

        if (request.LevelRank <= 0)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.LevelRankInvalid);
        }

        var policyId = new ApprovalRoutePolicyId(request.PolicyId);
        var policy = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy == null)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.PolicyNotFound);
        }

        try
        {
            var level = policy.AddLevel(request.LevelName, request.LevelRank, request.CanApproveLeave);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(level.Id.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(new Error("ApprovalRoute.DuplicateLevelRank", ex.Message));
        }
    }
}
