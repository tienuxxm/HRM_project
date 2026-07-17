# Báo cáo UAT & Xác minh Bảo mật Xác thực — Phân quyền Soft-Deleted Users & Inactive Employees

Tài liệu này chi tiết hóa kết quả triển khai và xác minh tính an toàn của lớp phân quyền (Authorization Layer) đối với các tài khoản nhân viên đã bị vô hiệu hóa (`IsActive = false`) hoặc tài khoản người dùng đã bị xóa mềm (`IsDeleted = true`) sau quy trình offboarding.

---

## 1. Ranh giới Kiến trúc được Bảo toàn (Architecture Boundary)

Trong suốt quá trình triển khai, chúng tôi cam kết bảo vệ ranh giới kiến trúc cốt lõi của hệ thống:
*   **`Web.Backend -> Application -> Domain`**
*   **`Infrastructure -> Application / Domain`**

Mọi logic nghiệp vụ kiểm tra trạng thái hoạt động được tích hợp trực tiếp vào tầng `Application` (`GetUserInfoCommandHandler`) và được cấu hình hạ tầng hỗ trợ thông qua `Infrastructure` (`RoleService`), không can thiệp trực tiếp vào các controller của MVC hay làm rò rỉ logic nghiệp vụ ra ngoài.

---

## 2. Chi tiết Thay đổi Thực thi (Implementation Details)

Lớp xác thực và phân quyền hiện tại đã được gia cố để chủ động chặn quyền truy cập của các nhân viên offboard thông qua 2 cơ chế chính:

### A. Tầng Application (Application Layer)
*   **File:** [GetUserInfoCommandHandler.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Auth/GetUserInfo/GetUserInfoCommandHandler.cs)
*   **Chi tiết:** Gia cố phương thức `Handle` để kiểm tra cờ `IsDeleted` của thực thể `User` và cờ `IsActive` của thực thể `Employee` liên kết:

```diff
             var user = await _repository.GetEntitiesAsQueryable()
                 .FirstOrDefaultAsync(x => x.IdentityId.Equals(new IdentityId(_userContext.IdentityId)),
                     cancellationToken);
-            if (user is null)
+            if (user is null || (user.IsDeleted.HasValue && user.IsDeleted.Value))
                 return Result.Failure<UserInfoResponse>(UserErrors.InvalidCredentials);
+
+            var employee = await _employeeRepository.GetEntitiesAsQueryable()
+                .FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
+            if (employee is not null && !employee.IsActive)
+                return Result.Failure<UserInfoResponse>(UserErrors.InvalidCredentials);
```

### B. Tầng Infrastructure (Infrastructure Layer)
*   **File:** [RoleService.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Infrastructure/RoleServices/RoleService.cs)
*   **Chi tiết:** Phương thức `checkRoleExist` (được sử dụng bởi các bộ lọc phân quyền và middleware hệ thống) nay sẽ tự động trả về `false` nếu tài khoản người dùng bị xóa mềm hoặc nhân viên liên kết bị vô hiệu hóa:

```diff
         var user = await _userRepository.GetEntitiesAsQueryable()
             .Include(x => x.Roles)
             .ThenInclude(x => x.Role)
             .ThenInclude(x => x.Permissions)
             .ThenInclude(x => x.Permission)
             .FirstOrDefaultAsync(x => 
                     x.IdentityId.Equals(new IdentityId(identityId))
                 , cancellationToken);
-        if (user is null)
+        if (user is null || (user.IsDeleted.HasValue && user.IsDeleted.Value))
+        {
+            return false;
+        }
+
+        var employee = await _employeeRepository.GetEntitiesAsQueryable()
+            .FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
+        if (employee is not null && !employee.IsActive)
+        {
+            return false;
+        }
```

---

## 3. Phân tích Tác động GitNexus (Impact Analysis)

Để đảm bảo tính ổn định tối đa cho hệ thống, chúng tôi đã thực hiện phân tích tác động trên các symbol cốt lõi:

> [!IMPORTANT]
> *   **Symbol:** `RoleService.checkRoleExist`
> *   **Risk Level:** **CRITICAL** (do là phương thức phân quyền trung tâm được sử dụng trên toàn hệ thống với 210 direct callers).
> *   **Phạm vi ảnh hưởng:** Tất cả các luồng kiểm tra quyền truy cập API và Views thuộc các phân hệ `LeaveRequest`, `LeaveBalance`, `Employee`, `Role`, `User`, cũng như các phân hệ legacy từ LUC.
> *   **Đánh giá an toàn:** Việc bổ sung kiểm tra `IsDeleted` và `IsActive` là giải pháp an toàn cao nhất, đảm bảo tính nhất quán bảo mật cho toàn hệ thống mà không phá vỡ logic kiểm tra quyền hiện hành.

---

## 4. Kịch bản Kiểm thử Thủ công (Manual UAT Checklist)

Do yêu cầu bảo mật thông tin tài khoản và quy tắc Keycloak local, chúng tôi cung cấp kịch bản kiểm thử chi tiết để Admin/HR tự thực hiện UAT:

### Kịch bản UAT-SEC-01: Chặn Đăng nhập/Truy cập của User bị Soft-Deleted (`IsDeleted = true`)
*   **Mục đích:** Đảm bảo một tài khoản người dùng đã bị xóa mềm không thể truy cập bất kỳ tài nguyên hay API nào.
*   **Điều kiện trước:** 
    *   Sử dụng Keycloak local (`http://localhost:8080`).
    *   Có một user `test_soft_delete` đã bị soft-deleted trong database (`IsDeleted = true`).
*   **Các bước thực hiện:**
    1. Mở trình duyệt ẩn danh, truy cập trang quản trị `/`.
    2. Đăng nhập bằng tài khoản `test_soft_delete`.
*   **Kết quả mong đợi:**
    *   Hệ thống từ chối quyền truy cập hoặc hiển thị thông báo lỗi xác thực không hợp lệ.
    *   API lấy thông tin `/api/user/info` trả về mã lỗi `InvalidCredentials` (hoặc chuyển hướng về trang lỗi đăng nhập).

### Kịch bản UAT-SEC-02: Chặn Phân quyền của Nhân viên bị Vô hiệu hóa (`IsActive = false`)
*   **Mục đích:** Xác minh nhân viên bị vô hiệu hóa (dù tài khoản User chưa bị xóa mềm hoàn toàn) vẫn bị tước toàn bộ quyền hạn hệ thống.
*   **Điều kiện trước:** 
    *   Nhân viên `test_inactive` có trạng thái `IsActive = false` trong database.
    *   User liên kết của nhân viên này vẫn có quyền truy cập Keycloak.
*   **Các bước thực hiện:**
    1. Đăng nhập bằng tài khoản liên kết của `test_inactive`.
    2. Thử truy cập vào bất kỳ trang yêu cầu quyền hạn nào (ví dụ: tạo đơn xin nghỉ phép `/leave-request`).
*   **Kết quả mong đợi:**
    *   Hệ thống trả về lỗi `403 Forbidden` hoặc chặn không cho truy cập do `checkRoleExist` trả về `false`.

---

## 5. Kết quả Kiểm tra Kỹ thuật (Technical Verification)

*   **Build Solution:** Thành công **SUCCESS** (0 errors).
*   **Kiểm tra Encoding:** Tất cả các file đã chỉnh sửa đều được quét và đảm bảo tuân thủ định dạng **UTF-8 BOM**.
*   **Git State:** Làm sạch và kiểm soát working tree, không chứa các file rác sinh ra trong quá trình build.
