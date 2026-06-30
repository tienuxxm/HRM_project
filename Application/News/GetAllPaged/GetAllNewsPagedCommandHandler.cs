using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.News.GetOne;
using Domain.Abstractions;
using Domain.News;

namespace Application.News.GetAllPaged;

internal sealed class GetAllNewsPagedCommandHandler : ICommandHandler<GetAllNewsPagedCommand, PagedList<NewsResponse>>
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly INewsRepository _newsRepository;

    public GetAllNewsPagedCommandHandler(
        INewsRepository newsRepository,
        IAwsS3Service awsS3Service
    )
    {
        _awsS3Service = awsS3Service;
        _newsRepository = newsRepository;
    }

    public async Task<Result<PagedList<NewsResponse>>> Handle(GetAllNewsPagedCommand request,
        CancellationToken cancellationToken)
    {
        var query = _newsRepository.GetEntitiesAsQueryable()
            .OrderByDescending(x => x.CreatedDate).AsQueryable();
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.AsEnumerable()
                .Where(x => x.Title.Value.ToLower().Contains(request.SearchTerm.ToLower()) ||
                            x.CreatedDate.ToString("dd/MM/yyyy").Contains(request.SearchTerm))
                .AsQueryable();

        var newsPaged = await _newsRepository.GetAllPaged(request, query);

        var newsResponseItems = newsPaged.Data.Select(n => new NewsResponse
        {
            Content = n.Content.Value,
            Description = n.Description.Value,
            Title = n.Title.Value,
            Id = n.Id.Value,
            Thumbnail = _awsS3Service.GetUrlPresign(n.Thumbnail.Value),
            CreatedDate = n.CreatedDate
        }).ToList();
        var newsResponse = new PagedList<NewsResponse>(newsResponseItems, newsPaged.TotalCount, newsPaged.CurrentPage,
            newsPaged.PageSize);
        return Result.Success(newsResponse);
    }
}