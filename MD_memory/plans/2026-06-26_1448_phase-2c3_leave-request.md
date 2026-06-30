# Kế Hoạch Triển Khai - Phase 2C.3: Quản Lý Đơn Nghỉ Phép (Leave Request)

- **Thời gian lập**: 2026-06-26 14:48 (Cập nhật: 2026-06-29 14:09)
- **Người lập**: Antigravity (Senior .NET Fullstack Engineer)
- **Trạng thái**: **UAT PASS / CHỜ USER APPROVE** ✅
- **Điều kiện vào phase**: Phase 2C.2 implementation done, main UAT passed.
- **Kết quả UAT**: Codex/User đã UAT toàn bộ test case qua HTTP session thật trên Web.Backend. Auth mode: Keycloak thật, `UseMockAuth=false`. Accounts: `admin` / `Admin@123456` và `employee` / `Admin@123456`. Routes test: `/leave-request`, `/leave-balance`. Tất cả case PASS.
- **Mục tiêu**: Thiết lập tính năng đăng ký và quản lý đơn nghỉ phép (Leave Request) dành cho nhân viên, tự động tính toán số ngày nghỉ hỗ trợ nghỉ nửa ngày, thực thi 8 quy tắc validation nghiệp vụ nghiêm ngặt, cho phép hủy đơn đang chờ duyệt, và phân quyền hiển thị (self-view vs admin-view).

---

## 📋 1. Nghiệp Vụ & Yêu Cầu Tính Năng

Theo các quyết định nghiệp vụ đã thống nhất trong `plans/2026-06-25_1515_phase-2c_leave-management-plan.md`:

1. **Mô hình nghỉ nửa ngày (`LeaveDayPart`)**:
   - Định nghĩa enum `LeaveDayPart`: `FullDay = 1`, `Morning = 2`, `Afternoon = 3`.
   - Một đơn nghỉ phép được xác định bởi: `StartDate`, `EndDate`, `StartDayPart` và `EndDayPart`.
   - **Cách tính tổng số ngày nghỉ (`TotalDays`)**:
     - `dayValue(part) = (part == FullDay) ? 1.0 : 0.5`
     - Nếu `StartDate == EndDate`:
       - Yêu cầu `StartDayPart == EndDayPart`. (Chặn Morning + Afternoon trong cùng một ngày).
       - `TotalDays = dayValue(StartDayPart)` (Có thể là 0.5 hoặc 1.0).
     - Nếu `StartDate < EndDate`:
       - `middleDays = EndDate.DayNumber - StartDate.DayNumber - 1` (Số ngày trọn vẹn ở giữa, tính cả T7/CN/ngày lễ theo quy tắc BD-8 - tính theo calendar days).
       - `TotalDays = dayValue(StartDayPart) + middleDays + dayValue(EndDayPart)`.
2. **Quy tắc tính khả dụng và số dư khả dụng (`AvailableDays`)**:
   - `AvailableDays = TotalAllocated - UsedDays - PendingDays`
   - Trong đó:
     - `TotalAllocated` và `UsedDays` được lấy từ bảng `leave_balance` của nhân viên ứng với loại phép và năm đó (`StartDate.Year`).
     - `PendingDays` là tổng số ngày nghỉ của các đơn ở trạng thái `Pending` của nhân viên đó, cùng loại phép và cùng năm nghỉ phép.
   - `UsedDays` trong Phase 2C.3 không tự động tăng (chờ duyệt ở Phase 3). Tuy nhiên, đơn ở trạng thái `Pending` sẽ tạm thời khóa số ngày phép tương ứng thông qua `PendingDays`.
3. **Trạng thái đơn nghỉ phép (`LeaveRequestStatus`)**:
   - `Pending = 1` (Mặc định khi tạo mới).
   - `Approved = 2` (Dành cho Phase 3).
   - `Rejected = 3` (Dành cho Phase 3).
   - `Canceled = 4` (Nhân viên tự hủy khi đơn đang ở trạng thái `Pending`).
4. **Hủy đơn nghỉ phép**:
   - Chỉ cho phép hủy (`Canceled`) các đơn nghỉ phép đang ở trạng thái `Pending`.
   - Không cho phép hủy các đơn đã `Approved`, `Rejected`, hoặc đã `Canceled` trước đó.

---

## 🔑 2. Phân Quyền (Permission-driven) — ĐÃ CHỐT

| Permission | Hành vi trong Phase 2C.3 | Gán cho Role |
| :--- | :--- | :--- |
| `VIEW_LEAVE_REQUEST` | Nhân viên xem danh sách đơn nghỉ phép **của chính mình** (self-view). Ẩn bộ lọc theo nhân viên khác. | `EMPLOYEE_SELF_VIEW`, `ADMIN` |
| `CREATE_LEAVE_REQUEST` | Tạo đơn xin nghỉ phép mới. | `EMPLOYEE_SELF_VIEW`, `ADMIN` |
| `APPROVE_LEAVE_REQUEST` | Xem toàn bộ danh sách đơn nghỉ phép của tất cả nhân viên kèm bộ lọc. **Chưa triển khai approve/reject UI/logic cho tới Phase 3.** | `ADMIN` |

**Quy tắc rẽ nhánh trong Controller/Handler**:
```
Nếu user có APPROVE_LEAVE_REQUEST:
  → Hiển thị toàn bộ danh sách đơn nghỉ, bộ lọc theo nhân viên/phòng ban/trạng thái.
  → Chưa có nút Approve/Reject (Phase 3).

Nếu user chỉ có VIEW_LEAVE_REQUEST (không có APPROVE_LEAVE_REQUEST):
  → Lấy IdentityId từ IUserContext.IdentityId.
  → Tìm User → tìm Employee.
  → Chỉ hiển thị đơn nghỉ phép của chính Employee đó.
  → Ẩn bộ lọc theo nhân viên khác.

Nếu user không có cả hai:
  → Redirect /NoPermission.
```

**Lưu ý quan trọng**:
- Tuyệt đối không dùng `UPDATE_LEAVE_BALANCE` để mở quyền xem toàn bộ danh sách LeaveRequest. Hai module có permission tách biệt.
- Không check role name, username, email, user id, hoặc magic GUID trong code. Toàn bộ rẽ nhánh dựa trên permission check qua `IRoleService.checkRoleExist`.
- "Admin/HR" chỉ là thuật ngữ mô tả nghiệp vụ, không phải logic code.

---

## 🗄️ 3. Thiết Kế Database Schema

Bảng mới: `leave_request`

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | Primary Key | Khóa chính |
| `employee_id` | `uuid` | FK (`employee.id`), Not Null | Nhân viên đăng ký nghỉ |
| `leave_type_id` | `uuid` | FK (`leave_type.id`), Not Null | Loại nghỉ phép |
| `start_date` | `date` | Not Null | Ngày bắt đầu nghỉ |
| `end_date` | `date` | Not Null | Ngày kết thúc nghỉ |
| `start_day_part` | `integer` | Not Null | Buổi bắt đầu (`LeaveDayPart`) |
| `end_day_part` | `integer` | Not Null | Buổi kết thúc (`LeaveDayPart`) |
| `total_days` | `numeric(18,2)` | Not Null | Tổng số ngày nghỉ (đã tính toán) |
| `reason` | `varchar(1000)` | Nullable | Lý do xin nghỉ |
| `status` | `integer` | Not Null, Default 1 | Trạng thái (`LeaveRequestStatus`) |
| `approved_by_id` | `uuid` | FK (`employee.id`), Nullable | Người duyệt (Phase 3) |
| `approver_comment` | `varchar(1000)` | Nullable | Ý kiến người duyệt (Phase 3) |
| `approved_date` | `timestamp` | Nullable | Ngày duyệt (Phase 3) |
| `is_active` | `boolean` | Not Null, Default true | Soft delete |
| `created_date` | `timestamp` | Not Null, Default UTC | Thời gian tạo đơn |

**Chỉ mục (Index)**:
- Overlap check: `ix_leave_request_employee_dates ON leave_request (employee_id, start_date, end_date) WHERE is_active = true;`
- PendingDays calc: `ix_leave_request_pending_calc ON leave_request (employee_id, leave_type_id, status) WHERE is_active = true AND status = 1;` *(Pending = 1 theo enum thực tế)*

---

## 🏗️ 4. Kiến Trúc Lớp (Clean Architecture)

### 4.1. Tầng Domain
- `Domain/LeaveRequests/LeaveRequestId.cs` — Value Object bọc `Guid`.
- `Domain/LeaveRequests/LeaveDayPart.cs` — Enum (`FullDay=1`, `Morning=2`, `Afternoon=3`).
- `Domain/LeaveRequests/LeaveRequestStatus.cs` — Enum (`Pending=1`, `Approved=2`, `Rejected=3`, `Canceled=4`).
- `Domain/LeaveRequests/LeaveRequest.cs` — Entity với phương thức `Cancel()`.
- `Domain/LeaveRequests/ILeaveRequestRepository.cs` — Interface repository.
- `Domain/LeaveRequests/LeaveRequestErrors.cs` — Domain errors.

### 4.2. Tầng Infrastructure
- `Infrastructure/Configurations/LeaveRequestConfiguration.cs` — EF mapping, FK, index.
- `Infrastructure/Repositories/LeaveRequestRepository.cs` — Implementation.
- Đăng ký DI trong `Infrastructure/DependencyInjection.cs`.
- EF Core Migration `AddLeaveRequest`.

### 4.3. Tầng Application (CQRS & MediatR)

**Commands:**
1. `CreateLeaveRequestCommand` + Handler — Tạo đơn nghỉ với bộ validation V-1 đến V-8.
2. `CancelLeaveRequestCommand` + Handler — Hủy đơn Pending (check C-1).

**Queries:**
1. `GetLeaveRequestsQuery` + Handler — Danh sách đơn nghỉ (self-view hoặc admin-view tùy permission).

---

## ⚡ 5. Validation Rules

### 5.1. Khi tạo LeaveRequest

| Rule | Kiểm tra | Lỗi trả về |
|------|---------|------------|
| V-1 | Resolve `UserContext.IdentityId → User → Employee` (active) | `EmployeeNotFound` |
| V-2 | `StartDate <= EndDate` | `DateOrderInvalid` |
| V-3 | `StartDate >= businessToday` (xem mục 5.2) | `PastDateNotAllowed` |
| V-4 | `StartDate.Year == EndDate.Year` | `CrossYearNotAllowed` |
| V-5 | Nếu `StartDate == EndDate`: `StartDayPart == EndDayPart` | `DayPartMismatch` |
| V-6 | Không trùng khoảng ngày với đơn `Pending` hoặc `Approved` cùng employee | `OverlapDetected` |
| V-8 | Phải có LeaveBalance cho employee + leaveType + year | `NoLeaveBalance` |
| V-7 | `TotalDays <= AvailableDays` (`TotalAllocated - UsedDays - PendingDays`) | `InsufficientBalance` |

### 5.2. Business Date cho rule V-3 (Không xin nghỉ quá khứ)

Project đã có clock abstraction:
- Interface: `Application/Abstractions/Clock/IDateTimeProvider` — property `UtcNow` (trả `DateTime`).
- Implementation: `Infrastructure/Clock/DateTimeProvider` — `DateTime.UtcNow`.
- Đã đăng ký DI: `Infrastructure/DependencyInjection.cs` dòng 86.

**Quyết định**: Handler `CreateLeaveRequestCommandHandler` sẽ inject `IDateTimeProvider` thay vì hardcode `DateTime.UtcNow`.
- Lấy `businessToday = DateOnly.FromDateTime(_dateTimeProvider.UtcNow)`.
- Rule V-3: `StartDate >= businessToday`.
- Lưu ý: Business date dựa trên UTC. Nếu sau này cần chuyển sang giờ Việt Nam (UTC+7), có thể dùng `_dateTimeProvider.ToVnTime(...)` rồi convert sang `DateOnly`.

### 5.3. Khi cancel

| Rule | Kiểm tra | Lỗi trả về |
|------|---------|------------|
| C-1 | `Status == Pending` | `NotPendingStatus` |

---

## 🖥️ 6. Giao Diện UI/UX

### 6.1. Route & Navigation
- **Route**: `/leave-request`
- **Sidebar**: Thêm mục "Leave Requests" dẫn tới `/leave-request`.

### 6.2. Trang Danh Sách (`Index.cshtml`)
- Bảng: Mã NV | Họ tên | Loại phép | Từ ngày | Đến ngày | Tổng ngày | Trạng thái (Badge) | Hành động.
- Badge: `Pending` = vàng, `Canceled` = xám, `Approved` = xanh lá (Phase 3), `Rejected` = đỏ (Phase 3).
- Nút "Cancel" chỉ hiển thị với đơn `Pending`, có modal xác nhận.

### 6.3. Trang Tạo Đơn (`Create.cshtml`)
- Dropdown loại phép (chỉ active). Date picker cho StartDate/EndDate.
- Dropdown StartDayPart/EndDayPart. Textarea lý do.
- JS tính TotalDays preview trên client.

---

## 🧪 7. Kịch Bản Kiểm Thử (UAT Checklist)

| STT | Kịch Bản | Dữ Liệu | Kết Quả Mong Đợi | Trạng Thái |
| :--- | :--- | :--- | :--- | :---: |
| 1 | Tạo đơn hợp lệ (nguyên ngày) | AL, 07-01→07-02 (FullDay→FullDay) | `total_days=2.0`, status `Pending` | ✅ Pass |
| 2 | Tạo đơn nửa ngày cùng ngày | AL, 07-01→07-01 (Morning→Morning) | `total_days=0.5` | ✅ Pass |
| 3 | Tạo đơn 1.0 ngày (lẻ) | AL, 07-01→07-02 (Afternoon→Morning) | `total_days=1.0` (0.5+0.5) | ✅ Pass |
| 4 | **Tạo đơn 1.5 ngày** | AL, 07-01→07-02 (Afternoon→FullDay) | `total_days=1.5` (0.5+0+1.0) | ✅ Pass |
| 5 | Chặn EndDate < StartDate (V-2) | Start: 07-02, End: 07-01 | Lỗi: "The start date must be before or equal to the end date" | ✅ Pass |
| 6 | Chặn ngày quá khứ (V-3) | Start: ngày hôm qua | Lỗi: "Creating a leave request in the past is not allowed" | ✅ Pass |
| 7 | Chặn bắc qua năm (V-4) | Start: 2026-12-31, End: 2027-01-01 | Lỗi: "A leave request cannot cross multiple calendar years" | ✅ Pass |
| 8 | Chặn Morning+Afternoon cùng ngày (V-5) | 07-01→07-01 (Morning→Afternoon) | Lỗi: "Invalid session selection for a single-day request" | ✅ Pass |
| 9 | Chặn trùng đơn Pending/Approved (V-6) | Đơn 1: 07-01→07-03. Đơn 2: trùng 07-02 | Lỗi: "This request overlaps with an existing pending or approved leave request" | ✅ Pass |
| 10 | Chặn vượt AvailableDays (V-7) | Allocated:12, Used:2, Pending:9→Còn 1. Xin 2 ngày | Lỗi: "Insufficient available leave days for this request" | ✅ Pass |
| 11 | Hủy đơn Pending thành công | Chọn đơn Pending → Cancel | Status → `Canceled`, PendingDays giải phóng | ✅ Pass |
| 12 | Chặn hủy đơn đã Canceled (C-1) | Cancel đơn status=Canceled | Lỗi: reject đúng (đơn không ở Pending) | ✅ Pass |
| 13 | Self-view phân quyền | Login Employee (`employee`) | Chỉ thấy đơn của mình (EMP002), ẩn bộ lọc NV | ✅ Pass |
| 14 | Admin-view phân quyền | Login Admin (`admin`) | Thấy toàn bộ đơn, có Employee filter, không có Approve/Reject (Phase 3) | ✅ Pass |
| 15 | Calendar days (qua cuối tuần) | Fri 2026-10-02 → Mon 2026-10-05 (FullDay→FullDay) | `total_days=4.0` (không loại T7/CN) | ✅ Pass |
| 16 | Leave Balance sau cancel | Cancel 1 đơn Pending → kiểm tra Balance | AvailableDays tăng trở lại đúng | ✅ Pass |

---

## 🔐 8. Auth/UAT Safety

### 8.1. Cấu hình Auth bắt buộc
- **`UseMockAuth: false`** — Dùng Keycloak thật cho mọi kiểm thử UAT.
- **Keycloak Local**: `http://localhost:8080`, Realm `hrm`, Client `hrm-web`.

### 8.2. Tài khoản UAT
- **Keycloak Management Admin (realm `master`)**: Username `admin`, password `admin`. KHÔNG ĐƯỢC sửa/reset.
- **HRM App Admin (realm `hrm`)**: Username `admin` hoặc `admin@hrm.local`, password `Admin@123456`. KHÔNG ĐƯỢC sửa.
- **Employee test**: Sử dụng tài khoản `employee` (đã tồn tại từ Phase 2C.2, mapping DB `EMP002`).

### 8.3. Quy tắc nghiêm ngặt
- **KHÔNG** tự tạo/sửa/reset/xóa user Keycloak nếu chưa được user xác nhận.
- **KHÔNG** bật `UseMockAuth: true` để bypass UAT.
- **KHÔNG** sửa `JwtService`, `JwtBearerOptionsSetup`, `UserContext`, hoặc auth config nếu task không phải auth task.
- Nếu thiếu user test cho UAT (ví dụ cần thêm `employee1@hrm.local`, `employee2@hrm.local`, `employee3@hrm.local`), PHẢI dừng lại hỏi user xác nhận trước khi tạo.
- Nếu gặp **403**: Kiểm tra bảng `permission`, `role_to_permission`, `user_to_role` trước. Không xem 403 là lỗi login nếu user đã đăng nhập thành công.
- Trước khi mở browser UAT, phải seed permission `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST` cho role tương ứng.

### 8.4. UAT Test Users — Hiện trạng
- `employee` (đã tồn tại Keycloak realm `hrm`, đã mapping DB user + employee `EMP002` từ Phase 2C.2).
- `employee1@hrm.local`, `employee2@hrm.local`, `employee3@hrm.local` — **chỉ được dùng nếu đã tồn tại hoặc được user xác nhận tạo mới**. Nếu chưa tồn tại, phải hỏi user trước khi thao tác Keycloak.

---

## 🛠️ 9. Kế Hoạch Thực Hiện

1. GitNexus Impact Analysis trên `ApplicationDbContext`, `DependencyInjection`.
2. Tạo Domain files (Entity, Enums, Repository interface, Errors).
3. Tạo Application Commands & Queries.
4. Tạo Infrastructure (Configuration, Repository, DI registration).
5. Chạy EF Migration `AddLeaveRequest`.
6. Xây dựng Controller + Views Razor.
7. Seed permissions cho UAT local.
8. Build + UAT theo checklist.
9. GitNexus `detect_changes()` + Báo cáo UAT.

---

## ⚠️ 10. Rủi Ro

| Rủi ro | Mức | Giảm thiểu |
|--------|-----|-----------|
| EF Core `DateOnly` → PostgreSQL `date` chưa verify thực tế | Thấp | Verify khi chạy migration, thêm `.HasColumnType("date")` nếu cần |
| Overlap check đã implement `Pending + Approved` | Thấp | Phase 3 cần đảm bảo logic vẫn đúng khi approve/reject thay đổi trạng thái đơn |
| Thiếu user test Keycloak cho multi-employee UAT | Thấp | Hỏi user xác nhận trước khi tạo thêm |
| `businessToday` dùng UTC có thể lệch 1 ngày so với giờ VN lúc 0h-7h | Thấp | Ghi nhận, Phase sau có thể chuyển sang VN timezone |
