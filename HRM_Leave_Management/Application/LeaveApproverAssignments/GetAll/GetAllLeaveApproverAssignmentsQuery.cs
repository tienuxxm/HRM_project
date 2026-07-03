using Application.Abstractions.Messaging;
using Domain.LeaveApproverAssignments;

namespace Application.LeaveApproverAssignments.GetAll;

public sealed record GetAllLeaveApproverAssignmentsQuery : IQuery<List<LeaveApproverAssignment>>;
