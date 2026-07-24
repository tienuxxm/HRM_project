using Domain.Departments;

namespace Domain.ApprovalRouting;

public interface IApprovalRoutePolicyRepository
{
    Task<ApprovalRoutePolicy?> GetByIdAsync(ApprovalRoutePolicyId id, CancellationToken cancellationToken = default);
    Task<ApprovalRoutePolicy?> GetByDepartmentIdAsync(DepartmentId departmentId, CancellationToken cancellationToken = default);
    Task<bool> HasActivePolicyForDepartmentAsync(DepartmentId departmentId, CancellationToken cancellationToken = default);
    void Add(ApprovalRoutePolicy policy);
    void Update(ApprovalRoutePolicy policy);
    void Remove(ApprovalRoutePolicy policy);
    IQueryable<ApprovalRoutePolicy> GetEntitiesAsQueryable();
}
