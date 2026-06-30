using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Jobs.YearEndJob;

public class YearEndJobCommandHandler : ICommandHandler<YearEndJobCommand>
{
    private const int MembershipGoldLevel = 2;
    private const int MembershipDiamondLevel = 3;

    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public YearEndJobCommandHandler(IMemberRepository memberRepository,
        IMembershipClassRepository membershipClassRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _membershipClassRepository = membershipClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(YearEndJobCommand request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetEntitiesAsQueryable()
            .Include(x => x.MembershipClass)
            .Include(x => x.MemberPointHistories)
            .Include(x => x.Orders).Where(x =>
                x.MembershipClass != null && x.MembershipClass.Level == new Level(MembershipDiamondLevel))
            .ToListAsync(cancellationToken);

        var goldClass = await _membershipClassRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(x => x.Level == new Level(MembershipGoldLevel), cancellationToken);

        var currentYear = DateTime.Now.Year;
        for (var i = 0; i < members.Count(); i++)
        {
            var member = members[i];
            var memberPoint = 0;
            var memberTotalBill = Money.Zero();

            if (member.Orders.Any())
                memberTotalBill = member.Orders.Where(x =>
                        x is { HasPayment: true, PaymentDate: not null } && x.PaymentDate.Value.Year == currentYear)
                    .Select(x => x.TotalBill).Aggregate((x, y) => x + y);

            if (member.MemberPointHistories != null)
                memberPoint = member.MemberPointHistories.Where(x => x.CreatedDate.Year == currentYear)
                    .Sum(x => x.MemberPoint.Value);
            if (memberPoint < 50000 && memberTotalBill.Amount < 50000000)
            {
                member.AssignMembershipClass(goldClass);
                _memberRepository.Update(member);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}