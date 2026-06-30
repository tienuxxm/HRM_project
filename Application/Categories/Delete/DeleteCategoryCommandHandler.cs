using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Categories;

namespace Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, BooleanResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.Id));
        if (category is null)
            return Result.Failure<BooleanResponse>(CategoryErrors.NotFound);
        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BooleanResponse
            { Result = true, Message = $"{request.Id} DELETED" });
    }
}