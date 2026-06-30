using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MembershipClasses;

namespace Application.MembershipClasses.Deactivate;

internal sealed class DeactiveMembershipClassCommandHandler : ICommandHandler<DeactivateMembershipClassCommand>
{
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactiveMembershipClassCommandHandler(IMembershipClassRepository membershipClassRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipClassRepository = membershipClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateMembershipClassCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var membershipClass = await _membershipClassRepository.GetByIdAsync(request.Id, cancellationToken);
            if (membershipClass is null)
                return Result.Failure(MembershipClassErrors.NotFound);
            membershipClass.Deactive();
            _membershipClassRepository.Update(membershipClass);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(MembershipClassErrors.DeleteFail);
        }
    }
}