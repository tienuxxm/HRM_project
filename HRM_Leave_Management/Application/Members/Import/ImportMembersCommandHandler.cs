using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberPointHistories;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.Shared;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Email = Domain.Members.Email;
using PhoneNumber = Domain.Members.PhoneNumber;

namespace Application.Members.Import;

internal class MemberRequest
{
    public string Gender { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string FullName { get; set; }
    public string CardNumber { get; set; }
    public int Point { get; set; }
    public string MembershipClass { get; set; }
    public DateTime? RegisterDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Note { get; set; }
}

public class ImportMembersCommandHandler : ICommandHandler<ImportMembersCommand>
{
    private const int MembershipDefaultLevel = 0;
    private const int MembershipSilverLevel = 1;
    private const int MembershipGoldLevel = 2;
    private const int MembershipDiamondLevel = 3;
    private readonly IMemberPointHistoryRepository _memberPointHistoryRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportMembersCommandHandler(IMemberRepository memberRepository, IUnitOfWork unitOfWork,
        IMembershipClassRepository membershipClassRepository,
        IMemberPointHistoryRepository memberPointHistoryRepository)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _membershipClassRepository = membershipClassRepository;
        _memberPointHistoryRepository = memberPointHistoryRepository;
    }

    public async Task<Result> Handle(ImportMembersCommand request, CancellationToken cancellationToken)
    {
        using (var reader = ExcelReaderFactory.CreateReader(request.Stream))
        {
            var memberList = new List<MemberRequest>();
            do
            {
                reader.Read();

                while (reader.Read())
                    try
                    {
                        var email = reader.GetString(6);
                        var address = reader.GetString(3);
                        var phone = reader.GetString(5);
                        var member = new MemberRequest
                        {
                            FullName = reader.GetString(0),
                            BirthDate = reader.GetDateTime(2),
                            Address = string.IsNullOrEmpty(address) ? "TP.Hồ Chí Minh" : address,
                            PhoneNumber = reader.GetString(5),
                            Email = string.IsNullOrEmpty(email) ? phone + "@wnz_random.com" : email,
                            MembershipClass = reader.GetString(8),
                            Point = (int)reader.GetDouble(9),
                            Note = reader.GetString(10)
                        };
                        if (string.IsNullOrEmpty(member.FullName)) continue;


                        memberList.Add(member);
                    }
                    catch (Exception)
                    {
                        //Ignore
                    }
            } while (reader.NextResult());


            var nonDuplicateEmail = memberList.GroupBy(x => x.Email)
                .Select(x => x.First())
                .ToList();
            var filteredList = nonDuplicateEmail.GroupBy(x => x.PhoneNumber)
                .Select(x => x.First())
                .ToList();

            var duplicateEmails = _memberRepository.GetEntitiesAsQueryable()
                .AsNoTracking()
                .AsEnumerable()
                .Where(x => filteredList.Select(r => r.Email).Contains(x.Email.Value))
                .Select(x => x.Email)
                .ToList();

            var duplicatePhone = _memberRepository.GetEntitiesAsQueryable()
                .AsNoTracking()
                .AsEnumerable()
                .Where(x => filteredList.Select(r => r.PhoneNumber).Contains(x.PhoneNumber.Value))
                .Select(x => x.Email)
                .ToList();

            var nonDuplicateEmailSystem = filteredList
                .Where(i => !duplicateEmails.Select(x => x.Value).ToList().Contains(i.Email))
                .ToList();

            var finalList = nonDuplicateEmailSystem
                .Where(i => !duplicatePhone.Select(x => x.Value).ToList().Contains(i.PhoneNumber)).ToList();

            try
            {
                var nextMemberCode = await _memberRepository.GetNextMemberCode(cancellationToken);
                var defaultMembershipList = await _membershipClassRepository.GetAll(cancellationToken);

                finalList.ForEach(x =>
                {
                    var nameSplit = x.FullName.Split(' ').ToList();
                    var firstName = nameSplit.First();
                    nameSplit.RemoveAt(0);
                    var lastName = string.Join(' ', nameSplit);
                    var newMember = Member.Create(new Code(nextMemberCode), new FirstName(firstName),
                        new LastName(lastName),
                        new Email(x.Email), new PhoneNumber(x.PhoneNumber), new Address(x.Address), DateTime.UtcNow,
                        x.BirthDate.HasValue ? x.BirthDate.Value.ToUniversalTime() : null,
                        null, RegisterType.SYSTEM, x.Note);
                    var membershipClass = GetMembershipClass(x.MembershipClass, defaultMembershipList!);
                    if (membershipClass is not null)
                    {
                        newMember.AssignMembershipClass(membershipClass);
                    }
                    var code = nextMemberCode.Remove(0, 2);
                    nextMemberCode = "KH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');
                    var memberPointHistory = MemberPointHistory.Create(
                        newMember.Id,
                        new MemberPoint(x.Point),
                        PointType.ADDED,
                        new Title("Tích điểm đồng bộ hệ thống"),
                        DateTime.UtcNow);
                    _memberPointHistoryRepository.Add(memberPointHistory);
                    _memberRepository.Add(newMember);
                });

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                //Ignore
            }

            return Result.Success();
        }
    }

    private MembershipClass? GetMembershipClass(string className, List<MembershipClass> defaultMembershipList)
    {
        return className.Trim().ToLower() switch
        {
            // "member" => defaultMembershipList!.First(x => x.Level.Value == MembershipDefaultLevel),
            // "vip" => defaultMembershipList!.First(x => x.Level.Value == MembershipSilverLevel),
            // "gold" => defaultMembershipList!.First(x => x.Level.Value == MembershipGoldLevel),
            // "diamond" => defaultMembershipList!.First(x => x.Level.Value == MembershipDiamondLevel),
            // _ => defaultMembershipList!.First(x => x.Level.Value == MembershipDefaultLevel)
            "member" => defaultMembershipList!.First(x => x.Level.Value == MembershipSilverLevel),
            "vip" => defaultMembershipList!.First(x => x.Level.Value == MembershipDiamondLevel),
            // "gold" => defaultMembershipList!.First(x => x.Level.Value == MembershipGoldLevel),
            // "diamond" => defaultMembershipList!.First(x => x.Level.Value == MembershipDiamondLevel),
            _ => null
        };
    }
}