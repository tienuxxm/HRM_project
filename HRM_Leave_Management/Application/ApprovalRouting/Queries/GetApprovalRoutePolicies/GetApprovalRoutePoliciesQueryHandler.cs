using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Departments;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Queries.GetApprovalRoutePolicies;

internal sealed class GetApprovalRoutePoliciesQueryHandler
    : IQueryHandler<GetApprovalRoutePoliciesQuery, List<ApprovalRoutePolicySummaryDto>>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public GetApprovalRoutePoliciesQueryHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IDepartmentRepository departmentRepository)
    {
        _policyRepository = policyRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<List<ApprovalRoutePolicySummaryDto>>> Handle(
        GetApprovalRoutePoliciesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Department)
            .Include(p => p.Levels)
            .Include(p => p.Rules)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.DepartmentFilter))
        {
            query = query.Where(p => p.Department != null && p.Department.Name.ToLower().Contains(request.DepartmentFilter.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || (p.Department != null && p.Department.Name.ToLower().Contains(term)));
        }

        var policies = await query.ToListAsync(cancellationToken);

        var list = policies.Select(p =>
        {
            var activeLevels = p.Levels.Where(l => l.IsActive).OrderBy(l => l.LevelRank).ToList();
            var levelSummary = activeLevels.Any()
                ? string.Join(", ", activeLevels.Select(l => $"Level {l.LevelRank} ({l.LevelName})"))
                : "No level slots";

            return new ApprovalRoutePolicySummaryDto(
                p.Id.Value,
                p.Name,
                p.DepartmentId.Value,
                p.Department?.Name ?? "Unknown Department",
                p.Rules.Count(r => r.IsActive),
                activeLevels.Count,
                levelSummary,
                p.IsActive,
                p.CreatedAt,
                p.CreatedAt);
        }).ToList();

        return Result.Success(list);
    }
}
