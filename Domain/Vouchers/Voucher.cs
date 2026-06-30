using Domain.Abstractions;
using Domain.Partners;
using Domain.Shared;

namespace Domain.Vouchers;

public sealed class Voucher : Entity<VoucherId>
{
    private Voucher(
        VoucherId id,
        TitleVoucher titleVoucher,
        ImageUrl imageUrl,
        DateTime startedDate,
        DateTime endedDate,
        Place? place,
        int point,
        DateTime createdDate,
        PartnerId? partnerId,
        ContentVoucher? contentVoucher,
        Conditions? conditions,
        int? limitQuantity,
        int? discountValue,
        double? discountPercent,
        int? maxDiscountValue,
        int? minOrderValue,
        int? index,
        bool? isDefault,
        VoucherDefaultType? voucherDefaultType,
        string? members,
        string? memberships
    )
    {
        Id = id;
        CreatedDate = createdDate;
        Point = point;
        StartedDate = startedDate;
        EndedDate = endedDate;
        TitleVoucher = titleVoucher;
        PartnerId = partnerId;
        ImageUrl = imageUrl;
        Place = place;
        ContentVoucher = contentVoucher;
        IsVoucherDefault = isDefault;
        Status = VoucherStatus.Available;
        IsActive = true;
        LimitQuantity = limitQuantity;
        Conditions = conditions;
        DiscountPercent = discountPercent;
        DiscountValue = discountValue;
        MaxDiscountValue = maxDiscountValue;
        MinOrderValue = minOrderValue;
        IsDelete = false;
        Index = index;
        VoucherDefaultType = voucherDefaultType;
        Members = members;
        Memberships = memberships;
    }

    private Voucher()
    {
    }

    public TitleVoucher TitleVoucher { get; private set; }

    public int Point { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime StartedDate { get; private set; }

    public DateTime EndedDate { get; private set; }
    public Place? Place { get; set; }
    public string? Members { get; private set; }
    public string? Memberships { get; private set; }

    public PartnerId? PartnerId { get; private set; }
    public Partner? Partner { get; }
    public ImageUrl ImageUrl { get; private set; }
    public ImageUrl? QrCodeImageUrl { get; private set; }
    public int? LimitQuantity { get; private set; }
    public bool? IsVoucherDefault { get; private set; }
    public VoucherDefaultType? VoucherDefaultType { get; private set; }
    public VoucherStatus Status { get; private set; }
    public QrCode? QrCode { get; private set; }
    public string? QrCodeId { get; private set; }
    public Conditions? Conditions { get; private set; }
    public int? DiscountValue { get; private set; }
    public int? MaxDiscountValue { get; private set; }
    public double? DiscountPercent { get; private set; }
    public int? MinOrderValue { get; private set; }
    public int? Index { get; private set; }
    public bool? IsDelete { get; private set; }


    public bool IsActive { get; private set; }

    public bool? IsUserVoucher { get; private set; }
    public bool IsExpired => DateTime.UtcNow.Date > EndedDate.Date;

    public ContentVoucher? ContentVoucher { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetQrCodeId(string id)
    {
        QrCodeId = id;
    }

    public void SetQrCode(ImageUrl imageUrl, QrCode qrCode)
    {
        QrCodeImageUrl = imageUrl;
        QrCode = qrCode;
    }

    public void SetUserVoucher()
    {
        IsUserVoucher = true;
    }

    public void SetMembers(string members)
    {
        Members = members;
    }

    public void SetMemberClasses(string memberClasses)
    {
        Memberships = memberClasses;
    }

    public static Voucher Create(
        TitleVoucher titleVoucher,
        ImageUrl imageUrl,
        DateTime startedDate,
        DateTime endedDate,
        Place? place,
        int point,
        DateTime createdDate,
        PartnerId? partnerId,
        ContentVoucher? contentVoucher,
        Conditions? conditions,
        int? limitQuantity,
        int? discountValue,
        double? discountPercent,
        int? maxDiscountValue,
        int? minOrderValue,
        int? index,
        bool? isDefault,
        VoucherDefaultType? voucherDefaultType,
        string? members,
        string? memberships
    )
    {
        return new Voucher(
            VoucherId.New(),
            titleVoucher,
            imageUrl,
            startedDate,
            endedDate,
            place,
            point,
            createdDate,
            partnerId,
            contentVoucher,
            conditions,
            limitQuantity,
            discountValue,
            discountPercent,
            maxDiscountValue,
            minOrderValue,
            index,
            isDefault,
            voucherDefaultType,
            members,
            memberships
        );
    }

    public void DescreaseQuantity()
    {
        if (LimitQuantity.HasValue) LimitQuantity--;
    }

    public static Voucher Clone(Voucher voucher)
    {
        var newVoucher = Create(voucher.TitleVoucher, voucher.ImageUrl, voucher.StartedDate, voucher.EndedDate,
            voucher.Place, voucher.Point, DateTime.UtcNow, voucher.PartnerId,
            voucher.ContentVoucher, voucher.Conditions, voucher.LimitQuantity, voucher.DiscountValue,
            voucher.DiscountPercent, voucher.MaxDiscountValue, voucher.MinOrderValue,
            voucher.Index, voucher.IsVoucherDefault, voucher.VoucherDefaultType, voucher.Members, voucher.Memberships);
        return newVoucher;
    }

    public void SetDefaultVoucher(DateTime startDate, DateTime endDate, string content, string title, double? discount)
    {
        StartedDate = startDate;
        EndedDate = endDate;
        ContentVoucher = new ContentVoucher(content);
        TitleVoucher = new TitleVoucher(title);
        DiscountPercent = discount;
    }


    public void Update(
        TitleVoucher? titleVoucher,
        ImageUrl? imageUrl,
        DateTime? startedDate,
        DateTime? endedDate,
        Place? place,
        int? point,
        PartnerId? partnerId,
        ContentVoucher? contentVoucher,
        Conditions? conditions,
        int? limitQuantity,
        int? discountValue,
        double? discountPercent,
        int? maxDiscountValue,
        int? minOrderValue,
        int? index,
        VoucherDefaultType? voucherDefaultType
    )
    {
        TitleVoucher = titleVoucher ?? TitleVoucher;
        ImageUrl = imageUrl ?? ImageUrl;
        StartedDate = startedDate ?? StartedDate;
        EndedDate = endedDate ?? EndedDate;
        Place = place ?? Place;
        Point = point ?? Point;
        PartnerId = partnerId ?? PartnerId;
        ContentVoucher = contentVoucher ?? ContentVoucher;
        LimitQuantity = limitQuantity;
        Conditions = conditions;
        DiscountValue = discountValue;
        DiscountPercent = discountPercent;
        MaxDiscountValue = maxDiscountValue;
        MinOrderValue = minOrderValue;
        Index = index;
        VoucherDefaultType = voucherDefaultType;
    }
}