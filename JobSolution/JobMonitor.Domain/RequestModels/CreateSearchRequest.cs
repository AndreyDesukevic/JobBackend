namespace JobMonitor.Domain.RequestModels;

public class CreateSearchRequest
{
    public required string Name { get; set; } 
    public required string Text { get; set; }
}