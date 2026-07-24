using Application.Abstractions.Messaging;

namespace Application.LeaveRequests.GetDepartmentLeaveLoad;

public record GetDepartmentLeaveLoadQuery() : IQuery<List<DepartmentLeaveLoadItem>>;
