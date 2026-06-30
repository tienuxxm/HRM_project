using Application.Abstractions.Messaging;
using Domain.FreeServices;
using Domain.News;
using Domain.Shared;

namespace Application.FeeServices.Update;

public sealed record UpdateFeeServicesCommand(
        Guid id, 
        string FeeName ,
        decimal? FeeAmount ,
        float? FeePercent ,
        bool IsPercent ,
        FeeType FeeType ,
        bool IsActive 
 ) : ICommand;
