# Kế hoạch Phase 2C: Xây dựng Module Quản lý Phép (Leave Management) — v5-final

- **Ngày tạo:** 2026-06-25
- **Cập nhật:** 2026-06-30 (v7 — đồng bộ trạng thái UAT, enum thực tế, overlap Pending+Approved)
- **Người lập kế hoạch:** Antigravity (Senior .NET Fullstack Engineer)
- **Phase:** Phase 2C
- **Trạng thái tổng:**
  - Phase 2C.1 LeaveType: ✅ UAT pass.
  - Phase 2C.2 LeaveBalance: ✅ Implementation done, main UAT pass, residual gaps documented.
  - Phase 2C.3 LeaveRequest: ✅ UAT pass / chờ user approve.
  - Phase 3 Approval Flow: ⏳ Chưa code, chờ plan được duyệt.
- **Thư mục Build/Run:** `HRM_Leave_Management`

---

## 0. Business Decisions Confirmed by User

| # | Quyết định | Chi tiết |
|---|-----------|----------|
| BD-1 | Cấp phát phép | Admin/HR cấp thủ công. Import Excel là hướng mở rộng sau. |
| BD-2 | Người tạo đơn | Nhân viên tự tạo cho chính mình. Không Admin tạo hộ. |
| BD-3 | Nghỉ nửa ngày | Dùng `StartDayPart` + `EndDayPart` (enum `LeaveDayPart`). Một đơn biểu diễn được 1.5 ngày. |
| BD-4 | Pending giữ chỗ | `AvailableDays = TotalAllocated - UsedDays - PendingDays`. `UsedDays` tăng Phase 3. |
| BD-5 | Hủy đơn | Chỉ cancel `Pending`. Không cancel `Approved`/`Rejected`/`Canceled`. |
| BD-6 | Permission | Seed sẵn `APPROVE_LEAVE_REQUEST`, ghi rõ chưa dùng cho tới Phase 3. |
| BD-7 | UAT data | Tạo nhiều employee test có mapping UserId → user Keycloak. |
| BD-8 | Calendar days | Tính theo calendar days, không loại trừ T7/CN/ngày lễ. Holiday calendar phase sau. |
| BD-9 | Không bắc qua năm | `StartDate.Year == EndDate.Year`. Khác năm → reject. |
| BD-10 | Trùng ngày | Phase 2C.3 đã implement overlap check chặn cả đơn `Pending` lẫn `Approved`. Không dùng chữ "active". Không Morning+Afternoon riêng cùng ngày. |
| BD-11 | UAT Keycloak test | Được phép tạo đúng 3 user local: `employee1@hrm.local`, `employee2@hrm.local`, `employee3@hrm.local`. Mapping Keycloak → bảng user → bảng employee. Không đổi admin. Không tạo user khác nếu chưa hỏi. |
| BD-12 | Mô hình nửa ngày | `StartDayPart`/`EndDayPart` thay `LeaveDayType` đơn. Cùng ngày: `FullDay+FullDay`=1, `Morning+Morning`=0.5, `Afternoon+Afternoon`=0.5. Không cho `Morning+Afternoon` cùng ngày. |
| BD-13 | Approval Phase 3 | Approve/Reject đầy đủ để Phase 3. Phase 2C chỉ Pending/Canceled. |

---

## 1. Phạm vi Phase 2C vs Phase 3 — ĐÃ CHỐT

### Phase 2C sẽ làm:
- CRUD LeaveType
- LeaveBalance: admin cấp phát thủ công
- LeaveRequest: nhân viên tự tạo, cancel Pending, xem danh sách
- Validation: overlap, quá khứ, bắc qua năm, AvailableDays (trừ PendingDays)
- Trạng thái Phase 2C: `Pending` và `Canceled`

### Phase 2C KHÔNG làm:
- Approve/Reject, trừ/hoàn phép, notification, dashboard HRM (Phase 3)

---

## 2. Employee.UserId — Giữ Nullable

**SQL đã chạy** (2026-06-25, DB `hrm_baseline_db`):
```sql
SELECT COUNT(*) AS total_employees,
       COUNT(CASE WHEN user_id IS NULL THEN 1 END) AS null_userid,
       COUNT(CASE WHEN user_id IS NOT NULL THEN 1 END) AS has_userid
FROM employee;
```
**Output:** 0 total, 0 null, 0 has_userid. Validate ở Application khi tạo LeaveRequest.

---

## 3. Thiết kế Domain Entities

### 3.1. Enum `LeaveDayPart` (BD-3, BD-12)
```csharp
public enum LeaveDayPart
{
    FullDay = 1,
    Morning = 2,
    Afternoon = 3
}
```
> **Historical note:** Thiết kế ban đầu (v5) dùng `FullDay=0, Morning=1, Afternoon=2`. Code thực tế đã implement `FullDay=1, Morning=2, Afternoon=3` (xem `Domain/LeaveRequests/LeaveDayPart.cs`).

### 3.2. Entity `LeaveType`
```csharp
public class LeaveType : Entity<LeaveTypeId>
{
    public string Name { get; private set; }
    public string Code { get; private set; }          // unique
    public int DefaultDays { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
}
```

### 3.3. Entity `LeaveBalance`
```csharp
public class LeaveBalance : Entity<LeaveBalanceId>
{
    public EmployeeId EmployeeId { get; private set; }
    public Employee Employee { get; private set; }
    public LeaveTypeId LeaveTypeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public int Year { get; private set; }
    public decimal TotalAllocated { get; private set; }
    public decimal UsedDays { get; private set; }       // Phase 2C: không tự tăng (chưa approve). Admin/HR có thể chỉnh thủ công. Phase 3: tự động tăng khi approve.
    public DateTime CreatedDate { get; private set; }
}
```
**Unique constraint:** `(EmployeeId, LeaveTypeId, Year)`

### 3.4. Entity `LeaveRequest` (CẬP NHẬT — StartDayPart/EndDayPart)
```csharp
public class LeaveRequest : Entity<LeaveRequestId>
{
    public EmployeeId EmployeeId { get; private set; }
    public Employee Employee { get; private set; }
    
    public LeaveTypeId LeaveTypeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public LeaveDayPart StartDayPart { get; private set; }  // FullDay, Morning, Afternoon
    public LeaveDayPart EndDayPart { get; private set; }    // FullDay, Morning, Afternoon
    public decimal TotalDays { get; private set; }           // Auto-computed
    public string? Reason { get; private set; }
    
    public LeaveRequestStatus Status { get; private set; }
    
    // Schema Phase 3
    public EmployeeId? ApprovedById { get; private set; }
    public Employee? ApprovedBy { get; private set; }
    public string? ApproverComment { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    
    public DateTime CreatedDate { get; private set; }
}
```

### 3.5. Enum `LeaveRequestStatus`
```csharp
public enum LeaveRequestStatus
{
    Pending = 1,
    Approved = 2,    // Phase 3
    Rejected = 3,    // Phase 3
    Canceled = 4
}
```
> **Historical note:** Thiết kế ban đầu (v5) dùng `Pending=0, Approved=1, Rejected=2, Canceled=3`. Code thực tế đã implement giá trị bắt đầu từ 1 (xem `Domain/LeaveRequests/LeaveRequestStatus.cs`).

---

## 4. Tính TotalDays từ StartDayPart/EndDayPart (BD-8, BD-12)

### 4.1. Công thức

```
dayValue(part) = (part == FullDay) ? 1.0 : 0.5

Nếu StartDate == EndDate:
    TotalDays = dayValue(StartDayPart)
    (StartDayPart phải == EndDayPart; không cho Morning+Afternoon cùng ngày)

Nếu StartDate < EndDate:
    middleDays = (EndDate.DayNumber - StartDate.DayNumber - 1)   // >= 0
    TotalDays = dayValue(StartDayPart) + middleDays + dayValue(EndDayPart)
```

### 4.2. Ví dụ

| StartDate | EndDate | StartDayPart | EndDayPart | TotalDays |
|-----------|---------|-------------|-----------|-----------|
| Thứ 5 | Thứ 5 | FullDay | FullDay | 1.0 |
| Thứ 5 | Thứ 5 | Morning | Morning | 0.5 |
| Thứ 5 | Thứ 6 | Afternoon | FullDay | 0.5 + 0 + 1.0 = 1.5 |
| Thứ 5 | Thứ 6 | FullDay | Morning | 1.0 + 0 + 0.5 = 1.5 |
| Thứ 5 | Thứ 6 | Afternoon | Morning | 0.5 + 0 + 0.5 = 1.0 |
| Thứ 4 | Thứ 6 | Afternoon | FullDay | 0.5 + 1 + 1.0 = 2.5 |

### 4.3. Quy tắc calendar days (BD-8)

Tính theo calendar days. Không loại trừ thứ Bảy, Chủ Nhật, ngày lễ. Holiday/working-day calendar để phase sau.

---

## 5. Validation Rules

### 5.1. Khi tạo LeaveRequest

| Rule | Kiểm tra | Lỗi trả về |
|------|---------|------------|
| V-1 Identity | Resolve `UserContext.IdentityId → User → Employee` | `UserNotFound` / `EmployeeNotLinkedToUser` / `EmployeeInactive` |
| V-2 Ngày hợp lệ | `StartDate <= EndDate` | `InvalidDateRange` |
| V-3 Không quá khứ | `StartDate >= today` | `CannotRequestPastDate` |
| V-4 Không bắc qua năm (BD-9) | `StartDate.Year == EndDate.Year` | `CrossYearNotAllowed` |
| V-5 DayPart cùng ngày (BD-12) | Nếu `StartDate == EndDate`: `StartDayPart == EndDayPart`. Không `Morning+Afternoon` | `InvalidDayPartCombination` |
| V-6 Overlap Pending (BD-10) | Không trùng khoảng ngày với đơn `Pending` cùng employee | `OverlappingPendingRequest` |
| V-7 Số dư (BD-4) | `TotalDays <= AvailableDays` = `TotalAllocated - UsedDays - PendingDays` | `InsufficientBalance` |
| V-8 Balance tồn tại | Phải có LeaveBalance cho employee + leaveType + year | `NoBalanceAllocated` |

### 5.2. Khi cancel

| Rule | Kiểm tra | Lỗi trả về |
|------|---------|------------|
| C-1 | `Status == Pending` | `NotPendingStatus` |

---

## 6. Overlap Detection (BD-10)

### 6.1. Query

```sql
SELECT COUNT(*)
FROM leave_request lr
WHERE lr.employee_id = @employeeId
  AND lr.status IN (1, 2)  -- Pending=1, Approved=2
  AND lr.start_date <= @endDate
  AND lr.end_date >= @startDate;
```

Nếu `COUNT > 0` → reject `OverlappingRequest`.

> **Ghi chú:** Code thực tế tại `CreateLeaveRequestCommandHandler` đã check `lr.Status == Pending || lr.Status == Approved`. Phase 3 cần đảm bảo logic này vẫn đúng khi approve/reject.

### 6.2. Rule 1 ngày 1 loại phép

Vì overlap check đã bao gồm bất kỳ đơn Pending hoặc Approved nào trùng ngày (không phân biệt loại phép), nên tự động enforce "1 ngày chỉ 1 đơn active" cho cùng employee.

---

## 7. Pending Days (BD-4)

```sql
SELECT COALESCE(SUM(lr.total_days), 0)
FROM leave_request lr
WHERE lr.employee_id = @employeeId
  AND lr.leave_type_id = @leaveTypeId
  AND EXTRACT(YEAR FROM lr.start_date) = @year
  AND lr.status = 1;  -- Pending (enum Pending = 1)
```

`AvailableDays = TotalAllocated - UsedDays - PendingDays`

---

## 8. Permissions — ĐÃ CHỐT

```
VIEW_LEAVE_TYPE, UPDATE_LEAVE_TYPE
VIEW_LEAVE_BALANCE, UPDATE_LEAVE_BALANCE
VIEW_LEAVE_REQUEST, CREATE_LEAVE_REQUEST
APPROVE_LEAVE_REQUEST  ← seed sẵn, chưa dùng logic/UI cho tới Phase 3
```

---

## 9. DateOnly — ĐÃ CHỐT

`DateOnly` cho `StartDate`/`EndDate`. Bằng chứng hạ tầng:
- `Infrastructure/Data/DateOnlyTypeHandler.cs` dòng 6-14
- `Infrastructure/DependencyInjection.cs` dòng 212
- EF Core 7+/Npgsql native support. Verify column type `date` khi tạo migration.

---

## 10. UAT Test Users (BD-11)

### 10.1. Keycloak users cần tạo (local only)

| Username | Password | Ghi chú |
|----------|----------|---------|
| `employee1@hrm.local` | `Emp@123456` | Employee test 1 |
| `employee2@hrm.local` | `Emp@123456` | Employee test 2 |
| `employee3@hrm.local` | `Emp@123456` | Employee test 3 |

### 10.2. Mapping chain

```
Keycloak user (realm hrm)
  → Đăng nhập → hệ thống tạo record bảng `user` (identity_id = Keycloak UUID)
  → Admin tạo Employee record, chọn User tương ứng → set employee.user_id
```

### 10.3. Lưu ý

- Không đổi password/config admin user `admin@hrm.local`
- Chỉ áp dụng local/UAT, ghi rõ trong report
- Tạo ít nhất 3 employee test để UAT LeaveBalance + LeaveRequest

---

## 11. Tiểu Phase — Thứ Tự Bắt Buộc

### Phase 2C.1: LeaveType CRUD + Permissions + UAT
**Điều kiện:** Phase 2B pass → UAT 2C.1 pass → sang 2C.2

- Domain: `LeaveType`, `LeaveTypeId`, `ILeaveTypeRepository`, `LeaveTypeErrors`
- Application: GetAllPaged, GetOne, Create, Update
- Infrastructure: Configuration, Repository, Migration `AddLeaveType`
- Web: `LeaveTypeController` + Views
- Seed: `VIEW_LEAVE_TYPE`, `UPDATE_LEAVE_TYPE`
- **UAT:**
  - [ ] `dotnet build` pass
  - [ ] Migration thành công, bảng `leave_type` tồn tại
  - [ ] CRUD LeaveType hoạt động trên `/leave-type` (delete = deactivate `IsActive=false`)
  - [ ] Keycloak thật, `UseMockAuth: false`

### Phase 2C.2: LeaveBalance + UAT Data + UAT
**Điều kiện:** 2C.1 pass → UAT 2C.2 pass → sang 2C.3

- Domain: `LeaveBalance`, `LeaveBalanceId`, `ILeaveBalanceRepository`, `LeaveBalanceErrors`
- Application: GetAllPaged, Allocate, Update
- Infrastructure: Configuration (unique constraint), Repository, Migration
- Web: `LeaveBalanceController` + Views
- Seed: `VIEW_LEAVE_BALANCE`, `UPDATE_LEAVE_BALANCE`
- **Tạo UAT data (BD-7, BD-11):**
  - [ ] Tạo 3 Keycloak users: `employee1@hrm.local`, `employee2@hrm.local`, `employee3@hrm.local`
  - [ ] Mỗi user đăng nhập → record bảng `user` được tạo
  - [ ] Admin tạo 3 Employee records, mapping `UserId` tương ứng
  - [ ] Admin cấp phát LeaveBalance cho mỗi employee test
- **UAT:**
  - [ ] Migration thành công
  - [ ] Cấp phát số dư thành công
  - [ ] Unique constraint hoạt động
  - [ ] Xem danh sách số dư đúng

### Phase 2C.3: LeaveRequest + Validation + UAT
**Điều kiện:** 2C.2 pass

- Domain: `LeaveRequest`, `LeaveRequestId`, `LeaveRequestStatus`, `LeaveDayPart`, `ILeaveRequestRepository`, `LeaveRequestErrors`
- Application: Create (resolve identity, validate V1-V8), Cancel (C-1), GetAllPaged, GetOne
- Infrastructure: Configuration, Repository, Migration
- Web: `LeaveRequestController` + Views (form chọn StartDayPart/EndDayPart)
- Seed: `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`
- UI: badge status, **không nút approve/reject**
- **UAT:**
  - [ ] Migration thành công
  - [ ] Employee test đăng nhập → tạo đơn (resolve identity OK)
  - [ ] `StartDayPart=Morning, EndDayPart=Morning` cùng ngày → TotalDays = 0.5
  - [ ] `StartDayPart=Afternoon, EndDayPart=FullDay` 2 ngày → TotalDays = 1.5
  - [ ] Reject xin phép quá khứ (V-3)
  - [ ] Reject bắc qua năm (V-4)
  - [ ] Reject `Morning+Afternoon` cùng ngày (V-5)
  - [ ] Reject overlap với đơn Pending (V-6)
  - [ ] Reject vượt AvailableDays do PendingDays (V-7)
  - [ ] Tạo 2 đơn Pending → đơn 3 reject nếu vượt balance
  - [ ] Cancel đơn Pending OK
  - [ ] Cancel đơn đã Canceled → reject (C-1)
  - [ ] Xem danh sách + chi tiết đơn

---

## 12. Cấu trúc file

```
Domain/
  LeaveTypes/
    LeaveType.cs, LeaveTypeId.cs, ILeaveTypeRepository.cs, LeaveTypeErrors.cs
  LeaveBalances/
    LeaveBalance.cs, LeaveBalanceId.cs, ILeaveBalanceRepository.cs, LeaveBalanceErrors.cs
  LeaveRequests/
    LeaveRequest.cs, LeaveRequestId.cs, LeaveRequestStatus.cs, LeaveDayPart.cs
    ILeaveRequestRepository.cs, LeaveRequestErrors.cs

Application/
  LeaveTypes/     GetAllPaged/, Create/, Update/, GetOne/
  LeaveBalances/  GetAllPaged/, Allocate/, Update/
  LeaveRequests/  Create/, Cancel/, GetAllPaged/, GetOne/

Infrastructure/
  Configurations/  LeaveTypeConfiguration.cs, LeaveBalanceConfiguration.cs, LeaveRequestConfiguration.cs
  Repositories/    LeaveTypeRepository.cs, LeaveBalanceRepository.cs, LeaveRequestRepository.cs

Web.Backend/
  Controllers/  LeaveTypeController.cs, LeaveBalanceController.cs, LeaveRequestController.cs
  Views/
    LeaveType/     (Index, Create, Edit)
    LeaveBalance/  (Index, Allocate/Edit)
    LeaveRequest/  (Index, Create, Detail)
```

---

## 13. GitNexus Impact Analysis

Bắt buộc chạy impact trước khi code:
- `ApplicationDbContext` — thêm DbSet
- `DependencyInjection` — thêm repository registration
- Không sửa `Employee`, `EmployeeConfiguration`

---

## 14. Verify Checklist tổng hợp

- [ ] 2C.1 pass → 2C.2 pass → 2C.3 pass
- [ ] 7 permissions seed + gán ADMIN
- [ ] 3 employee test có mapping UserId → Keycloak user
- [ ] Keycloak thật, `UseMockAuth: false`
- [ ] GitNexus `detect_changes` trước commit
- [ ] Build/run từ `HRM_Leave_Management`

---

## 15. Rủi ro còn lại

| Rủi ro | Mức | Giảm thiểu |
|--------|-----|-----------|
| EF Core migration `DateOnly` → PostgreSQL `date` | Đã verify | `.HasColumnType("date")` đã cấu hình trong `LeaveRequestConfiguration`. Migration pass. |
| Tạo Keycloak user test cần thao tác thủ công trên admin console | Thấp | Ghi rõ bước trong UAT report, có thể script hóa sau |
| `UsedDays` Phase 2C không tự tăng (chưa có approve). Admin/HR có thể chỉnh `UsedDays` thủ công qua LeaveBalance edit. Công thức `AvailableDays = TotalAllocated - UsedDays - PendingDays` luôn đúng bất kể `UsedDays` được cập nhật bằng cách nào. | Thấp | Không giả định `UsedDays = 0`. Phase 3 tự động tăng `UsedDays` khi approve. |

---

## 16. Phase 3 Notes (Nghiệp vụ đã chốt)

| Quyết định | Chi tiết |
|-----------|----------|
| Approve Pending | Tăng `UsedDays` theo `TotalDays` của đơn. |
| Reject Pending | **Không tăng, không hoàn `UsedDays`** (đơn Pending chưa bao giờ trừ UsedDays). |
| ApproverComment | Optional cho cả Approve và Reject. **Không validate bắt buộc comment khi reject.** |
| Reverse/Cancel Approved | Ngoài scope Phase 3 nếu user chưa chốt. |
| Overlap check Phase 3 | Đã implement Pending+Approved từ Phase 2C.3. Phase 3 cần đảm bảo logic vẫn đúng khi approve/reject. |
