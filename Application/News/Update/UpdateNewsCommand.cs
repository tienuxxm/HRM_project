using Application.Abstractions.Messaging;
using Domain.News;
using Domain.Shared;

namespace Application.News.Update;

public sealed record UpdateNewsCommand(NewsId NewsId, Content Content, Title Title,
    Description Description,
    ImageUrl Thumbnail) : ICommand;