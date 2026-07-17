# HRM Phase 3D.1 Plan — Work Calendar Foundation

## 1. Scope
Phase này tập trung hoàn toàn vào việc xây dựng nền tảng Domain Model, cơ chế ánh xạ EF Core (Configurations), định nghĩa Repository và đăng ký Dependency Injection cho phân hệ Lịch làm việc (`WorkCalendarDay`). Không bao gồm logic tính toán thời lượng hay giao diện người dùng.

---

## 2. Domain Model Design (`Domain.WorkCalendars`)

### A. Thực thể chính: `WorkCalendarDay`
*   **Thuộc tính**:
    *   `Id`: `WorkCalendarDayId` (Strongly-typed ID).
    *   `Date`: `DateOnly` - Ngày cấu hình (Unique).
    *   `DayType`: `CalendarDayType` - Loại ngày cấu hình.
    *   `WorkShift`: `WorkShiftType` - Ca làm việc áp dụng.
    *   `Description`: `string` - Mô tả (ví dụ: tên ngày lễ quốc gia).
    *   `IsActive`: `bool` - Trạng thái hoạt động.
    *   `CreatedBy`: `Guid` - ID người tạo.
    *   `CreatedAt`: `DateTime` - Thời gian tạo.
*   **Quy tắc nghiệp vụ**:
    *   Thực thể phải cung cấp phương thức khởi tạo (`Create`) và phương thức cập nhật trạng thái (`Update`) để đảm bảo đóng gói (encapsulation).

### B. Kiểu dữ liệu Enums
*   **`CalendarDayType`**:
    *   `PublicHoliday = 1` (Nghỉ lễ quốc gia).
    *   `CompanyCustomNonWorkingDay = 2` (Ngày nghỉ riêng của công ty).
    *   `WorkingSaturdayOverride = 3` (Thứ bảy đi làm bù).
    *   `StandardWorkingDayOverride = 4` (Ghi đè ngày làm việc chuẩn).
*   **`WorkShiftType`**:
    *   `None = 0` (Không có ca làm việc - dùng cho ngày nghỉ).
    *   `FullDay = 1` (Làm việc cả ngày).
    *   `MorningOnly = 2` (Chỉ làm việc buổi sáng).
    *   `AfternoonOnly = 3` (Chỉ làm việc buổi chiều).

---

## 3. Infrastructure & Repository Mapping

### A. Cấu hình EF Core (`WorkCalendarDayConfiguration`)
*   Ánh xạ thực thể `WorkCalendarDay` vào bảng `work_calendar_day` trong DB.
*   **Database Columns (snake_case)**:
    *   `id` (GUID, Primary Key).
    *   `date` (DateOnly, Unique Index).
    *   `day_type` (Integer).
    *   `work_shift` (Integer).
    *   `description` (NVARCHAR(500), Nullable).
    *   `is_active` (Boolean).
    *   `created_by` (GUID).
    *   `created_at` (DateTime).

### B. Interface Repository (`IWorkCalendarDayRepository`)
*   Định nghĩa các phương thức:
    *   `GetByIdAsync(WorkCalendarDayId id, CancellationToken cancellationToken)`
    *   `GetByDateAsync(DateOnly date, CancellationToken cancellationToken)`
    *   `GetByYearAsync(int year, CancellationToken cancellationToken)`
    *   `AddAsync(WorkCalendarDay day, CancellationToken cancellationToken)`
    *   `Update(WorkCalendarDay day)`
    *   `Delete(WorkCalendarDay day)`: Đây là phương thức xóa vật lý ở mức generic repository.
        *   **Giới hạn Phase 3D.1**: Tuyệt đối không triển khai bất kỳ luồng xóa vật lý (physical delete) nào thông qua giao diện người dùng (UI) hoặc API trong Phase 3D.1.
        *   **Nghiệp vụ cốt lõi**: Luồng nghiệp vụ bắt buộc phải sử dụng cơ chế vô hiệu hóa (`IsActive = false` qua phương thức `Update`) cho các cấu hình lịch làm việc (Deactivate thay vì xóa vật lý).
        *   **Quy trình mở rộng**: Mọi hoạt động xóa vật lý (physical delete) dữ liệu cấu hình lịch trong tương lai bắt buộc phải có phê duyệt riêng biệt từ phía Technical Lead/Người dùng và phải trải qua quy trình phân tích tác động (impact analysis) đầy đủ.

### C. Đăng ký Dependency Injection
*   Đăng ký `IWorkCalendarDayRepository` vào tầng Infrastructure (`DependencyInjection.cs`) với vòng đời `Scoped`.

---

## 4. Migration Plan
*   Tạo file migration mới thông qua EF Core CLI:
    `dotnet ef migrations add AddWorkCalendarDayTable --project Infrastructure --startup-project Web.Backend`
*   **Lưu ý an toàn**: Trong giai đoạn thiết kế kế hoạch này, tuyệt đối **không chạy lệnh cập nhật cơ sở dữ liệu** (`dotnet ef database update`).

---

## 5. Entry Checklist & Verification
*   [ ] Thực thể `WorkCalendarDay` được định nghĩa chuẩn xác trong tầng Domain.
*   [ ] Enums `CalendarDayType` và `WorkShiftType` khớp hoàn toàn với thiết kế nghiệp vụ.
*   [ ] File cấu hình mapping `WorkCalendarDayConfiguration` sử dụng snake_case cho các cột dữ liệu.
*   [ ] Đăng ký DI thành công và dự án build không có lỗi.