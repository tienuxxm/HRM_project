# Phase 3B Entry Checklist — LeaveApproverAssignment + Approve/Reject + CEO Auto Approve

**Ngày tạo:** 2026-07-01  
**Cập nhật:** 2026-07-01 08:46  
**Trạng thái:** ✅ ĐÃ VERIFIED — SẴN SÀNG CODE  
**Phase trước:** Phase 3A — ✅ UAT PASS / USER CONFIRMED (commit `88c664d`)  
**Architecture boundary:** `Web.Backend → Application → Domain` | `Infrastructure → Application/Domain`

---

## 1. Kết Quả Rà Soát Code Hiện Tại

### 1.1 Các file Phase 3B đã tồn tại (leftover)

| Vị trí | Trạng thái | Hành động đề xuất |
|--------|------------|-------------------|
| `Domain/LeaveApproverAssignments/` | ⚠️ Thư mục **rỗng** | Cần tạo mới entity + value objects |
| `Application/LeaveApproverAssignments/{Create,Delete,GetAll,GetOne,Update}/` | ⚠️ Thư mục **rỗng** (cấu trúc có, file trống) | Cần tạo mới command/query + handler |
| `Web.Backend/Views/LeaveApproverAssignment/` | ⚠️ Thư mục **rỗng** | Cần tạo mới views |
| `Infrastructure/Migrations/20260630071055_AddLeaveApproverAssignment.cs` | ✅ Migration **có nội dung** (170 dòng) | **Cần review** — xem bên dưới |
| `Infrastructure/Configurations/` | ❌ Không có `LeaveApproverAssignmentConfiguration` | Cần tạo mới |
| `Infrastructure/Repositories/` | ❌ Không có `LeaveApproverAssignmentRepository` | Cần tạo mới |
| `Infrastructure/DependencyInjection.cs` | ❌ Không có đăng ký `ILeaveApproverAssignmentRepository` | Cần thêm |
| `Infrastructure/ApplicationDbContext.cs` | ❌ Không có `DbSet<LeaveApproverAssignment>` | Cần thêm |
| `Web.Backend/Controllers/` | ❌ Không có `LeaveApproverAssignmentController` | Cần tạo mới |
| `Application/LeaveRequests/Approve/` | ❌ Không tồn tại | Cần tạo mới |
| `Application/LeaveRequests/Reject/` | ❌ Không tồn tại | Cần tạo mới |
| `LeaveRequestController` | ❌ Chưa có endpoint approve/reject | Cần thêm |

### 1.2 Review migration leftover `20260630071055_AddLeaveApproverAssignment`

**Schema tạo bảng `leave_approver_assignment`:**

| Column | Type | Nullable | Phù hợp plan? |
|--------|------|----------|---------------|
| `id` | uuid PK | No | ✅ |
| `approver_employee_id` | uuid FK → employee | No | ✅ |
| `target_department_id` | uuid FK → department | Yes | ✅ |
| `target_position_id` | uuid FK → position | Yes | ✅ |
| `is_active` | boolean | No | ✅ |
| `effective_from` | date | Yes | ✅ |
| `effective_to` | date | Yes | ✅ |
| `created_date` | timestamp | No | ✅ |

**Indexes:** composite `(approver_employee_id, target_department_id, target_position_id)` + individual indexes. ✅ Phù hợp.

**ForeignKey behavior:** Restrict (không cascade delete). ✅ Đúng.

**Vấn đề:** Migration cũng DropForeignKey + re-AddForeignKey cho `department`, `employee`, `position` — rename FK constraint names. Đây là side-effect từ EF Core regenerating constraints. **Không ảnh hưởng logic** nhưng cần verify migration đã apply vào DB hay chưa.

> [!NOTE]
> **✅ ĐÃ VERIFY (2026-07-01):** Migration `20260630071055_AddLeaveApproverAssignment` **ĐÃ APPLY** vào DB.
> - Chạy `dotnet ef migrations list` → migration xuất hiện KHÔNG có dấu "(Pending)".
> - Bảng `leave_approver_assignment` đã tồn tại trong DB PostgreSQL.
> - **Quyết định:** Giữ migration hiện có, tạo entity/config khớp schema đã apply.
> - Không cần tạo lại migration.

### 1.3 Entity Domain cần kiểm tra trước khi sửa

| Entity | Method cần thêm | Trạng thái hiện tại |
|--------|-----------------|---------------------|
| `LeaveBalance` | `AddUsedDays(decimal days)` | ❌ Chưa có — chỉ có `Update(allocated, used)` |
| `LeaveRequest` | `Approve()`, `Reject()`, `Cancel()` | ✅ **Đã có sẵn** (line 92-116) |
| `LeaveRequestErrors` | 4 errors mới | ❌ Chưa có — cần thêm |

### 1.4 LeaveRequest.Create() — CEO auto approve

**Hiện tại:** `LeaveRequest.Create()` luôn trả `Status = Pending` (line 77).

**Cần:** Thêm overload hoặc tham số cho `Status = Approved` khi CEO tạo đơn.

---

## 2. Permission Cần Seed

### 2.1 Permissions đã có (từ Phase trước)

| Permission | GUID | Gán cho role |
|------------|------|-------------|
| `VIEW_LEAVE_REQUEST` | `cf0b0ef2-...-aefb5` | ADMIN, EMPLOYEE_SELF_VIEW |
| `CREATE_LEAVE_REQUEST` | `cf0b0ef2-...-aefb6` | ADMIN, EMPLOYEE_SELF_VIEW |
| `APPROVE_LEAVE_REQUEST` | `cf0b0ef2-...-aefb7` | ADMIN |
| `VIEW_LEAVE_BALANCE` | `cf0b0ef2-...-aefb3` | ADMIN, EMPLOYEE_SELF_VIEW |
| `UPDATE_LEAVE_BALANCE` | `cf0b0ef2-...-aefb4` | ADMIN |
| `VIEW_DEPARTMENT` | (từ Phase 2) | ADMIN |
| `UPDATE_DEPARTMENT` | (từ Phase 2) | ADMIN |
| `VIEW_EMPLOYEE` | (từ Phase 2) | ADMIN |
| `UPDATE_EMPLOYEE` | (từ Phase 2) | ADMIN |
| `VIEW_POSITION` | (từ Phase 3A) | ADMIN |
| `UPDATE_POSITION` | (từ Phase 3A) | ADMIN |

### 2.2 Permissions mới cần seed cho Phase 3B

| Permission | Mục đích | Gán cho role |
|------------|----------|-------------|
| `VIEW_LEAVE_APPROVER_ASSIGNMENT` | Xem danh sách assignment | ADMIN |
| `UPDATE_LEAVE_APPROVER_ASSIGNMENT` | CRUD assignment | ADMIN |

**Lưu ý:** `APPROVE_LEAVE_REQUEST` đã seed trước đó cho ADMIN. Cần verify trong DB.

---

## 3. Assignment Mẫu Cần Seed

Theo plan (scope item #11):

| # | Approver | Target Dept | Target Position | Ý nghĩa |
|---|----------|-------------|-----------------|----------|
| 1 | Trưởng Phòng (TP) | Dept cụ thể | Nhân Viên (NV) | TP duyệt đơn NV trong phòng mình |
| 2 | CEO | `NULL` (tất cả dept) | Trưởng Phòng (TP) | CEO duyệt đơn TP bất kỳ |

> [!IMPORTANT]
> **Cần chốt trước khi seed:**
> - Employee nào là TP test? Employee nào là CEO test? → Cần verify trong DB
> - DepartmentId nào dùng cho assignment #1? → Cần verify trong DB
> - PositionId cho NV và TP? → ✅ Đã seed từ Phase 3A (3 positions: NV, TP, CEO)
> - CEO tạo đơn → auto Approved, không cần assignment (BD-R11) → ✅ Đã chốt

**Đề xuất:** Không seed assignment cứng trong migration. Tạo SQL seed script riêng trong `MD_memory/debug/` để chạy sau khi build pass — tương tự pattern phase trước.

---

## 4. CEO Auto Approve — Business Rules

| Rule | Chi tiết | Nguồn |
|------|----------|-------|
| Nhận diện CEO | `position.Code == "CEO"` (không hardcode user id/email/name) | Plan Risk #6 |
| CEO tạo đơn | Status = Approved, ProcessedBy = null, ProcessedAt = now | Plan line 229-232 |
| CEO balance check | Vẫn check balance bình thường trước khi tạo | Plan line 226, B-14 |
| CEO auto approve + balance | `balance.AddUsedDays(duration)` ngay khi tạo | Plan line 230 |
| CEO đơn overlap | Vẫn check overlap bình thường | Validation V-4 giữ nguyên |

---

## 5. GitNexus Impact Analysis — Phải Chạy Trước Khi Code

Theo plan (line 245-250), cần impact analysis cho:

| # | Symbol | Direction | Lý do |
|---|--------|-----------|-------|
| 1 | `CreateLeaveRequestCommandHandler` | upstream | Thêm CEO auto approve branch |
| 2 | `LeaveBalance` | upstream | Thêm `AddUsedDays()` method |
| 3 | `LeaveRequestErrors` | upstream | Thêm 4 errors mới |
| 4 | `LeaveRequestController` | upstream | Thêm approve/reject endpoints |
| 5 | `ApplicationDbContext` | upstream | Thêm `DbSet<LeaveApproverAssignment>` |

---

## 6. UAT Checklist Phase 3B (Dự kiến)

| # | Case | Expected | Pre-condition |
|---|------|----------|---------------|
| B-1 | Approve Pending thành công (có assignment) | Status → Approved, UsedDays tăng | TP có assignment, NV có đơn Pending |
| B-2 | Reject Pending thành công | Status → Rejected, UsedDays không đổi | TP có assignment |
| B-3 | Self-approve bị chặn | Error CannotApproveSelf | NV tự approve đơn mình |
| B-4 | Không có assignment → lỗi | Error NoApprovalAssignment | User có permission nhưng không có assignment |
| B-5 | Balance không đủ khi approve | Error InsufficientBalanceOnApprove | UsedDays gần hết quota |
| B-6 | Approve đơn không Pending | Error InvalidOperation | Đơn đã Approved/Rejected/Canceled |
| B-7 | User không có permission | 403 Forbidden | User thiếu APPROVE_LEAVE_REQUEST |
| B-8 | Admin có permission nhưng không có assignment | Không duyệt được | BD-R1: permission + assignment đều cần |
| B-9 | CRUD assignment | Tạo/sửa/xem/soft-delete hoạt động | Admin có UPDATE_LEAVE_APPROVER_ASSIGNMENT |
| B-10 | effective_from/to hết hạn | Không duyệt được | Assignment đã quá effective_to |
| B-11 | Comment optional | Lưu đúng hoặc null | Approve/reject có/không comment |
| B-12 | PendingDays recalc sau approve | AvailableDays tính lại đúng | Đơn Pending → Approved, pending count giảm |
| B-13 | CEO tạo đơn → auto Approved | Status = Approved, ProcessedBy = null, UsedDays tăng | Employee có position.Code == "CEO" |
| B-14 | CEO tạo đơn nhưng balance không đủ | Error InsufficientBalance | CEO balance gần hết |
| B-15 | Warning khi cấu hình department_id = null | UI hiển thị cảnh báo | Tạo assignment với dept = null |

---

## 7. File Sẽ Tạo Mới / Sửa

### 7.1 Files tạo mới (~20 files)

| Layer | File | Mô tả |
|-------|------|-------|
| Domain | `LeaveApproverAssignments/LeaveApproverAssignment.cs` | Entity |
| Domain | `LeaveApproverAssignments/LeaveApproverAssignmentId.cs` | Strongly-typed ID |
| Domain | `LeaveApproverAssignments/LeaveApproverAssignmentErrors.cs` | Error definitions |
| Domain | `LeaveApproverAssignments/ILeaveApproverAssignmentRepository.cs` | Repository interface |
| Application | `LeaveApproverAssignments/Create/*` | Command + Handler + Validator |
| Application | `LeaveApproverAssignments/Update/*` | Command + Handler + Validator |
| Application | `LeaveApproverAssignments/Delete/*` | Command + Handler |
| Application | `LeaveApproverAssignments/GetAll/*` | Query + Handler |
| Application | `LeaveApproverAssignments/GetOne/*` | Query + Handler |
| Application | `LeaveRequests/Approve/*` | Command + Handler |
| Application | `LeaveRequests/Reject/*` | Command + Handler |
| Infrastructure | `Configurations/LeaveApproverAssignmentConfiguration.cs` | EF Config |
| Infrastructure | `Repositories/LeaveApproverAssignmentRepository.cs` | Repository impl |
| Web | `Controllers/LeaveApproverAssignmentController.cs` | CRUD controller |
| Web | `Views/LeaveApproverAssignment/Index.cshtml` | List view |
| Web | `Views/LeaveApproverAssignment/_Create...Partial.cshtml` | Create modal |
| Web | `Views/LeaveApproverAssignment/_Update...Partial.cshtml` | Update modal |

### 7.2 Files sửa (~8 files, cần impact analysis trước)

| File | Thay đổi |
|------|----------|
| `Domain/LeaveBalances/LeaveBalance.cs` | Thêm `AddUsedDays(decimal days)` |
| `Domain/LeaveRequests/LeaveRequestErrors.cs` | Thêm 4 errors: CannotApproveSelf, CannotRejectSelf, InsufficientBalanceOnApprove, NoApprovalAssignment |
| `Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs` | Thêm CEO auto approve branch |
| `Web.Backend/Controllers/LeaveRequestController.cs` | Thêm Approve/Reject endpoints |
| `Infrastructure/ApplicationDbContext.cs` | Thêm `DbSet<LeaveApproverAssignment>` |
| `Infrastructure/DependencyInjection.cs` | Đăng ký `ILeaveApproverAssignmentRepository` |
| `Web.Backend/Views/LeaveRequest/Detail.cshtml` | **Tạo mới** hoặc sửa trang chi tiết — đặt nút Approve/Reject tại đây |
| `Web.Backend/Views/Shared/_Layout.cshtml` | Thêm item "Approver Assignments" vào `menuItems` JS array dưới nhóm LEAVE MANAGEMENT |

---

## 8. Quyết Định Đã Chốt (E-1 → E-4)

### E-1: Migration `20260630071055` — ✅ ĐÃ APPLY

- **Verify method:** `dotnet ef migrations list --project Infrastructure --startup-project Web.Backend`
- **Kết quả:** Migration xuất hiện trong danh sách, KHÔNG có "(Pending)" → đã apply vào DB.
- **Quyết định:** **Giữ migration hiện có**. Tạo Domain entity + Infrastructure config khớp schema đã apply.
- Không cần recreate migration.

### E-2: Employee test TP/CEO — ⏳ CẦN TẠO THÊM

- **User yêu cầu:** Tạo thêm employee test để phục vụ UAT Phase 3B.
- **Phân biệt rõ:**
  - **Employee app DB:** Có thể tạo bằng seed SQL script local (trong `MD_memory/debug/`).
  - **Keycloak user:** KHÔNG được tự tạo/sửa/reset. Phải hỏi user trước.
- **Dữ liệu test cần tạo (dự kiến):**

| # | EmployeeCode | FullName | Department | Position | UserId | Ghi chú |
|---|-------------|----------|------------|----------|--------|---------|
| 1 | TP-001 | Trưởng Phòng Test | (dept hiện có) | Trưởng Phòng | nullable hoặc mapped | Dùng để test approve |
| 2 | CEO-001 | CEO Test | NULL hoặc "Ban Giám Đốc" | CEO | nullable hoặc mapped | Dùng để test CEO auto approve |
| 3 | NV-001 | Nhân Viên Test | (dept hiện có) | Nhân Viên | mapped tới user hiện có | Dùng để tạo đơn test |

- **Để login bằng TP/CEO trong UAT:** Phải hỏi user trước khi thao tác Keycloak.
- **Bước đề xuất:** Khi code xong Phase 3B, tạo seed SQL script, user tự chạy hoặc xác nhận để Anti chạy.

### E-3: Vị trí Approve/Reject — ✅ DETAIL PAGE

- **Quyết định:** Nút Approve/Reject đặt trong **trang chi tiết đơn nghỉ phép** (Detail page), KHÔNG đặt trực tiếp ở list Index.
- **Hiện tại:** Chưa có Detail page cho LeaveRequest → cần tạo mới theo pattern hiện có.
- **Redirect sau approve/reject:** Về Index (giữ theo plan Q-4).
- **Files liên quan:**
  - `Web.Backend/Controllers/LeaveRequestController.cs` — thêm `GET Detail/{id}`, `POST Approve/{id}`, `POST Reject/{id}`
  - `Web.Backend/Views/LeaveRequest/Detail.cshtml` — tạo mới, hiển thị thông tin đơn + nút Approve/Reject conditional

### E-4: Menu LeaveApproverAssignment — ✅ DƯỚI NHÓM LEAVE MANAGEMENT

- **Quyết định:** Đặt trong sidebar, dưới nhóm "LEAVE MANAGEMENT" (cùng cấp với Leave Types, Leave Requests, Leave Balances).
- **Phân tích layout hiện tại:**
  - Sidebar dùng JS array `menuItems` (line 111-182 trong `_Layout.cshtml`).
  - Không có cơ chế sub-menu collapsible — chỉ có section header (`isTab: true`) + flat items.
  - Để thêm collapsible sub-menu thực sự cần refactor sidebar → **ngoài scope Phase 3B**.
- **Giải pháp tối thiểu:** Thêm 1 item `{ name: 'Approver Assignments', isTab: false, url: 'leave-approver-assignment', svgPath: ... }` vào array `menuItems` sau item "Leave Balances" (line 164).
- **Không refactor sidebar lớn** — chỉ thêm 1 entry.

---

## 9. Ràng Buộc Thực Thi

- ❌ Không push `origin/main` nếu chưa UAT pass
- ❌ Không thao tác Keycloak
- ❌ Không tự chạy browser UAT
- ❌ Không hardcode user id/email/role name cho logic nghiệp vụ
- ❌ Không tạo/sửa/reset Keycloak user nếu chưa có xác nhận riêng từ user
- ✅ Mọi symbol sửa phải chạy `impact()` trước
- ✅ Build + encoding scan sau mỗi sub-phase
- ✅ Git staged commit rõ scope
- ✅ Employee test data tạo bằng SQL seed script trong `MD_memory/debug/`, không bằng migration
