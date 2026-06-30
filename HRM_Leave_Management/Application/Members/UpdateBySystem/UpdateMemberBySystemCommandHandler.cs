using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Districts;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.UpdateBySystem;

public class UpdateMemberBySystemCommandHandler : ICommandHandler<UpdateMemberBySystemCommand>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberBySystemCommandHandler(IMemberRepository memberRepository, IUnitOfWork unitOfWork,
        IAuthenticationService authenticationService)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<Result> Handle(UpdateMemberBySystemCommand request, CancellationToken cancellationToken)
    {
        var memberResult = await _memberRepository.GetByIdAsync(request.Id, cancellationToken);
        if (memberResult is null) return Result.Failure(MemberErrors.NotFound);

        if (!string.IsNullOrEmpty(request.Email) && request.Email != memberResult.Email.Value)
        {
            var memberExistedWithNewEmail =
                await _memberRepository.IsEmailExisted(new Email(request.Email), request.Id, cancellationToken);
            if (memberExistedWithNewEmail)
                return Result.Failure(MemberErrors.EmailExisted);
            if (memberResult.RegisterType.HasValue && memberResult.RegisterType != RegisterType.SYSTEM &&
                !string.IsNullOrEmpty(memberResult.IdentityId))
            {
                var changeEmailResult =
                    await _authenticationService.ChangeEmail(request.Email, memberResult.IdentityId, cancellationToken);
                if (changeEmailResult.IsFailure)
                    return changeEmailResult;
            }
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

        var result = memberResult.Update(
            !string.IsNullOrEmpty(request.FirstName) ? new FirstName(request.FirstName) : null,
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

        _memberRepository.Update(memberResult);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}