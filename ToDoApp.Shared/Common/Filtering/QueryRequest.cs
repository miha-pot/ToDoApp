namespace ToDoApp.Shared.Common.Filtering;

public class QueryRequest
{
    public List<QueryFilter> Filters { get; set; } = [];
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
