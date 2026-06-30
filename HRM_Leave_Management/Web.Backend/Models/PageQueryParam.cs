namespace Web.Backend.Models;

public class PageQueryParam
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortOrder { get; set; }
    public string? SortColumn { get; set; }
}