using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Extensions;
using Domain.Abstractions;
using Domain.News;

namespace Application.News.GetOne;

public class GetOneNewsCommandHandler : ICommandHandler<GetOneNewsCommand, NewsResponse>
{
    private readonly INewsRepository _newsRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetOneNewsCommandHandler(INewsRepository newsRepository, IAwsS3Service awsS3Service)
    {
        _newsRepository = newsRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<NewsResponse>> Handle(GetOneNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await _newsRepository.GetByIdAsync(request.NewsId);
        if (news is null)
            return Result.Failure<NewsResponse>(NewsError.NotFound);
        var imageUuid = news.Content.Value.ExtractUUIDs();
        var imageUrls = imageUuid.ToDictionary(k => k, v => _awsS3Service.GetUrlPresign(v.ToString()));
        var content = news.Content.Value;
        /*foreach (var keyValuePair in imageUrls)
        {
            content = content.Replace(keyValuePair.Key.ToString(), keyValuePair.Value);
        }*/

        var newsResponse = new NewsResponse()
        {
            Content = content,
            Description = news.Description.Value,
            Id = news.Id.Value,
            Thumbnail = _awsS3Service.GetUrlPresign(news.Thumbnail.Value),
            ThumbNailId = news.Thumbnail.Value,
            Title = news.Title.Value,
        };
        return Result.Success(newsResponse);
    }
}