using Application.Abstractions.Messaging;
using Domain.News;
using Domain.Shared;

namespace Application.News.Create;

public sealed record CreateNewsCommand(
    Content Content, 
    Title Title,
    Description Description,
    ImageUrl Thumbnail) : ICommand<Guid>;