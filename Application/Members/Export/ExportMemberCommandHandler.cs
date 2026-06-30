using System.Drawing;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.MemberPointHistories;
using Domain.Members;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Application.Members.Export;

public class ExportMemberCommandHandler : ICommandHandler<ExportMemberCommand, byte[]>
{
    private readonly IMemberRepository _memberRepository;

    public ExportMemberCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<byte[]>> Handle(ExportMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var memberData = (await _memberRepository.GetEntitiesAsQueryable()
                    .Include(x => x.Orders)
                    .Include(x => x.MembershipClass)
                    .Include(x => x.MemberPointHistories)
                    .Include(x => x.MemberVouchers)
                    .ThenInclude(x => x.Voucher)
                    .Include(x => x.District)
                    .ThenInclude(x => x.Province).ToListAsync(cancellationToken))
                .Select(member => new
                {
                    Email = member.Email.Value,
                    Id = member.Id.Value,
                    FirstName = member.FirstName.Value,
                    LastName = member.LastName.Value,
                    Address = member.Address.Value,
                    PhoneNumber = member.PhoneNumber.Value,
                    MemberCode = member.MemberCode.Value,
                    member.BirthDate,
                    MembershipClass = member.MembershipClass is null ? "" : member.MembershipClass.ClassName.Value,
                    MemberPoint =
                        (member.MemberPointHistories ?? new List<MemberPointHistory>()).Sum(x => x.MemberPoint.Value),
                    TotalBill = member.Orders.Where(o => o.HasPayment).Select(o => o.TotalBill).ToList()
                }).Select(member => new
                {
                    member.Address,
                    member.Email,
                    member.Id,
                    member.BirthDate,
                    member.FirstName,
                    member.LastName,
                    member.MemberCode,
                    member.MemberPoint,
                    member.MembershipClass,
                    member.PhoneNumber,
                    TotalPaid = member.TotalBill.Any()
                        ? member.TotalBill.Aggregate((x, y) => x + y)
                        : Money.Zero(Currency.Vnd)
                }).OrderBy(x => x.MemberCode).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var result = new byte[] { };

            var columns = new List<string>
            {
                "NO",
                "Customer Code",
                "Full Name",
                "Phone",
                "Email",
                "Address",
                "BirthDay",
                "Rank",
                "Total Points",
                "Total Money Spent"
            };
            using (var excelPackage = new ExcelPackage())
            {
                // Adding a new Worksheet
                excelPackage.Workbook.Worksheets.Add("Sheet 1");

                // Fetching the worksheet by name
                var worksheet = excelPackage.Workbook.Worksheets["Sheet 1"];
                var range = worksheet.Cells[$"J2:J{memberData.Count + 1}"]; // For example, cells A1 to A10

                // Set the number format to Vietnamese Dong currency
                range.Style.Numberformat.Format = "#,##0 ₫";
                //First add the headers
                for (var i = 0; i < columns.Count(); i++) worksheet.Cells[1, i + 1].Value = columns[i];

                var headerRange = worksheet.Cells["A1:J1"]; // For example, cells A1 to A10

                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);


                var j = 2;
                var count = 1;
                foreach (var item in memberData)
                {
                    worksheet.Cells["A" + j].Value = count;
                    worksheet.Cells["B" + j].Value = item.MemberCode;
                    worksheet.Cells["C" + j].Value = item.FirstName + " " + item.LastName;
                    worksheet.Cells["D" + j].Value = item.PhoneNumber;
                    worksheet.Cells["E" + j].Value = item.Email;
                    worksheet.Cells["F" + j].Value = item.Address;
                    worksheet.Cells["G" + j].Value =
                        item.BirthDate.HasValue ? item.BirthDate.Value.ToString("dd/MM/yyyy") : "";
                    worksheet.Cells["H" + j].Value = item.MembershipClass;
                    worksheet.Cells["I" + j].Value = item.MemberPoint;
                    worksheet.Cells["J" + j].Value = item.TotalPaid.Amount;
                    j++;
                    count++;
                }

                // Setting values in the worksheet

                // You can also loop through your data here to fill the worksheet

                // Saving the Excel package
                result = await excelPackage.GetAsByteArrayAsync(cancellationToken);
            }


            return await Task.FromResult(Result.Success(result));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}