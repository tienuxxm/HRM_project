# Báo cáo Kết quả UAT Quy trình Thôi việc Nhân viên (Employee Offboarding) - Phase 3D

## 1. Thông tin chung
- **Ngày thực hiện**: 2026-07-09
- **Phiên bản nghiệp vụ**: DB-First, Keycloak-After (Offboarding Refactor)
- **Môi trường thử nghiệm**:
  - Web UI / API: `http://localhost:5300`
  - Keycloak Server: `http://localhost:8080` (Realm: `hrm`, Client: `hrm-web`)
  - Database: PostgreSQL (`hrm_baseline_db`)
- **Tài khoản kiểm thử (UAT Admin)**: `admin` / `Admin@123456`
- **Trạng thái cấu hình**: `UseMockAuth: false` (Sử dụng Keycloak thật)

---

## 2. Tóm tắt kết quả kiểm thử (UAT Summary)

| Ca kiểm thử | Tên ca kiểm thử | Nhân viên UAT & ID thực tế | Kết quả mong đợi | Trạng thái | Minh chứng & Lưu ý |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-01** | Chặn xóa khi có active subordinate (Cấp dưới hoạt động) | **MGR1636** (UAT_Delete_Manager_1636)<br>- ID: `616e0a4e-ff9d-4e84-ab5e-534ddbaf9fac`<br>Có subordinate **SUB1636** hoạt động. | Hệ thống chặn xóa ngay từ bước kiểm tra DB đầu tiên, không thay đổi DB, không gọi Keycloak. Hiển thị thông báo lỗi tiếng Anh đúng chuẩn: <br>`Cannot deactivate this employee because active subordinates are still assigned. Reassign them before deleting the employee.` | **PASS** | Toast lỗi hiển thị tiếng Anh trên UI chính xác.<br>Minh chứng: [Ảnh chụp màn hình lỗi](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/screenshots/2026-07-09_tc01_active_subordinate_error.png) |
| **TC-02** | Xóa cứng (Hard Delete) nhân viên không có lịch sử HRM | **EMP003** (uat.provision86)<br>- Employee ID: `392c6220-9be5-480a-a82a-a77d09b32732`<br>- User ID: `c4ea0c11-5576-44f9-bbf2-49490ea1410a`<br>- Identity ID: `28ce7cba-1f8b-4b3a-add5-8af240fa4b0d` | Nhân viên bị xóa hoàn toàn khỏi bảng `employee` trong DB. Tài khoản User liên kết bị soft-deleted (`is_deleted = true`). Tài khoản Keycloak tương ứng bị xóa sạch (`404 Not Found` trên Admin API). | **Behavior PASS; cleanup/no restore requested** | Kiểm tra DB và Keycloak:<br>- `employee` không còn dòng nào.<br>- `user` có `is_deleted = true`.<br>- Keycloak Admin API GET trả về 404 sau khi xóa. Xem chi tiết raw SQL/API output bên dưới. Người dùng xác nhận **không cần khôi phục** tài khoản này. |
| **TC-03** | Vô hiệu hóa mềm (Soft Deactivate) nhân viên có lịch sử HRM | Disposable employee có HRM history.<br>Supplemental UAT được User xác nhận ngày `2026-07-10`.<br><br>Historical run trước đó dùng nhầm **EMP-CEO-TEST** được giữ lại ở mục 3-5 để truy vết. | Nhân viên được cập nhật trạng thái `IsActive = False` để bảo toàn lịch sử dữ liệu. Tài khoản User liên kết bị soft-deleted (`is_deleted = true`). Tài khoản Keycloak tương ứng bị xóa sạch (`404 Not Found` trên Admin API). | **PASS (User-confirmed supplemental UAT)** | User xác nhận TC-03 đã PASS sau khi chạy lại UAT. Lưu ý: raw ID/log chi tiết của disposable employee bổ sung không được Codex trực tiếp thu thập trong lượt cập nhật report này. |
| **TC-04** | Xử lý lỗi Keycloak 404 (Idempotent Keycloak Delete) | Disposable employee có Keycloak user đã bị xóa trước khi offboarding.<br>Supplemental UAT được User xác nhận ngày `2026-07-10`. | Cuộc gọi Keycloak trả về lỗi 404 (Not Found) được coi là thành công. Giao dịch DB được hoàn tất bình thường và không gây lỗi (crash) hệ thống. | **PASS (User-confirmed runtime UAT)** | User xác nhận runtime case Keycloak DELETE 404 đã PASS. Logic xử lý cũng đã được kiểm chứng ở mã nguồn (`AuthenticationService` xử lý lỗi 404 như `Result.Success`). |

---

## 3. Chi tiết kết quả kiểm thử và Minh chứng Kỹ thuật ban đầu

### TC-01: Chặn xóa khi có cấp dưới hoạt động
- **Thao tác**:
  1. Đăng nhập tài khoản Admin vào hệ thống.
  2. Truy cập danh sách nhân viên (`/employee`).
  3. Chọn nút **Remove** bên cạnh nhân viên `UAT_Delete_Manager_1636` (Code: `MGR1636`) và xác nhận.
- **Kết quả thực tế**:
  - Hệ thống ngay lập tức hiện toast thông báo lỗi tiếng Anh ngăn chặn:
    `Cannot deactivate this employee because active subordinates are still assigned. Reassign them before deleting the employee.`
  - Cả hai nhân viên vẫn ở trạng thái `IsActive = True` trong DB.
- **Minh chứng UI**:
  ![Toast Error TC-01](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/screenshots/2026-07-09_tc01_active_subordinate_error.png)

### TC-02: Xóa cứng (Hard Delete) - EMP003
- **Thao tác**: Chọn nút **Remove** bên cạnh nhân viên `EMP003` (Full Name: `uat.provision86`) và xác nhận.
- **Minh chứng DB (Post-Delete)**:
  - Truy vấn bảng `employee`:
    ```sql
    SELECT * FROM employee WHERE employee_code = 'EMP003';
    -- Kết quả: 0 dòng (Bản ghi đã bị hard-delete khỏi bảng employee thành công)
    ```
  - Truy vấn bảng `user`:
    ```sql
    SELECT id, username, email, is_deleted, identity_id 
    FROM "user" 
    WHERE id = 'c4ea0c11-5576-44f9-bbf2-49490ea1410a';
    -- Kết quả thực tế từ DB:
    -- id: c4ea0c11-5576-44f9-bbf2-49490ea1410a
    -- username: nguyenvanb
    -- email: nguyenvanb@hrm.local
    -- is_deleted: true (User liên kết bị soft-deleted thành công)
    -- identity_id: 28ce7cba-1f8b-4b3a-add5-8af240fa4b0d
    ```
- **Minh chứng Keycloak (Post-Delete)**:
  - **Xác minh sự tồn tại của User ID qua Admin API (GET)**:
    - Yêu cầu: `GET http://localhost:8080/admin/realms/hrm/users/28ce7cba-1f8b-4b3a-add5-8af240fa4b0d`
    - Mã phản hồi thực tế: **HTTP 404 Not Found**
    - Raw JSON response:
      ```json
      {
        "error": "User not found"
      }
      ```
      *(Bằng chứng xác thực tài khoản đã bị xóa cứng hoàn toàn khỏi Keycloak)*

### TC-03: Vô hiệu hóa mềm (Soft Deactivate) - EMP-CEO-TEST (Lúc thực hiện UAT)
- **Ghi chú trạng thái hiện tại**: Đây là historical run dùng nhầm dữ liệu thật và không còn là bằng chứng chính thức để kết luận PASS. Trạng thái PASS cuối cùng của TC-03 dựa trên supplemental UAT với disposable employee được User xác nhận ngày `2026-07-10`.
- **Thao tác**: Chọn nút **Remove** bên cạnh nhân viên `EMP-CEO-TEST` (Full Name: `CEO Test`) và xác nhận.
- **Minh chứng DB (Post-Delete)**:
  - Truy vấn bảng `employee`:
    ```sql
    SELECT id, employee_code, full_name, is_active, user_id 
    FROM employee 
    WHERE employee_code = 'EMP-CEO-TEST';
    -- Kết quả thực tế từ DB:
    -- id: 5af94df7-f990-4bb8-b131-93dee211bdc9
    -- employee_code: EMP-CEO-TEST
    -- full_name: CEO Test
    -- is_active: false (Chuyển sang trạng thái vô hiệu hóa mềm)
    ```
  - Truy vấn bảng `user`:
    ```sql
    SELECT id, username, email, is_deleted, identity_id 
    FROM "user" 
    WHERE id = 'efd16b01-e5f1-4f56-a714-92b33787ff7b';
    -- Kết quả thực tế từ DB:
    -- is_deleted: true (User liên kết bị soft-deleted)
    ```
  - Số dư phép năm (`leave_balance`) và đơn phép (`leave_request`) vẫn được giữ nguyên vẹn nhằm bảo toàn lịch sử HRM.

---

## 4. Hiện trạng dữ liệu sau khi Phục hồi & Dọn dẹp (Post-UAT Restoration & Cleanup)

Sau khi hoàn tất đợt chạy UAT và phát hiện việc dùng nhầm dữ liệu thực tế, hệ thống đã thực hiện quy trình khôi phục tài khoản CEO và dọn dẹp dữ liệu rác kiểm thử theo phê duyệt của người dùng.

### 4.1. Phục hồi tài khoản phê duyệt CEO (`ceo.test / EMP-CEO-TEST`)
- **Keycloak**: 
  - Tạo mới lại user `ceo.test` với email `ceo.test@hrm.local`.
  - Cập nhật thông tin profile bắt buộc: `firstName: "CEO"`, `lastName: "Test"`.
  - Đặt password: `Admin@123456` (`temporary: false`).
  - Gán **Keycloak user id / subject id mới**: `e3bf34eb-5f6a-42c0-a890-0e24b7b025cd`.
  - *Không thực hiện gán Keycloak roles* do hệ thống phân quyền (App Authorization) hoàn toàn độc lập và dựa trên DB Roles.
- **Cơ sở dữ liệu (DB)**:
  - Khôi phục trạng thái hoạt động của Employee (`is_active = true`).
  - Khôi phục trạng thái hoạt động của User (`is_deleted = false`).
  - Cập nhật trường `identity_id` sang **Keycloak user id / subject id mới** (`e3bf34eb-5f6a-42c0-a890-0e24b7b025cd`).
  - Giữ nguyên liên kết vai trò cũ trong bảng `user_to_role` và cấu hình phân công phê duyệt trong bảng `leave_approver_assignment`.

### 4.2. Dọn dẹp dữ liệu kiểm thử rác (`MGR1636 / SUB1636`)
- Xóa hoàn toàn bản ghi `SUB1636` và `MGR1636` khỏi bảng `employee`. Do hai bản ghi này không liên kết với các bảng nghiệp vụ khác (như đơn nghỉ phép hay tài khoản ứng dụng), việc xóa cứng được thực thi thành công và không ảnh hưởng đến toàn vẹn dữ liệu.

### 4.3. Xử lý tài khoản `EMP003 / nguyenvanb`
- Người dùng xác nhận **không yêu cầu khôi phục** tài khoản này. Tài khoản tiếp tục được giữ nguyên trạng thái đã xóa cứng khỏi bảng `employee`, tài khoản User ứng dụng ở trạng thái soft-deleted (`is_deleted = true`) và đã xóa cứng khỏi Keycloak.

### 4.4. Xóa bỏ các Debug Scripts chứa Credentials nhạy cảm
- Đã xóa toàn bộ các tệp tin script kiểm tra và tương tác DB/Keycloak local được tạo ra trong ngày 2026-07-09 tại thư mục `MD_memory/debug/` để tránh lộ lọt credentials. Các minh chứng/evidence hiện tại chỉ còn là các snapshot/log đã trích xuất trong báo cáo này.

---

## 5. Minh chứng kỹ thuật cho Phục hồi & Dọn dẹp (Restoration & Cleanup Evidence)

### 5.1. Phục hồi tài khoản CEO `ceo.test`
- **Truy vấn kiểm tra trạng thái DB**:
  ```sql
  SELECT e.employee_code, e.full_name, e.is_active, u.username, u.is_deleted, u.identity_id 
  FROM employee e
  JOIN "user" u ON e.user_id = u.id
  WHERE e.employee_code = 'EMP-CEO-TEST';
  ```
  *Kết quả trả về*:
  ```text
  employee_code | full_name | is_active | username | is_deleted | identity_id
  --------------+-----------+-----------+----------+------------+-------------------------------------
  EMP-CEO-TEST  | CEO Test  | true      | ceo.test | false      | e3bf34eb-5f6a-42c0-a890-0e24b7b025cd
  ```
- **Xác thực Đăng nhập Keycloak (Password Grant)**:
  - Gửi yêu cầu đăng nhập bằng password grant sử dụng credentials `ceo.test` / `Admin@123456` qua API Keycloak.
  - Phản hồi nhận được: **HTTP 200 OK** kèm theo Access Token hợp lệ.
  ```text
  === Attempting authentication for ceo.test ===
  Authentication Status: SUCCESS
  Access Token generated (first 30 chars): eyJhbGciOiJSUzI1NiIsInR5cCIgOi...
  ```

### 5.2. Dọn dẹp dữ liệu `MGR1636 / SUB1636`
- **Truy vấn DB sau dọn dẹp**:
  ```sql
  SELECT employee_code, full_name, is_active FROM employee WHERE employee_code IN ('MGR1636', 'SUB1636');
  ```
  *Kết quả trả về*: **0 dòng (0 rows)**.

### 5.3. Danh sách các tệp debug ngày 2026-07-09 đã xóa
Toàn bộ các tệp sau đã được gỡ bỏ khỏi thư mục `MD_memory/debug/` theo yêu cầu bảo mật thông tin credentials nhạy cảm. Toàn bộ bằng chứng/evidence về việc kiểm tra trước đó hiện còn lại dưới dạng snapshot/log đã được trích trong báo cáo này:
- `2026-07-09_1622_query_employees_uat.py`
- `2026-07-09_1622_setup_tc01_subordinate.py`
- `2026-07-09_1645_check_history_active.py`
- `2026-07-09_1645_query_employees_with_users.py`
- `2026-07-09_1650_check_post_delete_details.py`
- `2026-07-09_1655_check_keycloak_post_delete.py`
- `2026-07-09_1655_check_keycloak_post_delete.ps1`
- `2026-07-09_1720_inventory_ceo_test.py`
- `2026-07-09_1730_temp_restore_keycloak.py`
- `2026-07-09_1740_check_delete_mgr_sub.py`
- `2026-07-09_1745_check_mgr_sub_columns.py`
- `2026-07-09_1750_execute_db_mutations.py`
- `2026-07-09_1800_verify_db_mutations.py`
- `2026-07-09_1810_reset_ceo_password.py`

---

## 6. Trạng thái Git Workspace hiện tại (Workspace Git Status)

- Workspace hiện tại **vẫn ở trạng thái không sạch (dirty/untracked)** do các tệp tin từ các phase phát triển trước đó (đặc biệt là module `WorkCalendar` chưa hoàn tất commit).
- Trong lượt hậu xử lý report/cleanup này, chỉ file report UAT được chỉnh sửa; các runtime files đang dirty là thay đổi tồn đọng từ các phase trước và không được stage/commit/push.
- Chỉ riêng các tệp tin debug được tạo trong ngày 2026-07-09 là đã được xóa sạch.
- **Tóm tắt kết quả lệnh `git status --short`** (Trích xuất các tệp tin liên quan trực tiếp đến Task này):
  - Các tệp tin mã nguồn có thay đổi (chưa staged):
    * `HRM_Leave_Management/Application/Employees/Delete/DeleteEmployeeCommandHandler.cs`
    * `HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`
    * `HRM_Leave_Management/Domain/Employees/EmployeeErrors.cs`
    * `HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs`
  - Tệp báo cáo UAT có thay đổi:
    * `MD_memory/reports/2026-07-09_1640_phase-3d_employee-offboarding-uat_report.md`
  - Các tệp tin untracked khác thuộc module `WorkCalendar` từ phase trước vẫn giữ nguyên trạng thái.

---

## 7. Xác nhận các cam kết thiết kế & Phân quyền (App Authorization)

1.  **Kiến trúc DB-First, Keycloak-After**:
    Quy trình offboarding thực thi DB-first trước, Keycloak-after sau. Trong trường hợp Keycloak lỗi, DB vẫn ở trạng thái vô hiệu hóa.
2.  **Cơ chế Phân quyền (App Authorization)**:
    Hệ thống phân quyền của ứng dụng được quản lý hoàn toàn ở cơ sở dữ liệu dựa trên DB roles thông qua chuỗi liên kết:
    `user_to_role -> role -> role_to_permission`
    Chúng ta không thực hiện gán hay cấu hình Keycloak roles cho tài khoản người dùng vì ứng dụng không sử dụng phân quyền phía Keycloak.
3.  **Intentional Technical Debt**:
    Lọc in-memory `ToListAsync()` tại `AdminLoginCommandHandler` được giữ lại có chủ đích nhằm giải quyết vấn đề case-insensitive cho username ở mức DB mà không làm ảnh hưởng đến collation mặc định.

---

## 8. Kết luận UAT

Kiểm thử UAT quy trình **Employee Offboarding** đã đạt trạng thái **PASS toàn bộ 4/4 test cases** theo bằng chứng đã ghi nhận trong report và xác nhận supplemental UAT của User ngày `2026-07-10`.

- **TC-01**: PASS với bằng chứng UI toast chặn active subordinate.
- **TC-02**: PASS với bằng chứng DB và Keycloak post-delete.
- **TC-03**: PASS theo xác nhận User sau supplemental UAT bằng disposable employee có HRM history.
- **TC-04**: PASS theo xác nhận User sau supplemental runtime UAT cho Keycloak DELETE 404.

Lưu ý về mức độ bằng chứng: TC-03 và TC-04 đã được cập nhật trạng thái theo xác nhận trực tiếp của User; Codex không trực tiếp thu thập raw ID/log bổ sung trong lượt cập nhật report này.

Không có thay đổi nào đối với mã nguồn chạy trực tiếp (runtime code) trong quá trình khôi phục và dọn dẹp này; do đó dự án không chạy lại lệnh biên dịch (build).
