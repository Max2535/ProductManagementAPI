namespace ProductManagement.Application.Interfaces;

/// <summary>
/// Background job service interface
/// </summary>
public interface IBackgroundJobService
{
    // Fire and forget jobs
    string Enqueue(Action action);
    string Enqueue<T>(Action<T> action);

    // Delayed jobs
    string Schedule(Action action, TimeSpan delay);
    string Schedule<T>(Action<T> action, TimeSpan delay);

    // Recurring jobs
    void AddOrUpdate(string jobId, Action action, string cronExpression);
    void AddOrUpdate<T>(string jobId, Action<T> action, string cronExpression);

    // Job management
    bool Delete(string jobId);
}