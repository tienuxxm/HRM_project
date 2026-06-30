# Kế hoạch Phase 3: Dynamic Approval Flow — v2

- **Ngày tạo:** 2026-06-30
- **Cập nhật:** 2026-06-30 13:07
- **Người lập:** Antigravity (Senior .NET Fullstack Engineer)
- **Trạng thái:** ⏳ **Phase 3A hoàn thành - Chờ user làm UAT**
- **Thư mục Build/Run:** `HRM_Leave_Management`
- **Tiền đề:** Phase 2C.3 UAT pass. Review model đã chốt (`2026-06-30_1044_phase-3_approval-model-review.md`).
- **Plan cũ superseded:** `2026-06-29_1542_phase-3_approval-flow.md` — KHÔNG còn hợp lệ.

---

## 0. Business Decisions Đã Chốt

| # | Quyết định | Chi tiết |
|---|-----------|----------|
| BD-R1 | Admin/HR = quản trị | KHÔNG mặc định là người duyệt. Permission xác định năng lực, assignment xác định phạm vi. |
| BD-R2 | Dynamic approval | Đổi người duyệt bằng cấu hình DB, không hardcode role/username/email/user id. |
| BD-R3 | ManagerId không dùng cho approval | Chỉ phục vụ hiển thị/org chart. |
| BD-R4 | 3 cấp: NV / TP / CEO | Dùng `Position` master data với `Level`. |
| BD-R5 | CEO chỉ duyệt trưởng phòng | Không gom toàn bộ đơn công ty. |
| BD-R6 | Position master data | Bảng `position` với code/name/level thay thế free-text. |
| BD-R7 | `leave_approver_assignment` | Bảng trung gian: ai (employee cụ thể) duyệt cho nhóm nào (department + position). |
| BD-R8 | Reject không đổi UsedDays | Đơn Pending chưa trừ UsedDays → reject không hoàn. |
| BD-R9 | ApproverComment optional | Không validate bắt buộc comment khi reject. |
| BD-R10 | Overlap check giữ nguyên | Đã check Pending+Approved từ Phase 2C.3. |
| BD-R11 | CEO xin nghỉ = auto Approved record | CEO submit → hệ thống auto chuyển `Approved`. Không cần approver, không phải self-approve. `ProcessedBy = null`, `ProcessedAt = CreatedAt`. Vẫn check balance + tăng UsedDays. |
| BD-R12 | Cột text `Position` cũ: xóa trong Phase 3A | Drop cột sau migration/verify, không giữ deprecated. |
| BD-R13 | `target_department_id = null` = tất cả phòng ban | Chỉ dùng cho scope cấp cao (CEO duyệt TP). TP duyệt NV nên dùng department cụ thể. |

---

## 1. Schema Mới

### 1.1. Bảng `position` (master data)

| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| id | uuid | PK |
| code | varchar(50) | UNIQUE NOT NULL |
| name | varchar(200) | NOT NULL |
| level | int | NOT NULL |
| is_active | bool | DEFAULT true |
| created_date | timestamp | |

**Seed:**

| Code | Name | Level |
|------|------|-------|
| `EMPLOYEE` | Nhân viên | 1 |
| `DEPT_MANAGER` | Trưởng phòng | 2 |
| `CEO` | CEO / Giám đốc | 3 |

### 1.2. Thay đổi bảng `employee`

- **Thêm:** `position_id uuid? FK → position.id` — nullable ban đầu để migrate an toàn.
- **Xóa:** cột `position` (varchar 200) text cũ — drop sau khi migration/verify xong trong Phase 3A.
- **Sau Phase 3A:** leave/approval flow phải reject employee thiếu `PositionId`. Phase sau có thể chuyển `position_id` thành required nếu cần.

**Migration strategy (Phase 3A) — Gate rõ ràng:**
1. Sửa code refs trước: Employee entity (xóa property `Position` string, thêm `PositionId?` + navigation `Position`), `EmployeeConfiguration`, Employee Create/Update commands/handlers, views/list/detail dùng `PositionId`/navigation thay vì string.
2. Tạo bảng `position`, seed 3 records.
3. Thêm cột `position_id` nullable FK vào `employee`.
4. Data migration: map employees hiện có → `position_id` dựa trên text cũ hoặc seed mặc định.
5. Build pass — verify toàn bộ code compile không lỗi.
6. Verify: không còn employee nào thiếu `position_id` ngoài case được phép.
7. Drop cột text `position` cũ.
8. Build lại — verify lần cuối.

### 1.3. Bảng `leave_approver_assignment`

| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| id | uuid | PK |
| approver_employee_id | uuid | FK → employee.id NOT NULL |
| target_department_id | uuid? | FK → department.id (null = tất cả phòng) |
| target_position_id | uuid? | FK → position.id (null = tất cả position) |
| is_active | bool | DEFAULT true |
| effective_from | date? | null = không giới hạn |
| effective_to | date? | null = không giới hạn |
| created_date | timestamp | |

**Ví dụ seed:**
- TP IT duyệt NV IT: `(TP_IT_emp, IT_dept, EMPLOYEE_pos)`
- CEO duyệt TP: `(CEO_emp, null, DEPT_MANAGER_pos)`

**Ý nghĩa `null`:**
- `target_department_id = null` → scope toàn công ty (tất cả phòng ban).
- `target_position_id = null` → scope tất cả position.
- `effective_from/to` cho phép cấu hình thời hạn ủy quyền tạm.

> **⚠️ Cảnh báo cấu hình `target_department_id = null`:**
> - TP duyệt NV nên dùng `department_id` **cụ thể** để tránh TP phòng A duyệt NV phòng B.
> - `null` chỉ nên dùng cho scope cấp cao (CEO duyệt TP toàn công ty) hoặc khi HR cố ý cấu hình cross-department.
> - UI/Admin CRUD assignment cần hiển thị warning khi `target_department_id = null` để tránh cấu hình nhầm gây duyệt quá rộng.

### 1.4. Bảng `leave_request` — KHÔNG SỬA

Schema đã có: `processed_by` (Guid?), `processed_at` (DateTime?), `comment` (string?).
Domain methods `Approve()` và `Reject()` đã có từ Phase 2C.3.

---

## 2. Rule Duyệt Đơn

### 2.1. Flow thông thường (NV, TP)

**Điều kiện duyệt (tất cả phải thỏa):**

1. Approver có permission `APPROVE_LEAVE_REQUEST`.
2. Approver ≠ chủ đơn (self-approve guard).
3. Approver resolve được sang Employee record (qua identity_id → employee).
4. Requester (chủ đơn) resolve được sang Employee record.
5. Tồn tại ≥1 `leave_approver_assignment` active thỏa:
   - `approver_employee_id = approver.Id`
   - `target_department_id IS NULL` hoặc `= requester.DepartmentId`
   - `target_position_id IS NULL` hoặc `= requester.PositionId`
   - `effective_from IS NULL OR effective_from ≤ today`
   - `effective_to IS NULL OR effective_to ≥ today`
   - `is_active = true`
6. Khi **approve**: re-check `AvailableDays = AllocatedDays - UsedDays - PendingDaysExcludingThisRequest`. Nếu `< Duration` → lỗi.
7. Khi **reject**: KHÔNG thay đổi `UsedDays`.

### 2.2. CEO xin nghỉ — Auto Approved Record

Khi employee có Position = CEO (Level 3) tạo đơn nghỉ phép:
1. Hệ thống check balance: `AvailableDays >= Duration`. Nếu không đủ → lỗi `InsufficientBalance` (không cho submit).
2. Nếu đủ balance → tạo `LeaveRequest` với:
   - `Status = Approved` (auto, không qua Pending).
   - `ProcessedBy = null` (không có người duyệt).
   - `ProcessedAt = CreatedAt` (thời điểm submit).
   - `Comment` = optional, có thể ghi "CEO leave record".
3. Tăng `UsedDays` ngay tại thời điểm submit (vì đã auto Approved).
4. Đây KHÔNG phải self-approve flow — CEO không bấm nút Approve. Hệ thống xử lý auto trong `CreateLeaveRequestCommandHandler`.

**Lưu ý implementation:**
- Logic check "requester là CEO?" phải ưu tiên dùng `position.Code == "CEO"`. KHÔNG dùng `Level == 3` làm rule chính (level có thể thêm cấp sau). KHÔNG hardcode user id/email.
- Handler `CreateLeaveRequestCommandHandler` cần thêm branch: nếu `requester.Position.Code == "CEO"` → auto approve + tăng UsedDays.

### 2.3. Bảng áp dụng 3 vai trò

| Người gửi | Người duyệt | Assignment config |
|-----------|-------------|-------------------|
| NV phòng IT (L1) | TP phòng IT (L2) | `(TP_IT_emp, IT_dept, EMPLOYEE_pos)` |
| TP phòng IT (L2) | CEO (L3) | `(CEO_emp, null, DEPT_MANAGER_pos)` |
| CEO (L3) | Auto Approved | Không cần assignment. Hệ thống auto approve khi submit. |

---

## 3. Chia Phase

### Phase 3A: Position Master Data + Employee Migration

**Scope:**

1. **Domain:** Entity `Position`, `PositionId`, `PositionErrors`.
2. **Application:** CRUD handlers — GetAll, Create, Update, Delete (soft-delete).
3. **Application abstraction:** `IPositionRepository`.
4. **Infrastructure:** `PositionConfiguration`, `PositionRepository`.
5. **Migration step 1:** Tạo bảng `position`, thêm `position_id` FK nullable vào `employee`.
6. **Seed:** 3 Position mặc định + permission `VIEW_POSITION`, `UPDATE_POSITION`.
7. **Migration step 2:** Data migration — map employees hiện có sang `position_id`.
8. **Migration step 3:** Verify không còn employee thiếu `position_id`.
9. **Migration step 4:** Drop cột text `position` cũ.
10. **Sửa entity:** `Employee` — xóa property `Position` (string), thêm `PositionId?` + navigation property `Position`.
11. **Sửa EF:** `EmployeeConfiguration` — xóa config cột text, thêm FK config.
12. **Sửa Application:** Employee Create/Update commands/handlers — dùng `PositionId`.
13. **Web:** CRUD UI `/position` (copy pattern LeaveType).
14. **Web:** Sửa Employee form — dropdown Position thay input text.
15. **Web:** Sửa Employee list/detail — hiển thị Position.Name thay string cũ.
16. **Sửa:** `ApplicationDbContext`, `DependencyInjection`.
17. Build + manual UAT guide.

**N+1 risk:** Thấp — Employee đã có pattern `Include(Department, User)`. Thêm `.Include(e => e.Position)` tương tự.

**GitNexus impact cần chạy trước khi code:**
- `Employee` (entity) — upstream
- `EmployeeConfiguration` — upstream
- `CreateEmployeeCommandHandler` — upstream
- `UpdateEmployeeCommandHandler` — upstream
- `ApplicationDbContext` — upstream

---

### Phase 3B: LeaveApproverAssignment + Approve/Reject Flow + CEO Auto Approve

**Scope:**

1. **Domain:** Entity `LeaveApproverAssignment`, `LeaveApproverAssignmentId`, `LeaveApproverAssignmentErrors`.
2. **Domain:** Thêm `LeaveBalance.AddUsedDays(decimal days)`.
3. **Domain:** Thêm errors vào `LeaveRequestErrors`: `CannotApproveSelf`, `CannotRejectSelf`, `InsufficientBalanceOnApprove`, `NoApprovalAssignment`.
4. **Application:** CRUD handlers cho assignment (GetAll, Create, Update, Delete).
5. **Application:** `ApproveLeaveRequestCommand` + `ApproveLeaveRequestCommandHandler`.
6. **Application:** `RejectLeaveRequestCommand` + `RejectLeaveRequestCommandHandler`.
7. **Application:** Sửa `CreateLeaveRequestCommandHandler` — thêm branch CEO auto approve.
8. **Application abstraction:** `ILeaveApproverAssignmentRepository`.
9. **Infrastructure:** `LeaveApproverAssignmentConfiguration`, `LeaveApproverAssignmentRepository`.
10. **Migration:** Tạo bảng `leave_approver_assignment`.
11. **Seed:** Assignment mặc định cho 2 vai trò test (TP→NV, CEO→TP). CEO không cần assignment.
12. **Seed:** Permission `VIEW_LEAVE_APPROVER_ASSIGNMENT`, `UPDATE_LEAVE_APPROVER_ASSIGNMENT`.
13. **Web:** CRUD UI `/leave-approver-assignment` (với warning khi `target_department_id = null`).
14. **Web:** Controller endpoints `POST /leave-request/approve/{id}`, `POST /leave-request/reject/{id}`.
15. **Web:** Sửa LeaveRequest UI — nút Approve/Reject chỉ hiện khi user có assignment hợp lệ.
16. Build + manual UAT guide.

**Approve flow chi tiết (non-CEO):**

```
1. Check permission APPROVE_LEAVE_REQUEST → 403 nếu không có.
2. Load LeaveRequest by ID → 404 nếu không tìm.
3. Load Employee của approver (identity_id → employee).
4. Self-approve guard: leaveRequest.EmployeeId == approverEmployee.Id → Error.
5. Load Employee của requester (Include PositionId, DepartmentId).
6. Query leave_approver_assignment tìm matching active record.
7. Không tìm → Error NoApprovalAssignment.
8. Re-check balance: AvailableDays = AllocatedDays - UsedDays - PendingDaysExcludingThisRequest.
9. AvailableDays < Duration → Error InsufficientBalanceOnApprove.
10. leaveRequest.Approve(identityId, utcNow, comment).
11. balance.AddUsedDays(leaveRequest.Duration).
12. SaveChangesAsync().
```

**CEO auto approve flow (trong CreateLeaveRequestCommandHandler):**

```
1. (Sau tất cả validation hiện có: overlap, date, balance check...)
2. Load requester.Position (Include).
3. Nếu requester.Position.Level == 3 (CEO):
   a. Tạo LeaveRequest với Status = Approved, ProcessedBy = null, ProcessedAt = now.
   b. balance.AddUsedDays(duration).
   c. SaveChangesAsync().
   d. Return success.
4. Nếu không phải CEO:
   a. Tạo LeaveRequest với Status = Pending (flow hiện tại).
   b. SaveChangesAsync().
```

**Reject flow:** Bước 1-7 giống approve. Bước 8: `leaveRequest.Reject(...)`. Không đổi UsedDays.

**N+1 risk:**
- Assignment query: `WHERE approver_employee_id = @id AND is_active = true` — 1 query.
- Approve handler: LeaveRequest + Employee (approver) + Employee (requester) + LeaveBalance = 4 queries max.
- CEO auto approve: thêm 1 Include Position vào query employee trong create handler.

**GitNexus impact cần chạy trước khi code:**
- `CreateLeaveRequestCommandHandler` — upstream (thêm CEO branch)
- `LeaveBalance` (entity) — upstream
- `LeaveRequestErrors` — upstream
- `LeaveRequestController` — upstream
- `ApplicationDbContext` — upstream

---

## 4. UAT Checklist Dự Kiến

### Phase 3A

| # | Case | Expected |
|---|------|----------|
| A-1 | CRUD Position: tạo/sửa/xem/soft-delete | Hoạt động đúng |
| A-2 | Employee form: dropdown Position (không còn input text) | PositionId lưu đúng |
| A-3 | Employee list hiển thị Position name | Không N+1, Include đúng |
| A-4 | Seed 3 positions hiển thị đúng | Code, name, level chính xác |
| A-5 | Cột text `position` cũ đã bị drop | DB schema verify |
| A-6 | Employee hiện có đã được gán PositionId | Data migration verify |

### Phase 3B

| # | Case | Expected |
|---|------|----------|
| B-1 | Approve Pending thành công (có assignment) | Status → Approved, UsedDays tăng |
| B-2 | Reject Pending thành công | Status → Rejected, UsedDays không đổi |
| B-3 | Self-approve bị chặn | Error CannotApproveSelf |
| B-4 | Không có assignment → lỗi | Error NoApprovalAssignment |
| B-5 | Balance không đủ khi approve | Error InsufficientBalanceOnApprove |
| B-6 | Approve đơn không Pending | Error InvalidOperation |
| B-7 | User không có permission | 403 Forbidden |
| B-8 | Admin có permission nhưng không có assignment | Không duyệt được (BD-R1) |
| B-9 | CRUD assignment | Tạo/sửa/xem/soft-delete hoạt động |
| B-10 | effective_from/to hết hạn | Không duyệt được |
| B-11 | Comment optional | Lưu đúng hoặc null |
| B-12 | PendingDays recalc sau approve | AvailableDays tính lại đúng |
| B-13 | CEO tạo đơn → auto Approved | Status = Approved, ProcessedBy = null, UsedDays tăng |
| B-14 | CEO tạo đơn nhưng balance không đủ | Error InsufficientBalance, không tạo được |
| B-15 | Warning khi cấu hình assignment `department_id = null` | UI hiển thị cảnh báo |

---

## 5. Rủi Ro

| Rủi ro | Mức | Giảm thiểu |
|--------|-----|-------------|
| Concurrency: 2 người approve cùng đơn | Thấp | Domain guard `Status != Pending` + DB transaction |
| Employee chưa có PositionId → assignment check fail | TB | Migration gán PositionId cho employees hiện có, verify trước khi drop cột cũ. Leave/approval flow reject employee thiếu PositionId. |
| Drop cột `position` text gây lỗi code chưa sửa hết | TB | Gate: sửa code refs → migrate data → build pass → verify no missing → drop column. GitNexus impact scan `Employee` upstream. |
| Cấu hình `department_id = null` nhầm → TP duyệt quá rộng | TB | UI warning + document rõ ý nghĩa null |
| Balance re-check race condition | Thấp | EF Core transaction, volume thấp |
| CEO auto approve dùng sai rule check | Thấp | Ưu tiên `position.Code == "CEO"`, không dùng `Level == 3` làm rule chính. Không hardcode user id/email. |

---

## 6. Câu Hỏi Cần User Trả Lời

| # | Câu hỏi | Trạng thái |
|---|---------|------------|
| Q-1 | CEO nghỉ phép ai duyệt? | ✅ **ĐÃ CHỐT:** Auto Approved record, không cần approver. |
| Q-2 | Cột text `Position` cũ giữ hay xóa? | ✅ **ĐÃ CHỐT:** Xóa trong Phase 3A sau migration/verify (gate rõ ràng). |
| Q-3 | `AddUsedDays()` method riêng hay dùng `Update()`? | ✅ **ĐÃ CHỐT:** Dùng `LeaveBalance.AddUsedDays(decimal days)` method riêng. |
| Q-4 | Redirect sau approve/reject về đâu? | ✅ **ĐÃ CHỐT:** Redirect về Index (danh sách). |
| Q-5 | Permission mới cho assignment CRUD? | ✅ **ĐÃ CHỐT:** `VIEW_LEAVE_APPROVER_ASSIGNMENT`, `UPDATE_LEAVE_APPROVER_ASSIGNMENT`. |

---

## 7. Ràng Buộc

- Không code cho đến khi user approve plan này. Tất cả Q đã chốt.
- Không thao tác Keycloak.
- Không tự chạy browser UAT.
- Kiến trúc: `Web.Backend → Application → Domain`. `Infrastructure → Application/Domain`.
- Mọi symbol sửa phải chạy GitNexus `impact()` trước, ghi kết quả vào report.
- Plan cũ `2026-06-29_1542_phase-3_approval-flow.md` đã SUPERSEDED — không dùng nữa.
