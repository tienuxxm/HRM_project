using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MembershipClasses;

namespace Application.MembershipClasses.Delete;

public class DeleteMembershipClassCommandHandler : ICommandHandler<DeleteMembershipClassCommand>
{
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMembershipClassCommandHandler(IMembershipClassRepository membershipClassRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipClassRepository = membershipClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteMembershipClassCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var membershipClass = await _membershipClassRepository.GetByIdAsync(request.Id, cancellationToken);
            if (membershipClass is null)
                return Result.Failure(MembershipClassErrors.NotFound);
            _membershipClassRepository.Remove(membershipClass);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(MembershipClassErrors.DeleteFail);
        }
    }
}