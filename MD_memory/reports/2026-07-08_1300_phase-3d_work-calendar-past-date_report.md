# Báo cáo UAT và Phân tích Lỗi TC-06: Chặn Ngày Trong Quá Khứ (Work Calendar Past Date Guard)

## 1. Tóm tắt sự cố (Incident Summary)
- **Triệu chứng**: Giao diện Quản lý Lịch làm việc (`/work-calendar`) cho phép người dùng có quyền quản trị thêm mới hoặc chỉnh sửa cấu hình lịch làm việc cho các ngày trong quá khứ mà không có bất kỳ rào cản hay cảnh báo nào. Điều này vi phạm quy tắc nghiệp vụ kiểm soát lịch sử làm việc (BD-D: Past/Future Calendar Control) của phân hệ `HRM_Leave_Management`.
- **Hành vi thực tế trước khi sửa**:
  - Tài khoản Admin có thể chọn ngày `2025-05-15` (ngày trong quá khứ so với mốc thời gian hiện tại 2026).
  - Yêu cầu Preview và Save thủ công đều thực hiện thành công (mã phản hồi HTTP 200).
  - Bản ghi được lưu trực tiếp vào cơ sở dữ liệu và hiển thị đầy đủ trên bảng danh sách của năm 2025.

---

## 2. Kết quả UAT Trình duyệt & Tích hợp Excel (UAT Verification Results)

### A. Luồng Thao Tác Thủ Công (Manual Form Flow)
1. Đăng nhập vào hệ thống HRM bằng tài khoản quản trị `admin` / `Admin@123456`.
2. Khi mở modal thêm mới Lịch làm việc, trường Date hiển thị thuộc tính HTML5 `min` động để chặn chọn ngày quá khứ trên giao diện.
3. Khi cố tình nhập và gửi yêu cầu lưu ngày trong quá khứ (ví dụ: ngày hôm qua `07/07/2026` so với ngày hệ thống hiện tại `08/07/2026`):
   - Hệ thống lập tức kích hoạt API xem trước và phản hồi lỗi trên UI:
     > **`Configuring calendar days for past dates is not allowed`** (Màu đỏ phía trên nút Save).
   - Nút **Save** bị chặn và không cho phép lưu thay đổi (không gửi form, modal không đóng, dữ liệu không được ghi vào DB).
4. Khi chỉnh sửa ngày thành ngày tương lai (ví dụ: `08/08/2026`):
   - Cảnh báo lỗi biến mất ngay lập tức.
   - Nút **Save** hoạt động bình thường và lưu thành công bản ghi mới lên danh sách hiển thị.

### B. Kiểm thử Tích hợp Excel (Excel Import Integration UAT)
1. Thực hiện UAT kiểm thử tích hợp bằng cách upload tệp Excel tự tạo (`WorkCalendar_PastDateTest.xlsx`) chứa hai dòng dữ liệu:
   - Dòng 1 (Ngày quá khứ): `2026-07-07` (ngày hôm qua so với mốc hôm nay `08/07/2026`).
   - Dòng 2 (Ngày tương lai): `2026-08-08`.
2. Gửi yêu cầu upload file lên endpoint `/work-calendar/upload`, server tiếp nhận thành công và trả về mã lô nhập `batchId`:
   - **`batchId`**: `a0743ca9-b449-4ddc-88e9-d58c75529656`
3. Truy cập preview giao diện tại `/work-calendar/preview/a0743ca9-b449-4ddc-88e9-d58c75529656` và xác thực kết quả hiển thị của từng dòng dữ liệu:
   - **Kết quả dòng quá khứ (`2026-07-07` - Row index 2)**: Trạng thái được đánh dấu rõ ràng là **Invalid** (status value `3` trong cơ sở dữ liệu) với cột thông báo lỗi hiển thị chính xác:
     > **`Configuring calendar days for past dates is not allowed`**
   - **Kết quả dòng tương lai (`2026-08-08` - Row index 3)**: Được chấp nhận là **Valid** (status value `2` trong cơ sở dữ liệu) và sẵn sàng để confirm.
   - **Hành vi nút Apply Import**: Nút "Apply Import" bị chặn/vô hiệu hóa hoàn toàn trên giao diện (thuộc tính `disabled` được thêm vào thẻ `<button>` do cờ `hasErrors` được đánh dấu là `true`), đảm bảo tính toàn vẹn của dữ liệu hệ thống.
   - **Lưu ý quan trọng**: Lô nhập này (`batchId: a0743ca9-b449-4ddc-88e9-d58c75529656`) được giữ làm dữ liệu UAT trong DB, **hoàn toàn chưa được áp dụng (NOT applied)** vào lịch làm việc chính thức do chứa lỗi vi phạm nghiệp vụ quá khứ.

---

## 3. Phân tích Nguyên nhân Gốc rễ & Mã nguồn đã sửa đổi (Root Cause & Fix Details)

### A. Tầng Domain
Đã định nghĩa mã lỗi nghiệp vụ mới tại `Domain/WorkCalendars/WorkCalendarErrors.cs`:
```csharp
public static class WorkCalendarErrors
{
    public static readonly Error PastEditingNotAllowed = new(
        "WorkCalendar.PastEditingNotAllowed",
        "Configuring calendar days for past dates is not allowed");
}
```
*Lưu ý về định nghĩa `Error`: Lớp lỗi sử dụng đúng cấu trúc `Error(Code, Name)` của dự án, trong đó `Name` chứa chuỗi thông điệp hiển thị.*

### B. Tầng Application (Application Handlers)
1. **Lưu dữ liệu (`SaveManualCalendarDayCommandHandler.Handle`)**:
   Tích hợp `IDateTimeProvider` để lấy giờ địa phương Việt Nam (ToVnTime) làm mốc so sánh ngày hiện tại và kiểm tra:
   ```csharp
   var today = DateOnly.FromDateTime(_dateTimeProvider.ToVnTime(_dateTimeProvider.UtcNow));
   if (request.Date < today)
   {
       return Result.Failure(WorkCalendarErrors.PastEditingNotAllowed);
   }
   ```
2. **Xem trước (`PreviewManualCalendarChangeQueryHandler.Handle`)**:
   Thực hiện kiểm tra chặn ngày quá khứ tương tự để ngăn tạo dữ liệu xem trước cho các ngày đã qua.
3. **Nhập dữ liệu Excel (`CalendarImportService.ParseAndSaveDraftAsync`)**:
   Thực hiện kiểm tra ngày quá khứ trong quá trình phân tích cú pháp từng dòng Excel. Nếu phát hiện ngày quá khứ, thêm lỗi chặn tương ứng giúp dòng đó không được lưu:
   ```csharp
   if (date != null)
   {
       var today = DateOnly.FromDateTime(_dateTimeProvider.ToVnTime(_dateTimeProvider.UtcNow));
       if (date.Value < today)
       {
           errors.Add(WorkCalendarErrors.PastEditingNotAllowed.Name);
       }
   }
   ```

### C. Tầng Web (Razor View)
Cập nhật tệp `Index.cshtml` để bổ sung thuộc tính `min` động trên lịch khi bấm nút thêm mới:
```javascript
$('#manualDate').val(dateStr);
if (dateStr) {
    $('#manualDate').prop('readonly', true);
    $('#manualDate').removeAttr('min');
} else {
    $('#manualDate').prop('readonly', false);
    const todayStr = new Date().toLocaleDateString('en-CA');
    $('#manualDate').attr('min', todayStr);
}
```

---

## 4. Phân tích Tác động Thủ Công (Manual Impact Analysis)
- **Tình trạng Index GitNexus**: 
  - Do các tệp tin trong phân hệ `WorkCalendar` hoàn toàn mới và đang ở trạng thái chưa theo dõi (untracked) trong git (`?? HRM_Leave_Management/Application/WorkCalendars/` và `?? HRM_Leave_Management/Domain/WorkCalendars/`), công cụ phân tích tự động GitNexus tạm thời không thể tìm thấy các ký hiệu này để lập biểu đồ cuộc gọi.
  - Do đó, việc phân tích tác động được thực hiện thủ công bằng cách tìm kiếm và ánh xạ các điểm tiêu thụ (Upstream Callers) trong mã nguồn.
- **Danh sách ký hiệu bị ảnh hưởng & Luồng Gọi Upstream**:
  - `WorkCalendarErrors.PastEditingNotAllowed` (Khai báo lỗi miền) -> Tác động thấp. Chỉ được tiêu thụ bởi các handler mới.
  - `SaveManualCalendarDayCommandHandler.Handle` -> Được trigger bởi MediatR gửi `SaveManualCalendarDayCommand` từ route `[HttpPost("save-manual")]` trong `WorkCalendarController.cs`.
  - `PreviewManualCalendarChangeQueryHandler.Handle` -> Được trigger bởi MediatR gửi `PreviewManualCalendarChangeQuery` từ route `[HttpPost("preview-manual")]` trong `WorkCalendarController.cs`.
  - `CalendarImportService.ParseAndSaveDraftAsync` -> Được gọi bởi `UploadCalendarImportBatchCommandHandler` để phân tích tệp tin excel đầu vào.
- **Phạm vi ảnh hưởng thực tế**: Chỉ giới hạn trong phân hệ quản lý lịch làm việc (`Work Calendar`), không ảnh hưởng đến các phân hệ cốt lõi khác của hệ thống HRM. Mối quan hệ ràng buộc và kiến trúc Clean Architecture được bảo đảm hoàn toàn.

---

## 5. Trạng thái Build & Kiểm thử cục bộ
- **Trạng thái Build**: **PASS**
  - Đã thực hiện build thành công giải pháp scoped cụ thể của HRM bằng lệnh:
    ```bash
    dotnet build HRM_Leave_Management/LUC.sln --no-restore
    ```
  - Output kết quả build chi tiết:
    ```text
    Build succeeded.
        15 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:01.65
    ```
- **Trạng thái UAT**: **PASS**
  - Đã kiểm tra cả hai luồng: Nhập thủ công trên giao diện và Nhập qua tệp tin Excel đều hoạt động chính xác theo đúng đặc tả chặn ngày quá khứ và hiển thị lỗi tương ứng.

---

## 6. Trạng thái Git & Mã hóa (Git Status & Encoding)
- **Kiểm soát Encoding**:
  - Đã thực hiện kiểm tra encoding thông qua công cụ quét `scan-mojibake.py`.
  - Kết quả quét:
    - Files scanned: 41
    - BOM failures: 0
    - Mojibake hits: 0
    - Exit code: 0
  - Tất cả các tệp tài liệu đều tuân thủ định dạng UTF-8 có BOM (BOM OK).
- **Trạng thái Git**: 
  - Tệp dữ liệu thử nghiệm `WorkCalendar_PastDateTest.xlsx` đã được xóa khỏi thư mục gốc để đảm bảo vệ sinh mã nguồn.
  - Cần chú ý chỉ thực hiện stage các tệp tin được chỉnh sửa rõ ràng khi commit dữ liệu, tránh sử dụng `git add .` hoặc `git add -A`. Không thực hiện commit/push tự ý khi chưa được phê duyệt.
