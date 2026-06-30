using System.Diagnostics;
using Application.Jobs.AppJob;
using MediatR;
using Quartz;

namespace Infrastructure.Jobs;

public class DailyJob : IJob
{
    private readonly ISender _sender;

    public DailyJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var command = new AppJobCommand();
        await _sender.Send(command);
        Console.WriteLine("MyDailyJob executed at: " + DateTime.Now);
        Debug.WriteLine("MyDailyJob executed at: " + DateTime.Now);
    }
}