# HRM Phase 3D — Work Calendar & Leave Rules: Final Plan Pack Report

## 1. Scope & Plan Pack Overview
Tài liệu này tổng hợp toàn bộ thông tin của gói kế hoạch thiết kế và triển khai phân hệ **Work Calendar & Leave Rules (Phase 3D)** thuộc hệ thống quản lý nghỉ phép HRM (`HRM_Leave_Management`). Gói tài liệu này đã được rà soát, đồng bộ hóa và chuẩn hóa mã hóa UTF-8 BOM nhằm đảm bảo tính toàn vẹn cao nhất trước khi bước vào giai đoạn phát triển mã nguồn.

### Danh sách các tài liệu trong gói kế hoạch:
1.  **Main Plan**: `MD_memory/plans/2026-07-06_1410_phase-3d_work-calendar-leave-rules.md` (Quyết định nghiệp vụ cốt lõi và kịch bản UAT).
2.  **Phase 3D.1 Plan**: `MD_memory/plans/2026-07-06_1544_phase-3d1_work-calendar-foundation_plan.md` (Domain Model & EF Core Configuration).
3.  **Phase 3D.2 Plan**: `MD_memory/plans/2026-07-06_1544_phase-3d2_duration-calculator-integration_plan.md` (Dịch vụ tính thời lượng & Tích hợp đơn nghỉ).
4.  **Phase 3D.3 Plan**: `MD_memory/plans/2026-07-06_1544_phase-3d3_import-and-recalculation_plan.md` (Quy trình Excel Import & Công cụ recalculation tự động).
5.  **Phase 3D.4 Plan**: `MD_memory/plans/2026-07-06_1544_phase-3d4_work-calendar-ui_plan.md` (Giao diện Razor MVC quản trị và cảnh báo).
6.  **Phase 3D.5 Plan**: `MD_memory/plans/2026-07-06_1544_phase-3d5_verification-uat_plan.md` (Quy trình và kịch bản UAT thủ công chi tiết).
7.  **Phase 3D.1 Report**: `MD_memory/reports/2026-07-06_1535_phase-3d1_work-calendar-foundation_report.md` (Báo cáo hoàn thành thiết kế nền tảng).

---

## 2. Core Business Decisions (BD-A to BD-E)
*   **BD-A (Saturday/Sunday)**: Thứ bảy và Chủ nhật mặc định là ngày nghỉ không làm việc. Thứ bảy chỉ là ngày làm việc nếu được ghi đè bằng `WorkingSaturdayOverride`. Đơn xin nghỉ phép chỉ gồm các ngày nghỉ mặc định sẽ bị chặn.
*   **BD-B (Excel Import)**: Hỗ trợ Excel Import theo cơ chế Incremental Update (chỉ thêm mới/cập nhật những ngày xuất hiện trong file Excel, giữ nguyên ngày cấu hình cũ không có trong file).
*   **BD-C (Calendar Change Recalculation)**: Khi cập nhật lịch làm việc ảnh hưởng đến đơn nghỉ phép đã duyệt (`Approved`), hệ thống tự động hoàn lại số dư `UsedDays` cũ, chuyển trạng thái đơn phép về **`Pending`**, xóa `ProcessedBy`/`ProcessedAt` hiện tại, và ghi nhận nhật ký tính toán lại vào bảng Audit. Không ghi đè thông tin recalculation vào trường `Comment` của đơn nghỉ phép.
*   **BD-D (Past/Future Control)**: Cho phép chỉnh sửa tự do ngày tương lai. Hạn chế nghiêm ngặt việc chỉnh sửa ngày trong quá khứ (`Date < Today`), yêu cầu cấu hình hệ thống bật và có quyền `UPDATE_PAST_WORK_CALENDAR`.
*   **BD-E (Draft Preview)**: Lô import được lưu trữ tạm thời dưới dạng Draft để hiển thị Preview các dòng lỗi (Error) trực tiếp trên UI trước khi người dùng bấm nút Apply chính thức.

---

## 3. Architecture & Safety Guardrails

### A. Ranh giới kiến trúc Clean Architecture
*   Tuyệt đối tuân thủ ranh giới tham chiếu:
    `Web.Backend -> Application -> Domain`
    `Infrastructure -> Application/Domain`
*   Tầng Domain (`Domain.WorkCalendars`) hoàn toàn không phụ thuộc vào các tầng bên ngoài.

### B. Bảo vệ Module cũ và Xác thực
*   **Không mutate CSM legacy**: Tuyệt đối không thay đổi mã nguồn các phân hệ cũ (Loyalty, Restaurant, Orders, Bookings, Vouchers, Promotions).
*   **Chốt chặn Keycloak**: Chạy UAT trên Keycloak thực tế (realm `hrm`, client `hrm-web`, tài khoản `admin` / `Admin@123456`). Không mock auth để bypass.
*   **Không tự động chạy Browser UAT**: Quy trình UAT sẽ được thực hiện thủ công bởi người dùng theo các kịch bản chi tiết được cung cấp tại Phase 3D.5.

### C. Quy trình Git an toàn
*   Trước khi thực hiện bất kỳ sửa đổi mã nguồn nào tại Domain hay Application Handler, bắt buộc phải chạy công cụ GitNexus `impact` để phân tích tầm ảnh hưởng của các thay đổi và báo cáo rủi ro.
*   Không stage các thư mục sinh tự động hoặc thư mục lưu trữ cục bộ.

---

## 4. Plan Pack Validation & Readiness
*   Gói kế hoạch đã hoàn thành toàn diện phần thiết kế kiến trúc và mô tả nghiệp vụ chi tiết.
*   Mã hóa tệp tin lập kế hoạch được xác minh đạt chuẩn UTF-8 BOM, đảm bảo không có lỗi mojibake hoặc lỗi BOM rỗng.
*   Dự án sẵn sàng bước vào giai đoạn phát triển (Implementation) cho Phase 3D.1 ngay khi có sự đồng thuận từ phía người dùng.
