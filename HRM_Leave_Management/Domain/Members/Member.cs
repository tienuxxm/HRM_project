using Domain.Abstractions;
using Domain.Districts;
using Domain.MemberPointHistories;
using Domain.Members.Events;
using Domain.MembershipClasses;
using Domain.MemberVouchers;
using Domain.Orders;
using Domain.Shared;
using Domain.Vouchers;

namespace Domain.Members;

public class Member : Entity<MemberId>, IComparable
{
    private Member(MemberId id, Code memberCode, FirstName firstName, LastName lastName, Email email,
        PhoneNumber phoneNumber,
        Address address, DateTime createdAt, DateTime? birthDate, RegisterType registerType, DistrictId? districtId,
        string? note)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        MemberCode = memberCode;
        CreatedAt = createdAt;
        IsActive = true;
        BirthDate = birthDate;
        RegisterType = registerType;
        DistrictId = districtId;
        Note = note;
    }

    private Member()
    {
    }


    public Email Email { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public District? District { get; }
    public DistrictId? DistrictId { get; private set; }

    public string FullName => FirstName.Value + " " + LastName.Value;
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    public MembershipClass? MembershipClass { get; private set; }
    public DateTime? MembershipAssignedDate { get; private set; }
    public MembershipClassId? MembershipClassId { get; private set; } = null;
    public ImageUrl? Avatar { get; private set; }
    public Code MemberCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public RegisterType? RegisterType { get; private set; }
    public string? MemberPlatformIdentityId { get; private set; }
    public bool IsActive { get; set; }
    public List<MemberVoucher> MemberVouchers { get; }
    public List<Order> Orders { get; }
    public string? Note { get; private set; }

    public DateTime? BirthDate { get; private set; }
    public DateTime? SendBirthDateNotificationDate { get; private set; }

    public List<MemberPointHistory>? MemberPointHistories { get; private set; }

    public string? IdentityId { get; private set; }

    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Member TValue => TValue.FullName.CompareTo(FullName),
            _ => 1
        };
    }

    public void SetAvatar(ImageUrl avatar)
    {
        Avatar = avatar;
    }

    public void SetSendBirthDateNotificationDate(DateTime dateTime)
    {
        SendBirthDateNotificationDate = dateTime;
    }

    public void SetMemberPlatformIdentityId(string id)
    {
        MemberPlatformIdentityId = id;
    }

    public void AddMemberPoint(MemberPointHistory memberPointHistory)
    {
        if (MemberPointHistories == null)
            MemberPointHistories = new List<MemberPointHistory> { memberPointHistory };
        else
            MemberPointHistories.Add(memberPointHistory);
    }

    public void Deactivate()
    {
        IsActive = false;
        IdentityId = null;
    }

    public static Member Create(Code memberCode, FirstName firstName, LastName lastName, Email email,
        PhoneNumber phoneNumber, Address address, DateTime createdAt, DateTime? birthDate, DistrictId? districtId,
        RegisterType registerType = 0, string? note = "")
    {
        var member = new Member(MemberId.New(), memberCode, firstName, lastName, email, phoneNumber, address,
            createdAt, birthDate, registerType, districtId, note);
        member.RaiseDomainEvent(new MemberCreatedDomainEvent(member.Id));
        return member;
    }

    public void RaiseCreateMemberEvent()
    {
        RaiseDomainEvent(new MemberCreatedDomainEvent(Id));
    }

    public void ClaimVoucher(MemberVoucher voucher)
    {
        MemberVouchers.Add(voucher);
    }

    public Result Update(FirstName? firstName, LastName? lastName, Address? address, PhoneNumber? phoneNumber,
        Email? email, DateTime? birthDate, DistrictId? districtId, string? note)
    {
        if (firstName != null)
            FirstName = firstName;
        if (lastName != null)
            LastName = lastName;
        if (address != null)
            Address = address;
        if (phoneNumber != null)
            PhoneNumber = phoneNumber;
        if (birthDate != null)
            BirthDate = birthDate;
        if (email != null)
            Email = email;
        if (districtId != null)
            DistrictId = districtId;
        Note = note;
        return Result.Success();
    }

    public void AddVoucher(List<VoucherId> voucherIds)
    {
        var memberVoucher = voucherIds.Select(v => MemberVoucher.Create(Id, v)).ToList();
        MemberVouchers.AddRange(memberVoucher);
    }

    public void AssignMembershipClass(MembershipClass? membershipClass)
    {
        MembershipClass = membershipClass;
        MembershipAssignedDate = DateTime.UtcNow.AddDays(-1);
        RaiseDomainEvent(new AssignedMembershipClassDomainEvent(Id));
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }
}