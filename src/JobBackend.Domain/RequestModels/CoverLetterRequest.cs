namespace JobBackend.Domain.RequestModels;

public class CoverLetterRequest
{
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Skills { get; set; }
}
