using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.News;

namespace Application.News.Delete;

internal sealed class DeleteNewsCommandHandler : ICommandHandler<DeleteNewsCommand>
{
    private readonly INewsRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNewsCommandHandler(INewsRepository newsRepository, IUnitOfWork unitOfWork)
    {
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
    {
        var news = await _newsRepository.GetByIdAsync(request.Id, cancellationToken);
        if (news is null)
            return Result.Failure(NewsError.NotFound);
        _newsRepository.Remove(news);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}