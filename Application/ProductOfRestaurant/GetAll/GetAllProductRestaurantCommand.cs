using Application.Abstractions.Messaging;
using Application.ProductOfRestaurant.Response;

namespace Application.ProductOfRestaurant.GetAll;

public record GetAllProductRestaurantCommand() : ICommand<List<ProductOfRestaurantResponse>>;