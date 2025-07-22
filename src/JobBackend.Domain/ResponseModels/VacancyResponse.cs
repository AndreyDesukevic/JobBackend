namespace JobBackend.Domain.ResponseModels;

public class VacancyResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? City { get; set; }
    public int? SalaryFrom { get; set; }
    public int? SalaryTo { get; set; }
    public string? Currency { get; set; }
    public string? Company { get; set; }
    public string? Url { get; set; }
}
