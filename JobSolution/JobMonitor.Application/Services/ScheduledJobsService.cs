using JobMonitor.Domain.Models;
using System.Collections.Concurrent;

namespace JobMonitor.Application.Services;

public class ScheduledJobsService
{
    public ConcurrentDictionary<Guid, JobTask> JobTasks { get; } = new();

    public JobTask? TaskOrDefaultByGuid(Guid taskId)
    {
        return !JobTasks.ContainsKey(taskId) ? null : JobTasks[taskId];
    }

    public Task<ScheduledJobStatus> AddTaskAsync<TResult>(string jobName, Func<CancellationToken, Task<TResult>> runTask)
    {
        var taskId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();

        var jobTask = new JobTask(taskId, jobName, runTask(cancellationTokenSource.Token), DateTime.UtcNow, cancellationTokenSource);
        while (!JobTasks.TryAdd(taskId, jobTask))
        {
            taskId = Guid.NewGuid();
        }

        return Task.FromResult(JobTasks[taskId].Status);
    }

    public JobTask? CancelTask(Guid guid)
    {
        var jobTask = TaskOrDefaultByGuid(guid);
        jobTask?.CancellationTokenSource.Cancel();

        return jobTask;
    }

    public async Task ClearHistoryAsync(CancellationToken cancellationToken)
    {
        var successfullyCompletedTasks = JobTasks
            .Where(x => x.Value.Task.IsCompletedSuccessfully);
        await ClearJobTaskListAsync(successfullyCompletedTasks, cancellationToken);
    }

    public async Task ClearJobTaskListAsync(CancellationToken cancellationToken)
    {
        var completedTasks = JobTasks.Where(x => x.Value.Task.IsCompleted).ToList();
        await ClearJobTaskListAsync(completedTasks, cancellationToken);
    }

    private async Task ClearJobTaskListAsync(IEnumerable<KeyValuePair<Guid, JobTask>> jobTasks, CancellationToken cancellationToken)
    {
        foreach (var jobTask in jobTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (!JobTasks.TryRemove(jobTask))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    public record JobTask (Guid TaskId, string JobName, Task Task, DateTime StartDate, CancellationTokenSource CancellationTokenSource)
    {
        public ScheduledJobStatus Status =>
            new()
            {
                Id = TaskId,
                Name = JobName,
                StartDate = StartDate,
                IsCompleted = Task.IsCompleted,
                IsFaulted = Task.IsFaulted,
                IsCanceled = Task.IsCanceled,
                IsCompletedSuccessfully = Task.IsCompletedSuccessfully,
                Exception = Task.Exception
            };
    }
}
