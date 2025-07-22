namespace JobBackend.Domain.ResponseModels;

public class SavedSearchResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }
    public int NewItemsCount { get; set; }
}
