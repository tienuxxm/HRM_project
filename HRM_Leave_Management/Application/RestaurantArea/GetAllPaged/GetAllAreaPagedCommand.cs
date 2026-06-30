using Application.Abstractions.Messaging;
using Application.RestaurantArea.GetAll;
using Domain.Abstractions;
using Domain.RestaurantAreas;

namespace Application.RestaurantArea.GetAllPaged;

public record GetAllAreaPagedCommand() : PagedQuery<Domain.RestaurantAreas.RestaurantArea, RestaurantAreaId>,
    ICommand<PagedList<RestaurantAreaResponse>>;