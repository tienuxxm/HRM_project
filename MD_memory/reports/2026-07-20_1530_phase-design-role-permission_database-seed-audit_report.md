# BÁO CÁO RÀ SOÁT HỆ THỐNG SEEDING DATABASE VÀ TRÙNG LẶP QUYỀN POSITION

- **Thời gian lập**: 2026-07-20 15:30
- **Tác giả**: Senior .NET Fullstack Engineer & Database Architect
- **Trạng thái**: Đã hoàn thành phân tích read-only

---

## 1. MỤC TIÊU KHẢO SÁT
Tiến hành rà soát read-only toàn bộ các tệp tin Python (`*.py`), SQL (`*.sql`), và PowerShell (`*.ps1`) trong dự án (bao gồm cả thư mục `MD_memory/debug`) nhằm:
1. Xác định danh sách các script có kết nối cơ sở dữ liệu và phân loại chức năng (Chẩn đoán Read-only vs. Seed/Ghi dữ liệu).
2. Kiểm tra xem các quyền `VIEW_POSITION` và `UPDATE_POSITION` có bị chèn trùng lặp do các script debug địa phương này hay không.
3. Chỉ ra nguyên nhân gốc rễ (Root Cause Analysis - RCA) của hiện tượng trùng lặp (duplicate) quyền Position trên cơ sở dữ liệu và đề xuất phương án khắc phục an toàn.

---

## 2. LIỆT KÊ VÀ PHÂN LOẠI CÁC SCRIPT KẾT NỐI DATABASE
Qua khảo sát hệ thống, toàn bộ các script Python thực hiện kết nối cơ sở dữ liệu (sử dụng thư viện `psycopg2`) đều tập trung tại thư mục `MD_memory/debug`. Dưới đây là phân loại chi tiết:

### A. Nhóm Script Chẩn Đoán & Truy Vấn (Read-Only)
Các script này chỉ thực hiện câu lệnh `SELECT`, không ghi dữ liệu hoặc thay đổi trạng thái database:
*   `2026-06-29_0342_check-db-permissions.py`: Kiểm tra danh sách các quyền hiện có trong bảng `permission`.
*   `2026-06-29_0348_inspect-permissions-mapping.py`: Kiểm tra ánh xạ vai trò và quyền hạn (`role_to_permission`).
*   `2026-07-01_1158_query_db_permissions.py`: Truy vấn các quyền liên quan đến phân hệ `LEAVE_REQUEST`.
*   `2026-07-01_1655_query_uat_users_db.py`: Kiểm tra thông tin các tài khoản UAT trong bảng `user`.
*   `2026-07-01_1700_query_keycloak_users.py`: So khớp danh sách tài khoản cục bộ với Keycloak (không ghi DB).
*   `2026-07-02_0949_query_users.py`: Kiểm tra `email` và `identity_id` của các tài khoản để phục vụ cấu hình auth.
*   `2026-07-02_1330_preview_uat_status.py`: Kiểm tra trạng thái liên kết giữa `employee`, `user`, `department`, và `position`.
*   `2026-07-02_1345_debug_tp_leave_visibility.py`: Truy vấn cấu hình phân quyền duyệt đơn nghỉ phép của Trưởng phòng (`tp.test`).
*   `2026-07-02_1402_preview_roles_permissions.py`: Liệt kê nhanh các quyền được gán cho từng vai trò cụ thể.
*   `2026-07-02_1403_inspect_tables.py`: Truy vấn số lượng bản ghi hoặc cấu trúc sơ bộ các bảng.
*   `2026-07-03_0930_diagnose_ceo_visibility.py`: Truy vấn kiểm tra cấu hình duyệt đơn của tài khoản CEO.
*   `2026-07-04_1510_diagnose-login.py`: Truy vấn thông tin user test để so sánh với Keycloak JWT token payload.
*   `2026-07-08_1800_query_import_batches.py`: Kiểm tra dữ liệu lịch sử import và các batch đồng bộ dữ liệu.

### B. Nhóm Script Chạy Seed / Ghi Dữ Liệu (Write/Seed SQL)
Nhóm script này thực thi các tệp tin SQL có khả năng chèn hoặc cập nhật dữ liệu (`INSERT`, `UPDATE`). Cụ thể như sau:

1.  **`2026-07-01_1305_execute_seed_sql.py`**
    *   *Tệp SQL gọi đến*: `2026-07-01_1030_seed-approver-permissions.sql`
    *   *Bảng bị tác động*: `permission`, `role_to_permission`
    *   *Dữ liệu chèn*: Chèn các quyền `VIEW_LEAVE_APPROVER_ASSIGNMENT`, `UPDATE_LEAVE_APPROVER_ASSIGNMENT` và gán chúng cho vai trò ADMIN.
2.  **`2026-07-02_1338_run_seed_sql.py`**
    *   *Tệp SQL gọi đến*: `2026-07-02_1315_seed-uat-employees-mapping.sql`
    *   *Bảng bị tác động*: `employee`, `leave_approver_assignment`
    *   *Dữ liệu chèn*: Tạo/cập nhật thông tin nhân viên UAT (`EMP-NV-TEST`, `EMP-TP-TEST`, `EMP-CEO-TEST`) và cấu hình người duyệt đơn nghỉ phép.
3.  **`2026-07-02_1458_run_tp_seed.py`**
    *   *Tệp SQL gọi đến*: `2026-07-02_1410_seed-tp-test-permissions.sql`
    *   *Bảng bị tác động*: `role`, `role_to_permission`, `user_to_role`
    *   *Dữ liệu chèn*: Thêm vai trò `LEAVE_APPROVER`, gán quyền phê duyệt đơn nghỉ phép (`VIEW_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`), và gán vai trò này cho tài khoản Trưởng phòng (`tp.test`).

---

## 3. RÀ SOÁT CÁC TỪ KHÓA LIÊN QUAN ĐẾN QUYỀN POSITION
Chúng tôi đã tiến hành tìm kiếm toàn văn (full-text search) trong toàn bộ thư mục `MD_memory/debug` với các từ khóa: `VIEW_POSITION`, `UPDATE_POSITION`, `View Position`, và `Update Position`.

*   **Kết quả**: Không có bất kỳ tệp tin `.py`, `.sql` hay `.ps1` nào trong thư mục `MD_memory/debug` định nghĩa, tham chiếu hoặc thực hiện chèn (`INSERT`) các quyền này.
*   **Các script PowerShell seeding khác** (như `2026-06-26_0850_seed-permissions.ps1` và `db_seed.ps1`) cũng chỉ tập trung seed quyền của `LEAVE_TYPE` hoặc đồng bộ dữ liệu `user` từ cơ sở dữ liệu LUC cũ, tuyệt đối không chạm tới Master Data của Position.
*   **Kết luận 1**: Các script debug cục bộ và dữ liệu seed địa phương **hoàn toàn vô can** trong việc sinh ra dữ liệu trùng lặp cho Position.

---

## 4. ROOT CAUSE ANALYSIS (RCA) - NGUYÊN NHÂN TRÙNG LẶP QUYỀN POSITION
Sau khi loại trừ tác nhân từ các script debug, chúng tôi đã rà soát hệ thống Migration của dự án và phát hiện nguyên nhân gốc rễ nằm ở tệp di trú:
`HRM_Leave_Management/Infrastructure/Migrations/20260630063720_AddPositionMasterData.cs`

### Phân tích mã nguồn Migration (Dòng 91-96):
```csharp
migrationBuilder.Sql($@"
    INSERT INTO permission (id, resource_name, display_name, is_default, created_date)
    VALUES 
    ('{viewPositionPermissionId}', 'VIEW_POSITION', 'View Position', true, NOW()),
    ('{updatePositionPermissionId}', 'UPDATE_POSITION', 'Update Position', true, NOW())
    ON CONFLICT (id) DO NOTHING;
");
```

### Cơ chế lỗi dữ liệu:
1.  **Cú pháp tranh chấp**: Lệnh `ON CONFLICT (id) DO NOTHING` chỉ hoạt động phòng ngừa nếu bản ghi bị trùng cột khóa chính `id`.
2.  **Thiếu Unique Constraint**: Trong thiết kế cơ sở dữ liệu hiện tại, cột `resource_name` của bảng `permission` **không có Unique Constraint/Index**.
3.  **Lỗi logic**: Nếu trên môi trường database thực tế đã tồn tại các bản ghi có `resource_name` là `'VIEW_POSITION'` hoặc `'UPDATE_POSITION'` nhưng lại sở hữu các `id` khác với các GUID được hardcode trong migration (`9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f` và `8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e`), thì:
    *   Lệnh `ON CONFLICT (id) DO NOTHING` sẽ bỏ qua kiểm tra vì `id` không trùng.
    *   Hệ quản trị cơ sở dữ liệu (PostgreSQL) sẽ chèn thêm bản ghi mới bình thường.
    *   Kết quả dẫn đến bảng `permission` tồn tại song song nhiều dòng chứa cùng giá trị `resource_name` là `'VIEW_POSITION'` và `'UPDATE_POSITION'`.

---

## 5. ĐỀ XUẤT PHƯƠNG ÁN KHẮC PHỤC DỮ LIỆU
Để xử lý triệt để lỗi toàn vẹn dữ liệu này mà không phá vỡ tính nhất quán của hệ thống, chúng tôi đề xuất quy trình gồm 2 bước (cần được User phê duyệt trước khi triển khai):

### Bước 1: Tạo Migration dọn dẹp và chuẩn hóa Schema
Tạo một Migration mới (ví dụ: `FixDuplicatePositionPermissionsAndAddUniqueConstraint.cs`) thực hiện:
1.  Tìm và xóa các bản ghi trùng lặp trong bảng `permission` (chỉ giữ lại bản ghi có ID chuẩn hoặc xóa các bản ghi thừa dựa trên cột `resource_name`).
2.  Cập nhật khóa ngoại liên quan trong bảng `role_to_permission` để hướng về ID duy nhất được giữ lại.
3.  Tạo một `Unique Index` trên cột `resource_name` của bảng `permission`. Điều này sẽ ngăn ngừa vĩnh viễn mọi hành vi insert trùng lặp quyền từ bất kỳ nguồn nào trong tương lai.

### Bước 2: Chuẩn hóa logic Seeding trong các Migration SQL sau này
Mọi câu lệnh insert master data dạng raw SQL trong Migration phải sử dụng điều kiện kiểm tra sự tồn tại của `resource_name` thay vì chỉ dựa vào `id`:
```sql
INSERT INTO permission (id, resource_name, display_name, is_default, created_date)
SELECT '9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f', 'VIEW_POSITION', 'View Position', true, NOW()
WHERE NOT EXISTS (SELECT 1 FROM permission WHERE resource_name = 'VIEW_POSITION');
```
*(Đây chính là pattern an toàn đang được áp dụng trong các file SQL seed ở thư mục debug).*
