using Application.Abstractions.Messaging;
using Application.Restaurants.GetAll;
using Domain.Abstractions;
using Domain.Restaurants;

namespace Application.Restaurants.GetAllPaged;

public record GetAllRestaurantPagedCommand() : PagedQuery<Restaurant, RestaurantId>,
    ICommand<PagedList<RestaurantResponse>>;