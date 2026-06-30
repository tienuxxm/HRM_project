using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Abstractions.Sms;
using Domain.Abstractions;
using Domain.Members;
using Domain.PhoneValidationCheck;
using PhoneNumber = Domain.PhoneValidationCheck.PhoneNumber;

namespace Application.Sms.SendValidationCode;

public class SendValidationCodeCommandHandler : ICommandHandler<SendValidationCodeCommand, string>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemberRepository _memberRepository;
    private readonly IPhoneValidationCheckRepository _phoneValidationCheckRepository;
    private readonly ISmsService _smsService;
    private readonly IUnitOfWork _unitOfWork;

    public SendValidationCodeCommandHandler(IMemberRepository memberRepository, ISmsService smsService,
        IPhoneValidationCheckRepository phoneValidationCheckRepository, IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _smsService = smsService;
        _phoneValidationCheckRepository = phoneValidationCheckRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(SendValidationCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // var isPhoneNumberExisted =
            //     await _memberRepository.IsExistedAsync(x => x.PhoneNumber == new PhoneNumber(request.PhoneNumber),
            //         cancellationToken);
            // if (isPhoneNumberExisted)
            //     return Result.Failure<string>(MemberErrors.PhoneNumberExisted);
            var phoneCheck =
                await _phoneValidationCheckRepository.GetByPhoneNumber(
                    new PhoneNumber(request.PhoneNumber), cancellationToken);
            if (phoneCheck is null)
            {
                var createPhoneCheck = PhoneValidationCheck.SendCode(
                    new PhoneNumber(request.PhoneNumber), _dateTimeProvider.UtcNow);
                _phoneValidationCheckRepository.Add(createPhoneCheck);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _smsService.SendMessageCode(createPhoneCheck.Code.Value, request.PhoneNumber);
                return Result.Success(createPhoneCheck.Code.Value);
            }

            var result = phoneCheck.ResendCode(_dateTimeProvider.UtcNow);
            if (result.IsFailure)
                return Result.Failure<string>(result.Error);
            _phoneValidationCheckRepository.Update(phoneCheck);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var sendResult = await _smsService.SendMessageCode(phoneCheck.Code.Value, request.PhoneNumber);
            if (sendResult.IsSuccess)
                return Result.Success(phoneCheck.Code.Value);
            return Result.Failure<string>(sendResult.Error);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}