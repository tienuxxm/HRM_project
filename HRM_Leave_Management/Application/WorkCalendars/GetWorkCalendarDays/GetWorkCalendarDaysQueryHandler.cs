using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.WorkCalendars;

namespace Application.WorkCalendars.GetWorkCalendarDays;

internal sealed class GetWorkCalendarDaysQueryHandler : IQueryHandler<GetWorkCalendarDaysQuery, List<WorkCalendarDayResponse>>
{
    private readonly IWorkCalendarDayRepository _workCalendarDayRepository;

    public GetWorkCalendarDaysQueryHandler(IWorkCalendarDayRepository workCalendarDayRepository)
    {
        _workCalendarDayRepository = workCalendarDayRepository;
    }

    public async Task<Result<List<WorkCalendarDayResponse>>> Handle(GetWorkCalendarDaysQuery request, CancellationToken cancellationToken)
    {
        int year = request.Year ?? DateTime.Today.Year;
        var days = await _workCalendarDayRepository.GetActiveByYearAsync(year, cancellationToken);

        if (request.Month.HasValue)
        {
            days = days.Where(d => d.Date.Month == request.Month.Value).ToList();
        }

        var response = days.Select(d => new WorkCalendarDayResponse
        {
            Id = d.Id.Value,
            Date = d.Date,
            DayType = d.DayType.ToString(),
            WorkShift = d.WorkShift.ToString(),
            Description = d.Description,
            IsActive = d.IsActive
        })
        .OrderBy(d => d.Date)
        .ToList();

        return response;
    }
}
