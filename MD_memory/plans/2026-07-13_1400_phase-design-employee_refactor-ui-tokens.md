# Kế hoạch Refactor UI Tokens - Phân hệ Employee

## 1. Xác định ranh giới kiến trúc và quy tắc bảo toàn
- **Kiến trúc bảo toàn**:
  - `Web.Backend -> Application -> Domain`
  - `Infrastructure -> Application/Domain`
- **Quy tắc tuyệt đối**:
  - KHÔNG sửa đổi mã nguồn C# (controllers, handlers, entities).
  - KHÔNG thay đổi database schema hay thực hiện migration mới.
  - KHÔNG thay đổi Keycloak/Authentication/Authorization setup.
  - KHÔNG thực hiện `git stage`, `git commit` hay `git push` tự động.
  - Chỉ chỉnh sửa các file giao diện Razor View (`Index.cshtml`, `_CreateEmployeePartial.cshtml`, `_UpdateEmployeePartial.cshtml`, `_ProvisionAccountPartial.cshtml`) để chuẩn hóa hệ thống màu sắc theo tokens Thụy Sĩ đã chốt.

## 2. Mục tiêu kỹ thuật
- Loại bỏ hoàn toàn các mã màu hex viết cứng (`#D1D1D1`, `#E62429`, `#bb0015`, `#F4F3F3`, `#FBFBFB`, `#93000e`, v.v.) trong các view của Employee.
- Sử dụng các lớp tiện ích (utility classes) từ cấu hình Tailwind:
  - `bg-swiss-light` thay cho nền `#FAF9F9` / `#F4F3F3` / `#F8F8F8`
  - `border-swiss-border` thay cho viền `#D1D1D1`
  - `text-swiss-red` / `bg-swiss-red` thay cho màu đỏ `#bb0015` / `#93000e`
  - `text-swiss-accent-red` / `bg-swiss-accent-red` thay cho màu đỏ tươi `#E62429`
  - Lớp `bg-black`, `border-black`, `text-white` cho các hành động primary để miễn nhiễm hoàn toàn với các class `.bg-primary !important` của Bootstrap.
- Rebuild lại file CSS chính thức thông qua công cụ Tailwind CSS (`npm run css:build`).
- Kiểm tra trực quan kết quả biên dịch và chạy ứng dụng.

## 3. Các file sẽ chỉnh sửa (ĐÃ HOÀN THÀNH 100%)
1. `HRM_Leave_Management/Web.Backend/Views/Employee/_CreateEmployeePartial.cshtml` (Đã refactor)
2. `HRM_Leave_Management/Web.Backend/Views/Employee/_UpdateEmployeePartial.cshtml` (Đã refactor)
3. `HRM_Leave_Management/Web.Backend/Views/Employee/_ProvisionAccountPartial.cshtml` (Đã refactor)
4. `HRM_Leave_Management/Web.Backend/Views/Employee/Index.cshtml` (Đã refactor)

## 4. Kế hoạch kiểm thử UAT thủ công (Manual UAT Plan) (ĐÃ HOÀN THÀNH & PHÁT HÀNH REPORT)
- **Chuẩn bị**:
  - Biên dịch ứng dụng: `dotnet build HRM_Leave_Management/LUC.sln` -> **ĐÃ ĐẠT**
  - Chạy Tailwind build: `npm run css:build` trong thư mục `HRM_Leave_Management/Web.Backend` -> **ĐÃ ĐẠT**
- **Thực hiện kiểm tra bằng mắt (Manual UAT Report)**:
  - Báo cáo UAT chi tiết đã được tạo tại `MD_memory/reports/2026-07-13_1430_phase-design-employee_modals-colors-verification_report.md`.
  - Dự án đang được chạy chạy tại `http://localhost:5300` để sẵn sàng cho User thực hiện kiểm thử thực tế.

