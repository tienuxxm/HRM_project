using Application.Jobs.YearEndJob;
using MediatR;
using Quartz;

namespace Infrastructure.Jobs;

public class YearEndJob : IJob
{
    private readonly ISender _sender;

    public YearEndJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var yearEndJobCommand = new YearEndJobCommand();
        await _sender.Send(yearEndJobCommand);
    }
}