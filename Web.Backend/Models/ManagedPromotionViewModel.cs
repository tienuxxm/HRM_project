using System.ComponentModel.DataAnnotations;
using Application.Restaurants.GetAll;
using Domain.Extension;


namespace Web.Backend.Models;

public class ManagedPromotionViewModel
{
     public Guid? Id { get; set; }
     
     [MaxLength(250)] public string TitlePage { get; set; }
     
    [MaxLength(250)] public string PromotionName { get; set; }
    [Required] [MaxLength(250)] public string Title { get; set; }
    [Required] [MaxLength(250)] public string Content { get; set; }
    public string StartedAt  { get; set;}
    public string EndedAt  { get; set;}
    public IFormFile? Image  { get; set;}
    
    public List<Guid>? RestaurantIds { get; set;}
    
    public List<RestaurantResponse>? Restaurants { get; set;}
    public string? ImageUrl  { get; set;}
    
    public DateTime StartedAtUtc  => StartedAt.StringToDateTimeUtc(); 
    public DateTime EndedAtUtc => EndedAt.StringToDateTimeUtc(); 
}