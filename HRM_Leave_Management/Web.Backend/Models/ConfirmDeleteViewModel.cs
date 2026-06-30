namespace Web.Backend.Models;

public class ConfirmDeleteViewModel
{
    public string Type { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string ActionName { get; set; }
    public string ActionController { get; set; }
    public string? Url { get; set; }
}