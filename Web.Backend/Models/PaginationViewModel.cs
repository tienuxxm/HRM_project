namespace Web.Backend.Models;

public class PaginationViewModel
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPage { get; set; }
    public int TotalItem { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public string? ExtentQuery { get; set; }
    public int NextPage => HasNext ? CurrentPage + 1 : CurrentPage;
    public int PrevPage => HasPrevious ? CurrentPage - 1 : CurrentPage;

    public string PageString =>
        $"{CurrentPage * PageSize - (PageSize - 1)}  - {(TotalItem > (CurrentPage * PageSize) ? CurrentPage * PageSize : TotalItem)} of {TotalItem}";

    public string NextPageUrl => HasNext ? $"?Page={(CurrentPage + 1)}{ExtentQuery}" : "#";
    public string PrevPageUrl => HasPrevious ? $"?Page={(CurrentPage - 1)}{ExtentQuery}" : "#";

    public static List<int> PageSizeList => new List<int>() { 10, 20, 30, 40, 50 };
}