using Hangfire;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.Infrastructure.Services;

/// <summary>
/// Hangfire implementation of background job service
/// </summary>
public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue(Action action)
    {
        return BackgroundJob.Enqueue(() => action());
    }

    public string Enqueue<T>(Action<T> action)
    {
        return BackgroundJob.Enqueue<T>(x => action(x));
    }

    public string Schedule(Action action, TimeSpan delay)
    {
        return BackgroundJob.Schedule(() => action(), delay);
    }

    public string Schedule<T>(Action<T> action, TimeSpan delay)
    {
        return BackgroundJob.Schedule<T>(x => action(x), delay);
    }

    public void AddOrUpdate(string jobId, Action action, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobId, () => action(), cronExpression);
    }

    public void AddOrUpdate<T>(string jobId, Action<T> action, string cronExpression)
    {
        RecurringJob.AddOrUpdate<T>(jobId, x => action(x), cronExpression);
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }
}