using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Role;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Commands.AddApprovalRouteRule;

internal sealed class AddApprovalRouteRuleCommandHandler : ICommandHandler<AddApprovalRouteRuleCommand, Guid>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IUserContext _userContext;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public AddApprovalRouteRuleCommandHandler(
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

    public async Task<Result<Guid>> Handle(AddApprovalRouteRuleCommand request, CancellationToken cancellationToken)
    {
        var checkPerm = await _roleService.checkRoleExist(_userContext.IdentityId, "UPDATE_LEAVE_APPROVER_ASSIGNMENT", cancellationToken);
        if (!checkPerm.Value)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.NoPermission);
        }

        if (request.RequesterPositionId == Guid.Empty)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.RequesterPositionRequired);
        }

        var policyId = new ApprovalRoutePolicyId(request.PolicyId);
        var policy = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Levels)
            .Include(p => p.Rules)
                .ThenInclude(r => r.Candidates)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy == null)
        {
            return Result.Failure<Guid>(ApprovalRouteErrors.PolicyNotFound);
        }

        var requesterPositionId = new PositionId(request.RequesterPositionId);
        var candidateLevelId = new ApprovalRouteLevelId(request.CandidateLevelId);

        try
        {
            var rule = policy.Rules.FirstOrDefault(r => r.IsActive && r.RequesterPositionId == requesterPositionId);
            if (rule == null)
            {
                rule = policy.AddRule(requesterPositionId);
            }

            var candidate = policy.AddRuleCandidate(rule.Id, candidateLevelId, request.PriorityOrder > 0 ? request.PriorityOrder : 1);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(rule.Id.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(new Error("ApprovalRoute.RuleError", ex.Message));
        }
    }
}
