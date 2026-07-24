using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Queries.GetApprovalRoutePolicyDetail;

internal sealed class GetApprovalRoutePolicyDetailQueryHandler
    : IQueryHandler<GetApprovalRoutePolicyDetailQuery, ApprovalRoutePolicyDetailDto>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public GetApprovalRoutePolicyDetailQueryHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IPositionRepository positionRepository,
        IEmployeeRepository employeeRepository)
    {
        _policyRepository = policyRepository;
        _positionRepository = positionRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<ApprovalRoutePolicyDetailDto>> Handle(
        GetApprovalRoutePolicyDetailQuery request,
        CancellationToken cancellationToken)
    {
        var policyId = new ApprovalRoutePolicyId(request.PolicyId);
        var policy = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Department)
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .Include(p => p.Rules)
                .ThenInclude(r => r.Candidates)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy == null)
        {
            return Result.Failure<ApprovalRoutePolicyDetailDto>(new Error("ApprovalRoute.PolicyNotFound", "The specified approval route policy was not found."));
        }

        // Map Position names
        var positionIds = policy.Rules.Select(r => r.RequesterPositionId).Distinct().ToList();
        var positionDict = await _positionRepository.GetEntitiesAsQueryable()
            .Where(p => positionIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        // Map Employee names
        var employeeIds = new List<EmployeeId>();
        foreach (var level in policy.Levels)
        {
            foreach (var a in level.Assignments.Where(a => a.IsActive))
            {
                employeeIds.Add(a.AssignedEmployeeId);
            }
        }
        foreach (var rule in policy.Rules)
        {
            if (rule.SpecificApproverEmployeeId != null)
            {
                employeeIds.Add(rule.SpecificApproverEmployeeId);
            }
        }
        employeeIds = employeeIds.Distinct().ToList();

        var employeeDict = await _employeeRepository.GetEntitiesAsQueryable()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => (Name: e.FullName, Code: e.EmployeeCode), cancellationToken);

        // Build Level Slots
        var levelSlots = policy.Levels.Where(l => l.IsActive).OrderBy(l => l.LevelRank).Select(l =>
        {
            var activeAssignment = l.Assignments.FirstOrDefault(a => a.IsActive && a.IsValidOnDate(DateOnly.FromDateTime(DateTime.Today)));
            string? empName = null;
            string? empCode = null;
            Guid? empId = null;

            if (activeAssignment != null && employeeDict.TryGetValue(activeAssignment.AssignedEmployeeId, out var empInfo))
            {
                empName = empInfo.Name;
                empCode = empInfo.Code;
                empId = activeAssignment.AssignedEmployeeId.Value;
            }

            return new LevelSlotSummaryDto(
                l.Id.Value,
                l.LevelName,
                l.LevelRank,
                l.CanApproveLeave,
                empId,
                empName,
                empCode,
                activeAssignment != null);
        }).ToList();

        var levelDict = levelSlots.ToDictionary(ls => ls.LevelId, ls => ls);

        // Build Position Rules
        var positionRules = policy.Rules.Where(r => r.IsActive).Select(r =>
        {
            string posName = positionDict.TryGetValue(r.RequesterPositionId, out var name) ? name : r.RequesterPositionId.Value.ToString();
            string? specName = null;
            string? specCode = null;
            bool isOverride = r.SpecificApproverEmployeeId != null;

            if (r.SpecificApproverEmployeeId != null && employeeDict.TryGetValue(r.SpecificApproverEmployeeId, out var specInfo))
            {
                specName = specInfo.Name;
                specCode = specInfo.Code;
            }

            var candidates = r.Candidates.Where(c => c.IsActive).OrderBy(c => c.PriorityOrder).Select(c =>
            {
                levelDict.TryGetValue(c.ApprovalRouteLevelId.Value, out var levelSummary);
                return new RuleCandidateSummaryDto(
                    c.Id.Value,
                    c.ApprovalRouteLevelId.Value,
                    levelSummary?.LevelName ?? "Level",
                    levelSummary?.LevelRank ?? 1,
                    c.PriorityOrder,
                    levelSummary?.CurrentlyAssignedEmployeeName);
            }).ToList();

            return new PositionRuleSummaryDto(
                r.Id.Value,
                r.RequesterPositionId.Value,
                posName,
                isOverride,
                r.SpecificApproverEmployeeId?.Value,
                specName,
                specCode,
                r.IsActive,
                candidates);
        }).ToList();

        var result = new ApprovalRoutePolicyDetailDto(
            policy.Id.Value,
            policy.Name,
            policy.DepartmentId.Value,
            policy.Department?.Name ?? "Department",
            policy.IsActive,
            policy.CreatedAt,
            levelSlots,
            positionRules);

        return Result.Success(result);
    }
}
