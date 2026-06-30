using Application.Abstractions.Messaging;
using Application.FeeServices.Response;
using Domain.FreeServices;

namespace Application.FeeServices.GetOne;

public sealed record GetOneFeeServicesCommand(FeeServiceId FeeServiceId) : ICommand<FeeAndServiceResponse>;