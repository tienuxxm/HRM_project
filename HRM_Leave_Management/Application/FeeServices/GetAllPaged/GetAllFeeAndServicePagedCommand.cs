using Application.Abstractions.Messaging;
using Application.FeeServices.Response;
using Domain.Abstractions;
using Domain.FreeServices;

namespace Application.FeeServices.GetAllPaged;

public record GetAllFeeAndServicePagedCommand() : PagedQuery<FeeService, FeeServiceId>,
    ICommand<PagedList<FeeAndServiceResponse>>;