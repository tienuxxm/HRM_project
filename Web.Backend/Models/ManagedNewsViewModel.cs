using System.ComponentModel.DataAnnotations;


namespace Web.Backend.Models;

public class ManagedNewsViewModel
{
     public Guid? Id { get; set; }
    [MaxLength(250)] public string TitlePage { get; set; }
    [Required] [MaxLength(250)] public string Title { get; set; }
    [Required] [MaxLength(250)] public string Content { get; set; }
    [MaxLength(250)] public string Description  { get; set;}
    public IFormFile? Thumbnail  { get; set;}
    
    public string? ThumbnailUrl  { get; set;}
}