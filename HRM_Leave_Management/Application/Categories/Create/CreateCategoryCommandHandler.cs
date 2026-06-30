using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Categories;
using Domain.Shared;

namespace Application.Categories.Create;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, BooleanResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, ICategoryRepository categoryRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var isCategoryExisted =
            await _categoryRepository.IsExistedAsync(x => x.CategoryName == new CategoryName(request.Name));
        if (isCategoryExisted)
            return Result.Failure<BooleanResponse>(CategoryErrors.CategoryExisted);
        var category = Category.Create(new CategoryName(request.Name), new Description(request.Description),
            _dateTimeProvider.UtcNow, request.Index);
        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse()
            { Result = true, Message = $"Category: {request.Name} has been created" });
    }
}