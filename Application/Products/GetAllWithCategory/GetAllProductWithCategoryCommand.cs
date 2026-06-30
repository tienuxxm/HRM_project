using Application.Abstractions.Messaging;

namespace Application.Products.GetAllWithCategory;

public record GetAllProductWithCategoryCommand
    (bool? allowDelivery = null) : ICommand<List<ProductWithCategoryResponse>>;