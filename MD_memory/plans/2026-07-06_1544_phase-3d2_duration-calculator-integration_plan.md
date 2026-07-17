# HRM Phase 3D.2 Plan — Duration Calculator & Leave Request Creation Integration

## 1. Scope
Phase này tập trung vào việc thiết kế và phát triển dịch vụ tính toán thời lượng đơn xin nghỉ phép (`IWorkCalendarService`) tại tầng Application, tích hợp thuật toán loại trừ ngày nghỉ/lễ và xử lý ca nửa ngày vào quy trình tạo và cập nhật đơn nghỉ phép.

---

## 2. Work Calendar Service Design (`Application.WorkCalendars`)

### A. Interface `IWorkCalendarService`
Định nghĩa phương thức chính để tính toán thời lượng nghỉ thực tế giữa hai mốc thời gian đăng ký:
```csharp
public interface IWorkCalendarService
{
    Task<decimal> CalculateLeaveDurationAsync(
        DateOnly startDate,
        DateOnly endDate,
        LeaveRequestShift startShift,
        LeaveRequestShift endShift,
        CancellationToken cancellationToken);
}
```

### B. Thuật toán loại trừ và tính toán ca (`WorkCalendarService`)
Với mỗi ngày trong phạm vi đăng ký từ `startDate` đến `endDate`:
1.  **Kiểm tra xem ngày đó có cấu hình ghi đè trong DB không**:
    *   Lấy bản ghi `WorkCalendarDay` tương ứng (nếu có và `IsActive == true`).
2.  **Xác định ca làm việc thực tế của ngày**:
    *   Nếu có cấu hình ghi đè: Sử dụng giá trị `WorkShiftType` của cấu hình.
    *   Nếu không có cấu hình ghi đè:
        *   Nếu là Thứ bảy hoặc Chủ nhật $\rightarrow$ Ca làm việc mặc định là `None` (Ngày nghỉ).
        *   Nếu là Thứ hai đến Thứ sáu $\rightarrow$ Ca làm việc mặc định là `FullDay` (Làm việc cả ngày).
3.  **Tính toán thời lượng đóng góp của ngày đó vào đơn phép**:
    *   Nếu ca làm việc thực tế là `None` $\rightarrow$ Ngày đó đóng góp **0.0 ngày** (loại trừ hoàn toàn).
    *   Nếu ca làm việc thực tế là `FullDay` $\rightarrow$ Ngày đó đóng góp **1.0 ngày** (hoặc **0.5 ngày** nếu nhân viên đăng ký nghỉ nửa ngày).
    *   Nếu ca làm việc thực tế là `MorningOnly` hoặc `AfternoonOnly` (Ngày làm việc nửa ngày):
        *   Nếu nhân viên đăng ký nghỉ cả ngày (`FullDay` request): Đóng góp của ngày đó vào thời lượng đơn phép là **0.5 ngày**.
        *   Nếu nhân viên đăng ký nghỉ trùng khớp ca làm việc nửa ngày (ví dụ nghỉ ca `Morning` vào ngày làm việc `MorningOnly`): Đóng góp là **0.5 ngày**.
        *   Nếu nhân viên đăng ký nghỉ lệch ca làm việc nửa ngày (ví dụ đăng ký nghỉ ca `Afternoon` vào ngày làm việc `MorningOnly`): Hệ thống chặn và báo lỗi validation `LeaveRequest.InvalidShiftRegistration`.

---

## 3. Application Commands Integration

### A. Tích hợp vào `CreateLeaveRequestCommandHandler`
*   Trước khi lưu đơn nghỉ phép, gọi `IWorkCalendarService` để tính toán thời lượng thực tế của đơn phép (`Duration`).
*   Tính toán số dư khả dụng thực tế của nhân viên: `AvailableDays = AllocatedDays - UsedDays - PendingDuration` (với `PendingDuration` được tính động từ các đơn phép đang chờ duyệt).
*   Nếu `Duration > AvailableDays`: Chặn và trả về lỗi `LeaveBalance.InsufficientBalance`.
*   Nếu `Duration == 0.0`: Chặn và trả về lỗi `LeaveRequest.OnlyNonWorkingDays`.

### B. Tích hợp vào `UpdateLeaveRequestCommandHandler`
*   Thực hiện gọi dịch vụ tính toán lại thời lượng đơn phép tương tự khi người dùng chỉnh sửa ngày nghỉ hoặc ca đăng ký trên đơn phép `Pending`.
*   Kiểm tra số dư khả dụng tương ứng và chặn nếu không đủ phép.

---

## 4. Safety Constraints & Impact Analysis
*   **GitNexus Impact Analysis**: Trước khi bắt đầu sửa đổi mã nguồn tại `CreateLeaveRequestCommandHandler.cs` hoặc `LeaveRequest.cs`, bắt buộc phải chạy phân tích blast radius cho các thực thể/handler này để đảm bảo không làm gãy các quy trình nghiệp vụ cũ.
*   **Quy tắc clean architecture**: Dịch vụ `WorkCalendarService` chỉ phụ thuộc vào `IWorkCalendarDayRepository` và các thực thể Domain, tuyệt đối không gọi trực tiếp EF DbContext hoặc các API lớp Web.

---

## 5. Verification Checklist
*   [ ] Interface `IWorkCalendarService` được đăng ký DI chuẩn xác.
*   [ ] Thuật toán tính toán xử lý chính xác Thứ bảy/Chủ nhật mặc định là nghỉ.
*   [ ] Các đơn phép xin nghỉ đi qua ngày lễ (PublicHoliday) tự động trừ ngày lễ ra khỏi thời lượng.
*   [ ] Lỗi đăng ký sai ca (`InvalidShiftRegistration`) được kích hoạt và chặn thành công khi đăng ký lệch ca trên ngày làm việc nửa ngày.