# Báo Cáo Triển Khai Bản Vá Quyền Master Data — Phase 3D Work Calendar

Tài liệu này ghi nhận việc triển khai bản vá cơ sở dữ liệu để bổ sung các quyền bắt buộc cho tính năng Lịch làm việc (Work Calendar) vào bảng dữ liệu master `permission`.

---

## 1. Vấn Đề Ghi Nhận (Problem Identified)

Trong quá trình chuẩn bị cho UAT Phase 3D, hệ thống kiểm tra quyền hạn truy cập của tính năng Lịch làm việc thông qua hai định danh quyền:
- `VIEW_WORK_CALENDAR`: Quyền xem lịch làm việc.
- `UPDATE_WORK_CALENDAR`: Quyền cập nhật, tải lên (Import) và xác nhận lịch làm việc.

Tuy nhiên, do các migration trước đó của Phase 3D chưa thực hiện khai báo (seed) hai bản ghi master này vào bảng `permission`, dẫn đến việc các quyền này không hiển thị trên giao diện quản trị **Roles & Permissions UI** của hệ thống. HR/Admin không thể gán quyền để thực hiện kiểm thử UAT.

---

## 2. Giải Pháp Triển Khai (Solution Implemented)

Để xử lý lỗ hổng dữ liệu master mà không can thiệp thủ công bằng SQL thô ngoài luồng hoặc phá vỡ cấu trúc của hệ thống, một migration EF Core độc lập đã được tạo ra:

### Tệp tin Migration
- **Tệp Migration chính**: [20260707115000_AddWorkCalendarPermissions.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Infrastructure/Migrations/20260707115000_AddWorkCalendarPermissions.cs)
- **Tệp Designer**: [20260707115000_AddWorkCalendarPermissions.Designer.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Infrastructure/Migrations/20260707115000_AddWorkCalendarPermissions.Designer.cs)

### Thiết kế An toàn (Defensive Design)
Lệnh SQL trong migration sử dụng mệnh đề `WHERE NOT EXISTS` kiểm tra theo trường `resource_name` thay vì chỉ kiểm tra ID để tránh xung đột dữ liệu hoặc trùng lặp bản ghi nếu migration được chạy lại:
```sql
INSERT INTO permission (id, resource_name, is_default, display_name, created_date)
SELECT 'b8b50e2d-dc99-43ef-b387-052637738f61', 'VIEW_WORK_CALENDAR', false, 'View Work Calendar', NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM permission WHERE resource_name = 'VIEW_WORK_CALENDAR'
);
```

---

## 3. Trạng Thái Vệ Sinh Git (Git Hygiene Status)

- **Trạng thái Working Tree**: Tất cả các tệp mới và chỉnh sửa hiện tại đều được giữ ở trạng thái **unstaged** (chưa stage).
- **Không tự động commit/push**: Không có commit hoặc push nào được thực hiện lên nhánh chính `main` nhằm tuân thủ tuyệt đối quy tắc an toàn của dự án.

---

## 4. Hướng Dẫn Chạy Migration & Kiểm Thử Cho Người Dùng

Do quyền thực thi terminal cục bộ bị hạn chế trên môi trường phát triển của Agent (`unexpected user interaction type: not permission`), người dùng cần thực hiện chạy các lệnh sau từ máy local để áp dụng migration và chuẩn bị dữ liệu:

1. **Thực hiện chạy Migration lên Database**:
   - Nếu chạy từ **thư mục gốc của repository** (repo root):
     ```bash
     dotnet ef database update --project HRM_Leave_Management/Infrastructure --startup-project HRM_Leave_Management/Web.Backend
     ```
   - Nếu chạy từ bên trong thư mục **`HRM_Leave_Management`**:
     ```bash
     dotnet ef database update --project Infrastructure --startup-project Web.Backend
     ```
2. **Xác minh quyền trong UI**:
   - Đăng nhập hệ thống bằng tài khoản Admin (`admin@hrm.local`).
   - Truy cập giao diện **Roles & Permissions UI**.
   - Xác nhận hai quyền `View Work Calendar` và `Update Work Calendar` đã xuất hiện trong danh sách và tiến hành gán cho các vai trò kiểm thử tương ứng.
3. **Thực thi UAT**:
   - Tiến hành chạy các ca kiểm thử trong tài liệu UAT Checklist [2026-07-07_1110_phase-3d_manual-uat-checklist_report.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/2026-07-07_1110_phase-3d_manual-uat-checklist_report.md).
