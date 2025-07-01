using System.Text.Json.Serialization;

namespace JobMonitor.Domain.Models;

public class ScheduledJobStatus
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime StartDate { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFaulted { get; set; }
    public bool IsCanceled { get; set; }
    public bool IsCompletedSuccessfully { get; set; }

    [JsonIgnore]
    public AggregateException? Exception { get; set; }

    public List<string> Messages 
    { 
        get
        {
            var messages = new List<string>();
            if (Exception != null)
            {
                messages.Add($"{Exception.GetType()}: {Exception.Message}");
                messages.AddRange(Exception.Flatten().InnerExceptions.Select(e => $"{e.GetType()}: {e.Message}"));
            }

            return messages;
        }
    }
}
