using Application.Abstractions.Messaging;
using Application.Products.Delete;
using Domain.Abstractions;
using Domain.Partners;
using Domain.Products;

namespace Application.Partners.Delete;

internal sealed class DeletePartnerCommandHandler : ICommandHandler<DeletePartnerCommand>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePartnerCommandHandler(IPartnerRepository partnerRepository, IUnitOfWork unitOfWork)
    {
        _partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await _partnerRepository.GetByIdAsync(request.PartnerId);
        if (partner is null)
        {
            return Result.Failure<Guid>(PartnerErrors.NotFound);
        }

        _partnerRepository.Remove(partner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}