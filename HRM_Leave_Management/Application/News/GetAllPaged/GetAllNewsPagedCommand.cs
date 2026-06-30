using Application.Abstractions.Messaging;
using Application.News.GetOne;
using Domain.Abstractions;
using Domain.News;

namespace Application.News.GetAllPaged;

public sealed record GetAllNewsPagedCommand() : PagedQuery<Domain.News.News, NewsId>, ICommand<PagedList<NewsResponse>>;