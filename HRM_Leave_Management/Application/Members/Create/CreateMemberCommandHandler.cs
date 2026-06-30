using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Create;

public class CreateMemberCommandHandler : ICommandHandler<CreateMemberCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMemberCommandHandler(IMemberRepository memberRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        // var isPhoneExisted = await _memberRepository.IsPhoneExisted(request.PhoneNumber, null, cancellationToken);
        // if (isPhoneExisted)
        //     return Result.Failure(MemberErrors.PhoneNumberExisted);
        // var isEmailExisted = await _memberRepository.IsEmailExisted(request.Email, null, cancellationToken);
        // if (isEmailExisted)
        //     return Result.Failure(MemberErrors.EmailExisted);
        var existedMember = await _memberRepository.GetEntitiesAsQueryable().FirstOrDefaultAsync(mem =>
                (mem.Email.Equals(request.Email) || mem.PhoneNumber.Equals(request.PhoneNumber)) && mem.IsActive,
            cancellationToken);
        var address = request.Address.Value;
        var birthDate = request.BirthDate;
        if (existedMember is not null)
        {
            if (!string.IsNullOrEmpty(existedMember?.IdentityId))
            {
                if (existedMember.Email.Equals(request.Email)) return Result.Failure(MemberErrors.EmailExisted);

                if (existedMember.PhoneNumber.Equals(request.PhoneNumber))
                    return Result.Failure(MemberErrors.PhoneNumberExisted);
            }

            if (string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(existedMember?.Address.Value))
                address = existedMember.Address.Value;

            if (!birthDate.HasValue && existedMember?.BirthDate is not null) birthDate = existedMember.BirthDate;
        }

        var nextMemberCode = await _memberRepository.GetNextMemberCode(cancellationToken);
        var member = Member.Create(new Code(nextMemberCode), request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, new Address(address), DateTime.UtcNow, birthDate, null);
        _memberRepository.Add(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}