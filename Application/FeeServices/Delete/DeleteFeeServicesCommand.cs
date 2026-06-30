using Domain.FreeServices;
using ICommand = Application.Abstractions.Messaging.ICommand;

namespace Application.FeeServices.Delete;

public sealed record DeleteFeeServicesCommand(FeeServiceId Id) : ICommand;