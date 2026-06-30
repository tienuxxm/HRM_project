using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MembershipBenefits;
using Domain.MembershipClasses;
using Microsoft.EntityFrameworkCore;

namespace Application.MembershipClasses.Update;

public class UpdateMembershipClassCommandHandler : ICommandHandler<UpdateMembershipClassCommand>
{
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IMembershipBenefitRepository _membershipBenefitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMembershipClassCommandHandler(IMembershipClassRepository membershipClassRepository,
        IUnitOfWork unitOfWork, IMembershipBenefitRepository membershipBenefitRepository)
    {
        _membershipClassRepository = membershipClassRepository;
        _unitOfWork = unitOfWork;
        _membershipBenefitRepository = membershipBenefitRepository;
    }

    public async Task<Result> Handle(UpdateMembershipClassCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var membershipClass = await _membershipClassRepository.GetEntitiesAsQueryable()
                .Include(x => x.MembershipBenefits)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.MembershipClassId), cancellationToken);
            if (membershipClass is null)
                return Result.Failure(MembershipClassErrors.NotFound);
            if (membershipClass.ClassName.Value != request.ClassName.Value)
            {
                var isMembershipClassExisted =
                    await _membershipClassRepository.IsExistedAsync(m => m.ClassName == request.ClassName);
                if (isMembershipClassExisted)
                    return Result.Failure(MembershipClassErrors.Existed);
            }

            var updateResult = membershipClass.Update(
                request.ClassName,
                request.Level,
                request.MaxMoney,
                request.PercentDefault,
                request.PercentBirthDate, request.effectiveYears);
            var benefits = request.Benefits
                .Select(b => MembershipBenefit.Create(membershipClass.Id, b.Description, b.Title))
                .ToList();
            membershipClass.SetBenefits(benefits);
            _membershipClassRepository.Update(membershipClass);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return updateResult;
        }
        catch (Exception e)
        {
            return Result.Failure(MembershipClassErrors.UpdateFail);
        }
    }
}