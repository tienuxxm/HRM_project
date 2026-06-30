using Application.Abstractions.Messaging;
using Domain.Products;
using Domain.Restaurants;

namespace Application.ProductOfRestaurant.Create;

public record CreateProductOfRestaurantCommand(List<ProductId> ProductIds, RestaurantId RestaurantId,
    bool AllowDelivery) : ICommand;