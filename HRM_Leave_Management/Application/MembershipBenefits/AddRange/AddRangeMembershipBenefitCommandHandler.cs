using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MembershipBenefits;
using Domain.MembershipClasses;

namespace Application.MembershipBenefits.AddRange;

public class AddRangeMembershipBenefitCommandHandler : ICommandHandler<AddRangeMembershipBenefitCommand>
{
    private readonly IMembershipBenefitRepository _membershipBenefitRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRangeMembershipBenefitCommandHandler(IMembershipBenefitRepository membershipBenefitRepository,
        IUnitOfWork unitOfWork, IMembershipClassRepository membershipClassRepository)
    {
        _unitOfWork = unitOfWork;
        _membershipClassRepository = membershipClassRepository;
        _membershipBenefitRepository = membershipBenefitRepository;
    }

    public async Task<Result> Handle(AddRangeMembershipBenefitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var membershipClass =
                await _membershipClassRepository.GetByIdAsync(request.MembershipClassId, cancellationToken);
            if (membershipClass is null)
                return Result.Failure(MembershipClassErrors.NotFound);
            var listBenefit = request.AddMembershipBenefits.Select(r =>
                MembershipBenefit.Create(request.MembershipClassId, r.Description, r.Title)).ToList();
            _membershipBenefitRepository.AddRange(listBenefit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("Create.Fail", "Fail to create benefit"));
        }
    }
}