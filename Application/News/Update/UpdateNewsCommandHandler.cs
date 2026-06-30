using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.News;

namespace Application.News.Update;

public class UpdateNewsCommandHandler : ICommandHandler<UpdateNewsCommand>
{
    private readonly INewsRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNewsCommandHandler(INewsRepository newsRepository,
        IUnitOfWork unitOfWork)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await _newsRepository.GetByIdAsync(request.NewsId, cancellationToken);
        if (news is null)
            return Result.Failure(NewsError.NotFound);
        news.Update(request.Content, request.Title, request.Description, request.Thumbnail);
        _newsRepository.Update(news);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}