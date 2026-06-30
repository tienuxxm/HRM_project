using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberActivities;
using Domain.Shared;

namespace Application.MemberActivites;

public class CreateManyMemberActivityCommandHandler : ICommandHandler<CreateManyMemberActivityCommand>
{
    private readonly IMemberActivityRepository _memberActivityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateManyMemberActivityCommandHandler(IMemberActivityRepository memberActivityRepository,
        IUnitOfWork unitOfWork)
    {
        _memberActivityRepository = memberActivityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateManyMemberActivityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var logs = request.Activities.Select(a => MemberActivity.Create(a.Type, new LogMessage(a.Message)))
                .ToList();
            _memberActivityRepository.AddRange(logs);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(Error.None);
        }
    }
}