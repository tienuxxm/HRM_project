using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.News;

namespace Application.News.Create;

public class CreateNewsCommandHandler : ICommandHandler<CreateNewsCommand, Guid>
{
    private readonly INewsRepository _newsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNewsCommandHandler(INewsRepository newsRepository,
        IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        var news = Domain.News.News.Create(
            request.Content,
            request.Title,
            request.Description,
            request.Thumbnail,
            _dateTimeProvider.UtcNow);
        _newsRepository.Add(news);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(news.Id.Value);
    }
}