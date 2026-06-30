using Application.Abstractions.Messaging;
using Domain.News;

namespace Application.News.GetOne;

public sealed record GetOneNewsCommand(NewsId NewsId) : ICommand<NewsResponse>;