using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Products;

namespace Application.Products.Delete;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, BooleanResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null) return Result.Failure<BooleanResponse>(ProductErrors.NotFound);
        product.Delete();
        _productRepository.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BooleanResponse { Result = true });
    }
}