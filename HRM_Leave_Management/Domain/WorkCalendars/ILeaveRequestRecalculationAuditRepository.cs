using System.Linq.Expressions;
using Domain.LeaveRequests;

namespace Domain.WorkCalendars;

public interface ILeaveRequestRecalculationAuditRepository
{
    Task AddAsync(LeaveRequestRecalculationAudit audit, CancellationToken cancellationToken = default);
    Task<List<LeaveRequestRecalculationAudit>> GetByBatchIdAsync(CalendarImportBatchId batchId, CancellationToken cancellationToken = default);
    Task<List<LeaveRequestRecalculationAudit>> GetByLeaveRequestIdAsync(LeaveRequestId leaveRequestId, CancellationToken cancellationToken = default);
    Task<bool> IsExistedAsync(Expression<Func<LeaveRequestRecalculationAudit, bool>> expression, CancellationToken cancellationToken = default);
}
