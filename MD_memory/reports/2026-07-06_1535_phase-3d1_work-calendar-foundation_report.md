# HRM Phase 3D.1 Report — Work Calendar Foundation Design & Implementation Report

## 1. Executive Summary
Báo cáo này tổng hợp kết quả thiết kế và triển khai thực tế nền tảng lịch làm việc (`WorkCalendarDay`) thuộc Phase 3D.1 đã được điều chỉnh và hoàn thiện. Toàn bộ mã nguồn cốt lõi trong các tầng Domain và Infrastructure đã được xây dựng hoàn tất, tuân thủ nghiêm ngặt các ranh giới kiến trúc sạch (Clean Architecture), mẫu thiết kế repository của dự án hiện tại, và các hướng dẫn an toàn đã cam kết.

---

## 2. Completed Implementation Details

### A. Domain Layer - Entities & Enums (Domain/WorkCalendars/)
*   **Enums**:
    *   `CalendarDayType.cs`: Định nghĩa loại ngày thực tế trong mã nguồn:
        *   `PublicHoliday = 1`
        *   `CompanyCustomNonWorkingDay = 2`
        *   `WorkingSaturdayOverride = 3`
        *   `StandardWorkingDayOverride = 4`
    *   `WorkShiftType.cs`: Định nghĩa ca làm việc thực tế trong mã nguồn:
        *   `None = 0`
        *   `FullDay = 1`
        *   `MorningOnly = 2`
        *   `AfternoonOnly = 3`
*   **Value Object**:
    *   `WorkCalendarDayId.cs`: Định dạng strongly-typed ID cho `WorkCalendarDay` kế thừa từ `Guid`.
*   **Entity**:
    *   `WorkCalendarDay.cs`: Thực thể nghiệp vụ kế thừa `Entity<WorkCalendarDayId>`. Triển khai đóng gói dữ liệu nghiêm ngặt với các private setter và phương thức khởi tạo/cập nhật thông qua factory method (`Create`, `Update`). Quản lý trạng thái hoạt động thông qua thuộc tính `IsActive`.

### B. Domain Layer - Repository Interface (Domain/WorkCalendars/)
*   **Repository Interface**:
    *   `IWorkCalendarDayRepository.cs`: Được đặt tại thư mục Aggregate của Domain theo đúng mẫu thiết kế của dự án (đồng bộ với `ILeaveTypeRepository`, `IEmployeeRepository`, v.v.). Giao diện chỉ khai báo các phương thức nghiệp vụ tương tác với bản ghi đang hoạt động và loại bỏ hoàn toàn cơ chế xóa vật lý:
        *   `GetByIdAsync`
        *   `GetActiveByDateAsync` (lọc `IsActive == true`)
        *   `GetActiveByYearAsync` (lọc `IsActive == true`)
        *   `AddAsync`
        *   `Update`

### C. Infrastructure Layer (Infrastructure/)
*   **Entity Configuration**:
    *   `Infrastructure/Configurations/WorkCalendarDayConfiguration.cs`: Thiết lập ánh xạ EF Core cho bảng `work_calendar_day`. Cấu hình strongly-typed ID converter, khóa chính, và chỉ mục duy nhất (Unique Index) trên cột `date`. Các cột thuộc tính tự động ánh xạ sang **snake_case** qua `UseSnakeCaseNamingConvention()`.
*   **Repository Implementation**:
    *   `Infrastructure/Repositories/WorkCalendarDayRepository.cs`: Triển khai `IWorkCalendarDayRepository`, kế thừa từ lớp cơ sở `Repository<WorkCalendarDay, WorkCalendarDayId>`. 
        *   Hàm `GetActiveByYearAsync` được thiết kế tối ưu bằng cách so sánh khoảng ngày (`Date >= start && Date <= end`) thay vì truy vấn `Date.Year` nhằm thân thiện với chỉ mục cơ sở dữ liệu và tránh mơ hồ khi dịch câu lệnh SQL.
        *   Loại bỏ hoàn toàn phương thức xóa vật lý `Delete(...)`.
*   **DbContext Integration**:
    *   `Infrastructure/ApplicationDbContext.cs`: Khai báo thêm `DbSet<WorkCalendarDay> WorkCalendarDays` và `using Domain.WorkCalendars;`.
*   **Dependency Injection Registration**:
    *   `Infrastructure/DependencyInjection.cs`: Đăng ký Scoped cho `IWorkCalendarDayRepository` đi kèm with `WorkCalendarDayRepository` giúp tự động inject dịch vụ vào ứng dụng. Loại bỏ using directive của Application Repository dư thừa.

---

## 3. Implementation Guardrails & Boundary Verification
*   **Ranh giới kiến trúc sạch (Clean Architecture Boundaries)**:
    *   `Web.Backend -> Application -> Domain`
    *   `Infrastructure -> Application/Domain`
    *   Các thay đổi chỉ giới hạn trong phạm vi cho phép của Phase 3D.1, tuyệt đối không chỉnh sửa hay tác động đến các module di sản CSM (Restaurant, Booking, Loyalty, Voucher, v.v.).
*   **Không cung cấp cơ chế xóa vật lý**: Cả interface `IWorkCalendarDayRepository` và lớp triển khai `WorkCalendarDayRepository` đều không chứa phương thức `Delete(...)`. Quy trình vô hiệu hóa ngày lịch làm việc sẽ được thực hiện bằng cách thay đổi cờ trạng thái hoạt động `IsActive = false` và cập nhật thông qua phương thức `Update` của Repository.
*   **An toàn Dữ liệu & Môi trường**:
    *   Chưa chạy lệnh thực thi Migration trên cơ sở dữ liệu thực tế (`dotnet ef database update`). Việc áp dụng migration sẽ chỉ được tiến hành sau khi có sự chấp thuận tiếp theo từ User/Codex.
    *   Không thay đổi bất kỳ cấu hình Keycloak, tài khoản UAT, hoặc dữ liệu Runtime nào.

---

## 4. Verification Results & Git Scope

### A. Build Verification
*   Lệnh kiểm tra: `dotnet build HRM_Leave_Management/Infrastructure/Infrastructure.csproj --no-restore`
*   Kết quả biên dịch: **Targeted Infrastructure build including referenced projects: Build succeeded, 0 errors, 10 warnings.**

### B. Migration Review (Phase 3D.1 Migration)
*   **Các tệp tin Migration được tạo ra**:
    *   `HRM_Leave_Management/Infrastructure/Migrations/20260707012425_AddWorkCalendarDay.cs`
    *   `HRM_Leave_Management/Infrastructure/Migrations/20260707012425_AddWorkCalendarDay.Designer.cs`
    *   `HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` (Cập nhật snapshot của Context)
*   **Đánh giá cấu trúc Migration**:
    *   Migration chỉ tạo đúng một bảng duy nhất là `work_calendar_day` và các cột thuộc tính tương ứng (`id`, `date`, `day_type`, `work_shift`, `description`, `is_active`, `created_by`, `created_at`).
    *   Thiết lập khóa chính `pk_work_calendar_day` trên cột `id`.
    *   Tạo Unique Index `ix_work_calendar_day_date` trên cột `date` của bảng `work_calendar_day`.
    *   Phương thức `Down` thực hiện drop bảng `work_calendar_day` chuẩn xác.
    *   **Không phát hiện** bất kỳ thay đổi ngoài ý muốn nào đối với các bảng khác hoặc các khóa ngoại (FK) khác trong hệ thống.

### C. Git Scope Analysis (Trung thực)
*Verified by command:* `git status --short`

Dưới đây là danh sách phân tách cụ thể phạm vi thay đổi dựa trên tình trạng mã nguồn hiện tại sau khi sinh Migration:

1. **Phase 3D.1 files allowed to stage**:
   *   `HRM_Leave_Management/Domain/WorkCalendars/CalendarDayType.cs`
   *   `HRM_Leave_Management/Domain/WorkCalendars/WorkShiftType.cs`
   *   `HRM_Leave_Management/Domain/WorkCalendars/WorkCalendarDayId.cs`
   *   `HRM_Leave_Management/Domain/WorkCalendars/WorkCalendarDay.cs`
   *   `HRM_Leave_Management/Domain/WorkCalendars/IWorkCalendarDayRepository.cs`
   *   `HRM_Leave_Management/Infrastructure/Configurations/WorkCalendarDayConfiguration.cs`
   *   `HRM_Leave_Management/Infrastructure/Repositories/WorkCalendarDayRepository.cs`
   *   `HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs`
   *   `HRM_Leave_Management/Infrastructure/DependencyInjection.cs`
   *   `HRM_Leave_Management/Infrastructure/Migrations/20260707012425_AddWorkCalendarDay.cs`
   *   `HRM_Leave_Management/Infrastructure/Migrations/20260707012425_AddWorkCalendarDay.Designer.cs`
   *   `HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`
   *   `MD_memory/reports/2026-07-06_1535_phase-3d1_work-calendar-foundation_report.md`

2. **Phase 3D.1 report file**:
   *   `MD_memory/reports/2026-07-06_1535_phase-3d1_work-calendar-foundation_report.md`

3. **Existing dirty files outside scope, not to stage**:
   *   `.agents/rules/project.md`
   *   `.agents/skills/luc-hrm-refactor-guard/SKILL.md`
   *   `HRM_Leave_Management/Web.Backend/Views/Employee/Index.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/Employee/_ProvisionAccountPartial.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Detail.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Index.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/_ConfirmCancelPartial.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/Role/CreateRoleView.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/User/CreateUserView.cshtml`
   *   `HRM_Leave_Management/Web.Backend/Views/User/Detail.cshtml`
   *   `HRM_Leave_Management/Web.Backend/appsettings.json`
   *   `MD_memory/gap_analysis.md`
   *   `MD_memory/hrm_refactor_mapping.md`
   *   `MD_memory/project_architecture_analysis.md`
   *   `Web.Backend/appsettings.json`
   *   `MD_memory/plans/2026-07-03_1500_phase-3c_user-employee-provisioning.md`
   *   `MD_memory/plans/2026-07-04_1349_phase-3c5_employee-provision-ui.md`
   *   `MD_memory/plans/2026-07-06_1410_phase-3d_work-calendar-leave-rules.md`
   *   `MD_memory/plans/2026-07-06_1544_phase-3d1_work-calendar-foundation_plan.md`
   *   `MD_memory/plans/2026-07-06_1544_phase-3d2_duration-calculator-integration_plan.md`
   *   `MD_memory/plans/2026-07-06_1544_phase-3d3_import-and-recalculation_plan.md`
   *   `MD_memory/plans/2026-07-06_1544_phase-3d4_work-calendar-ui_plan.md`
   *   `MD_memory/plans/2026-07-06_1544_phase-3d5_verification-uat_plan.md`
   *   `MD_memory/reports/2026-07-03_1635_phase-3c2_impact-analysis_report.md`
   *   `MD_memory/reports/2026-07-03_1700_phase-3c2_employee-provisioning_report.md`
   *   `MD_memory/reports/2026-07-04_1349_phase-3c5_employee-provision-ui_report.md`
   *   `MD_memory/reports/2026-07-04_1405_phase-3c5_employee-auth-analysis_report.md`
   *   `MD_memory/reports/2026-07-06_1110_phase-3c5_browser-uat_result_report.md`
   *   `MD_memory/reports/2026-07-06_1600_phase-3d_final_plan_pack_report.md`
   *   `MD_memory/reports/2026-07-06_1640_phase-3d_hygiene_restoration_report.md`

4. **Removed local untracked misplaced file**:
   *   `HRM_Leave_Management/Application/Abstractions/Repositories/IWorkCalendarDayRepository.cs`

### D. Encoding Scan
*   Lệnh kiểm tra: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/plans/*.md MD_memory/reports/*.md --require-bom`
*   *Verified by command:*
    ```
    BOM failures = 0
    Mojibake hits = 0
    Exit code = 0
    ```