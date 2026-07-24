using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.ApprovalRouting;
using Domain.Employees;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;

namespace Application.ApprovalRouting.Queries.GetLevelSlotAssignments;

internal sealed class GetLevelSlotAssignmentsQueryHandler
    : IQueryHandler<GetLevelSlotAssignmentsQuery, LevelSlotAssignmentsDto>
{
    private readonly IApprovalRoutePolicyRepository _policyRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestApprovalAssignmentRepository _leaveAssignmentRepository;

    public GetLevelSlotAssignmentsQueryHandler(
        IApprovalRoutePolicyRepository policyRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestApprovalAssignmentRepository leaveAssignmentRepository)
    {
        _policyRepository = policyRepository;
        _employeeRepository = employeeRepository;
        _leaveAssignmentRepository = leaveAssignmentRepository;
    }

    public async Task<Result<LevelSlotAssignmentsDto>> Handle(
        GetLevelSlotAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var policyId = new ApprovalRoutePolicyId(request.PolicyId);
        var policy = await _policyRepository.GetEntitiesAsQueryable()
            .Include(p => p.Department)
            .Include(p => p.Levels)
                .ThenInclude(l => l.Assignments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy == null)
        {
            return Result.Failure<LevelSlotAssignmentsDto>(new Error("ApprovalRoute.PolicyNotFound", "The specified approval route policy was not found."));
        }

        var assignedEmployeeIds = policy.Levels
            .SelectMany(l => l.Assignments)
            .Where(a => a.IsActive)
            .Select(a => a.AssignedEmployeeId)
            .Distinct()
            .ToList();

        var employeeDict = await _employeeRepository.GetEntitiesAsQueryable()
            .Where(e => assignedEmployeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => (Name: e.FullName, Code: e.EmployeeCode), cancellationToken);

        var rows = new List<LevelSlotAssignmentRowDto>();

        foreach (var level in policy.Levels.Where(l => l.IsActive).OrderBy(l => l.LevelRank))
        {
            var activeAssignment = level.Assignments.FirstOrDefault(a => a.IsActive && a.IsValidOnDate(DateOnly.FromDateTime(DateTime.Today)));

            Guid? assignId = activeAssignment?.Id.Value;
            Guid? empId = activeAssignment?.AssignedEmployeeId.Value;
            string? empName = null;
            string? empCode = null;
            DateOnly? from = activeAssignment?.EffectiveFrom;
            DateOnly? to = activeAssignment?.EffectiveTo;
            int impactedCount = 0;

            if (activeAssignment != null)
            {
                if (employeeDict.TryGetValue(activeAssignment.AssignedEmployeeId, out var empInfo))
                {
                    empName = empInfo.Name;
                    empCode = empInfo.Code;
                }

                var pendingAssignments = await _leaveAssignmentRepository.GetPendingAssignmentsByApproverAsync(activeAssignment.AssignedEmployeeId, cancellationToken);
                impactedCount = pendingAssignments.Count(a => a.LeaveRequest != null && a.LeaveRequest.Status == LeaveRequestStatus.Pending && a.SnapshotLevelAssignmentId == activeAssignment.Id);
            }

            rows.Add(new LevelSlotAssignmentRowDto(
                level.Id.Value,
                level.LevelName,
                level.LevelRank,
                level.CanApproveLeave,
                assignId,
                empId,
                empName,
                empCode,
                from,
                to,
                activeAssignment != null,
                impactedCount));
        }

        var dto = new LevelSlotAssignmentsDto(
            policy.Id.Value,
            policy.Name,
            policy.DepartmentId.Value,
            policy.Department?.Name ?? "Department",
            rows);

        return Result.Success(dto);
    }
}
