using Domain.Abstractions;

namespace Domain.ApprovalRouting;

public static class ApprovalRouteErrors
{
    public static readonly Error NoPermission = new(
        "ApprovalRoute.NoPermission",
        "You do not have permission to perform this approval routing action.");

    public static readonly Error PolicyNotFound = new(
        "ApprovalRoute.PolicyNotFound",
        "The specified approval route policy was not found.");

    public static readonly Error PolicyNameRequired = new(
        "ApprovalRoute.PolicyNameRequired",
        "Policy name is required.");

    public static readonly Error DepartmentRequired = new(
        "ApprovalRoute.DepartmentRequired",
        "Department is required for department-level approval routing policy.");

    public static readonly Error DuplicateActivePolicyForDepartment = new(
        "ApprovalRoute.DuplicateActivePolicyForDepartment",
        "An active approval policy already exists for this department.");

    public static readonly Error DuplicateActiveCompanyPolicy = new(
        "ApprovalRoute.DuplicateActiveCompanyPolicy",
        "An active company-level approval policy already exists.");

    public static readonly Error LevelNotFound = new(
        "ApprovalRoute.LevelNotFound",
        "The specified approval level was not found in this policy.");

    public static readonly Error LevelNameRequired = new(
        "ApprovalRoute.LevelNameRequired",
        "Approval level name is required.");

    public static readonly Error LevelRankInvalid = new(
        "ApprovalRoute.LevelRankInvalid",
        "Approval level rank must be greater than zero.");

    public static readonly Error DuplicateLevelRank = new(
        "ApprovalRoute.DuplicateLevelRank",
        "An active level with this rank already exists in this policy.");

    public static readonly Error RequesterPositionRequired = new(
        "ApprovalRoute.RequesterPositionRequired",
        "Requester position is required for approval routing rule.");

    public static readonly Error LevelAssignmentNotFound = new(
        "ApprovalRoute.LevelAssignmentNotFound",
        "The specified level slot assignment was not found.");

    public static readonly Error ApproverEmployeeNotFound = new(
        "ApprovalRoute.ApproverEmployeeNotFound",
        "The specified approver employee was not found or is inactive.");

    public static readonly Error ApproverUserNotLinked = new(
        "ApprovalRoute.ApproverUserNotLinked",
        "The specified approver employee has no active linked user account.");

    public static readonly Error ApproverNoApprovePermission = new(
        "ApprovalRoute.ApproverNoApprovePermission",
        "The specified approver employee does not have permission 'APPROVE_LEAVE_REQUEST'.");

    public static readonly Error ApproverDepartmentMismatch = new(
        "ApprovalRoute.ApproverDepartmentMismatch",
        "The approver employee must belong to the same department as the approval policy.");
}
