using Domain.Abstractions;
using Domain.Shared;
using Domain.Vouchers;

namespace Domain.Partners;

public sealed class Partner : Entity<PartnerId>
{
    public PartnerName PartnerName { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public PartnerAddress? Address { get; private set; }
    public Email? Email { get; private set; }
    public List<Voucher>? Vouchers { get; private set; }
    public ImageUrl? QrCode { get; private set; }
    public string? QrCodeId { get; private set; }
    public bool? IsDelete { get; private set; } = false;

    public void SetQrCodeImage(ImageUrl qrCode)
    {
        QrCode = qrCode;
    }

    public void SetQrCodeId(string id)
    {
        QrCodeId = id;
    }

    public void Delete()
    {
        IsDelete = true;
    }

    private Partner(PartnerId id, PartnerName partnerName, PartnerAddress partnerAddress, PhoneNumber phoneNumber,
        Email email, DateTime createdDate)
    {
        Id = id;
        PartnerName = partnerName;
        CreatedDate = createdDate;
        Address = partnerAddress;
        PhoneNumber = phoneNumber;
        Email = email;
    }

    private Partner()
    {
    }


    public static Partner Create(PartnerName partnerName, PartnerAddress partnerAddress, PhoneNumber phoneNumber,
        Email email, DateTime createdDate)
    {
        var partner = new Partner(PartnerId.New(), partnerName, partnerAddress, phoneNumber, email, createdDate);
        return partner;
    }


    public void Update(PartnerName? partnerName, PartnerAddress? address, PhoneNumber? phoneNumber, Email? email,
        List<Voucher>? vouchers)
    {
        PartnerName = partnerName ?? PartnerName;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        Vouchers = vouchers;
    }
}