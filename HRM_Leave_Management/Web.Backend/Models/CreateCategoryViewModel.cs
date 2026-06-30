namespace Web.Backend.Models;

public class CreateCategoryViewModel
{
    public string CategoryName { get; set; }
    public Guid? Id { get; set; }
    public string Description { get; set; }
    public int? Index { get; set; }
}