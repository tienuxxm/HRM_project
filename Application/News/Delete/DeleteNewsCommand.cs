using Domain.News;
using ICommand = Application.Abstractions.Messaging.ICommand;

namespace Application.News.Delete;

public sealed record DeleteNewsCommand(NewsId Id) : ICommand;