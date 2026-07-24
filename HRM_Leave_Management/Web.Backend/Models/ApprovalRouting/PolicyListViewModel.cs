using Application.ApprovalRouting.Queries.GetApprovalRoutePolicies;
using Domain.Departments;

namespace Web.Backend.Models.ApprovalRouting;

public class PolicyListViewModel
{
    public List<ApprovalRoutePolicySummaryDto> Policies { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public string? DepartmentFilter { get; set; }
    public string? SearchTerm { get; set; }
    public bool CanUpdate { get; set; }
}
