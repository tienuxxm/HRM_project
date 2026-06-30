using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Districts;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Update;

public class UpdateMemberCommandHandler : ICommandHandler<UpdateMemberCommand>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberCommandHandler(IMemberContext memberContext, IMemberRepository memberRepository,
        IAuthenticationService authenticationService, IUnitOfWork unitOfWork)
    {
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _authenticationService = authenticationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var memberId = _memberContext.IdentityId;
        var memberResult = await _memberRepository.GetByIdentityAsync(memberId, cancellationToken);
        if (memberResult is null) return Result.Failure(MemberErrors.NotFound);

        if (!string.IsNullOrEmpty(request.Email) && request.Email != memberResult.Email.Value)
        {
            var memberExistedWithNewEmail = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(x => x.Email.Equals(new Email(request.Email)) && !x.Id.Equals(memberResult.Id),
                    cancellationToken);
            if (memberExistedWithNewEmail is not null)
                return Result.Failure(MemberErrors.EmailExisted);
            var changeEmailResult =
                await _authenticationService.ChangeEmail(request.Email, memberId, cancellationToken);
            if (changeEmailResult.IsFailure)
                return changeEmailResult;
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != memberResult.PhoneNumber.Value)
        {
            var memberExistedWithNewPhone = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(
                    x => x.PhoneNumber.Equals(new PhoneNumber(request.PhoneNumber)) && !x.Id.Equals(memberResult.Id),
                    cancellationToken);
            if (memberExistedWithNewPhone is not null)
                return Result.Failure(MemberErrors.PhoneNumberExisted);
        }

        var member = await _memberRepository.GetByIdentityAsync(memberId, cancellationToken);
        if (member is null)
            return Result.Failure(MemberErrors.NotFound);

        var result = member.Update(!string.IsNullOrEmpty(request.FirstName) ? new FirstName(request.FirstName) : null,
            !string.IsNullOrEmpty(request.LastName) ? new LastName(request.LastName) : null,
            !string.IsNullOrEmpty(request.Address) ? new Address(request.Address) : null,
            !string.IsNullOrEmpty(request.PhoneNumber) ? new PhoneNumber(request.PhoneNumber) : null,
            !string.IsNullOrEmpty(request.Email) ? new Email(request.Email) : null,
            request.BirthDate,
            request.DistrictId.HasValue ? new DistrictId(request.DistrictId.Value) : null,
            request.Note
        );
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _memberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}