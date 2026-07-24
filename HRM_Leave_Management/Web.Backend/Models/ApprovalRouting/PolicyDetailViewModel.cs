using Application.ApprovalRouting.Queries.GetApprovalRoutePolicyDetail;
using Domain.Employees;
using Domain.Positions;

namespace Web.Backend.Models.ApprovalRouting;

public class PolicyDetailViewModel
{
    public ApprovalRoutePolicyDetailDto Detail { get; set; } = null!;
    public List<Position> Positions { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public bool CanUpdate { get; set; }
}
