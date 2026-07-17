using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.GetLeaveRequestRecalculationAudits;

public sealed record GetLeaveRequestRecalculationAuditsQuery(Guid LeaveRequestId) : IQuery<List<LeaveRequestRecalculationAuditResponse>>;
