using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MembershipBenefits;
using Domain.MembershipClasses;
using Domain.Shared;

namespace Application.MembershipClasses.Create;

public class CreateMembershipClassCommandHandler : ICommandHandler<CreateMembershipClassCommand, Guid>
{
    private readonly IMembershipClassRepository _membershipClassRepository;

    private readonly IUnitOfWork _unitOfWork;

    public CreateMembershipClassCommandHandler(IMembershipClassRepository membershipClassRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipClassRepository = membershipClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMembershipClassCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var membershipClassExisted =
                await _membershipClassRepository.IsExistedAsync(m => m.ClassName == request.ClassName,
                    cancellationToken);
            if (membershipClassExisted)
                return Result.Failure<Guid>(MembershipClassErrors.Existed);
            var membershipClassLevelExisted =
                await _membershipClassRepository.IsExistedAsync(m => m.Level == request.Level,
                    cancellationToken);
            if (membershipClassLevelExisted)
                return Result.Failure<Guid>(MembershipClassErrors.LevelExisted);
            var membershipClass = MembershipClass.Create(request.ClassName, request.Level, request.MaxMoney,
                request.PercentDefault, request.PercentBirthDate, request.effectiveYears);
            var benefits =
                request.Benefits.Select(b => MembershipBenefit.Create(membershipClass.Id, b.Description, b.Title))
                    .ToList();
            membershipClass.SetBenefits(benefits);
            _membershipClassRepository.Add(membershipClass);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(membershipClass.Id.Value);
        }
        catch (Exception e)
        {
            return Result.Failure<Guid>(MembershipClassErrors.CreateFail);
        }
    }
}