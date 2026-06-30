# Review Mô Hình Approval Flow — Phase 3

- **Ngày tạo:** 2026-06-30
- **Người phân tích:** Antigravity (Senior .NET Fullstack Engineer)
- **Trạng thái:** 📋 **Review — chờ user chốt phương án trước khi code**
- **Mục tiêu:** Phân tích 3 phương án (A/B/C) cho mô hình duyệt đơn nghỉ phép dynamic, đề xuất phương án tối ưu.

---

## 0. Hiện Trạng Code Liên Quan

### Employee entity (`Domain/Employees/Employee.cs`)

```
Properties hiện có:
- FullName (string, required)
- EmployeeCode (string, required, unique)
- DepartmentId? (FK → Department)
- UserId? (FK → User, mapping Keycloak)
- Position (string?, max 200) ← free-text, không có master data
- JoinDate (DateTime)
- ManagerId? (self-ref FK → Employee)
- IsActive (bool)
- CreatedDate (DateTime)
```

**Nhận xét quan trọng:**
- `Position` hiện là **string free-text** (ví dụ: "Nhân viên", "Trưởng phòng", "CEO"). Không có master data, không có level, không có code.
- `ManagerId` đã tồn tại nhưng là optional. Hiện **không được dùng trong bất kỳ logic nghiệp vụ nào** — chỉ lưu trữ và hiển thị.
- `DepartmentId` đã có FK đến `Department`. Department có `ParentDepartmentId` (tree structure).

### Department entity (`Domain/Departments/Department.cs`)

```
Properties: Name, Code (unique), Description?, ParentDepartmentId?, IsActive, CreatedDate
```

- Hỗ trợ cây phòng ban (self-ref `ParentDepartmentId`).
- Không có `HeadEmployeeId` hoặc trường nào liên kết "trưởng phòng".

### Phase 3 Plan hiện tại (`2026-06-29_1542_phase-3_approval-flow.md`)

- Thiết kế theo mô hình **đơn giản**: ai có quyền `APPROVE_LEAVE_REQUEST` thì duyệt tất cả (trừ đơn của mình).
- **KHÔNG có approval scope** — bất kỳ user nào có permission đều thấy và duyệt mọi đơn Pending.
- **KHÔNG phân biệt** nhân viên/trưởng phòng/CEO.
- Thiếu: dynamic scope, position-based routing, phân cấp duyệt.

---

## 1. Yêu Cầu Nghiệp Vụ Mới (Từ User)

| # | Yêu cầu | Ý nghĩa |
|---|---------|---------|
| R-1 | Admin/HR là quản trị hệ thống, **không mặc định là người duyệt** | Permission chỉ xác định năng lực, approval scope xác định phạm vi |
| R-2 | Người duyệt phải thuộc phạm vi duyệt hợp lệ | Cần bảng cấu hình scope |
| R-3 | Không dùng `ManagerId` làm rule duyệt bắt buộc | `ManagerId` chỉ phục vụ hiển thị/org chart |
| R-4 | Dynamic — có thể thay đổi người duyệt theo cấu hình | Config-driven, không hardcode |
| R-5 | 3 vai trò: Nhân viên, Trưởng phòng, CEO | Cần phân cấp (level) |
| R-6 | CEO chỉ duyệt cấp trưởng phòng, không gom toàn bộ | Phân cấp duyệt theo level |
| R-7 | Không hardcode theo role name, username, email, user id | Tất cả phải config-driven |

---

## 2. Phân Tích Phương Án

### Phương án A: `employee_level` + `leave_approval_scope`

**Mô tả:** Thêm cột `Level` (int) vào entity `Employee`. Tạo bảng `leave_approval_scope` ánh xạ "người có level X duyệt được người có level Y".

**Ưu điểm:**
- Đơn giản nhất, ít migration nhất (chỉ thêm 1 cột + 1 bảng).
- Không cần sửa UI Employee nhiều (chỉ thêm 1 dropdown Level).
- Query nhanh: join `employee.level` → `leave_approval_scope`.

**Nhược điểm:**
- `Level` là **magic number** không có ý nghĩa tự giải thích (Level 1 = gì? Level 2 = gì?). Dễ gây nhầm lẫn khi cấu hình.
- Nếu sau này cần thêm thông tin cho từng level (tên, mô tả, thứ tự hiển thị) thì phải refactor lại thành bảng riêng — gần như là tạo bảng `position` nhưng muộn hơn.
- Không tận dụng được `Position` free-text hiện có (phải maintain song song 2 trường).
- Không phù hợp nếu cần mở rộng: cùng level nhưng khác chức danh (ví dụ: Trưởng phòng Kỹ thuật vs Trưởng phòng Nhân sự cùng level 2 nhưng có thể cần scope khác nhau).

**Rủi ro migration/data:** Thấp — `ALTER TABLE employee ADD COLUMN level INT DEFAULT 1`.

**Rủi ro UI/UAT:** Thấp — thêm dropdown vào form Employee.

**Scope bloat:** Thấp.

---

### Phương án B: `position` (master data) + `leave_approval_scope`

**Mô tả:** Tạo entity `Position` (master data) với `Code`, `Name`, `Level`, `IsActive`. Thêm `PositionId?` FK vào `Employee` (thay thế hoặc song song với `Position` free-text). Tạo bảng `leave_approval_scope`.

**Ưu điểm:**
- **Self-documenting**: "Trưởng phòng" (code `DEPT_MANAGER`, level 2) tự giải thích hơn "Level 2".
- **Tái sử dụng**: Position là master data dùng được cho nhiều module khác (báo cáo nhân sự, bảng lương, org chart, phân cấp ký duyệt khác ngoài leave).
- **Flexible**: Có thể thêm field mới vào `Position` (ví dụ: `IsApprover`, `MaxApprovalLevel`) mà không ảnh hưởng `Employee`.
- **Clean Architecture**: Entity `Position` tách biệt khỏi `Employee` — SRP. `Employee` chỉ reference `PositionId`.
- **Giải quyết vấn đề `Position` free-text**: Hiện tại `Position` là string "Nhân viên" / "Trưởng phòng" nhập tay, không validate được. Chuyển sang FK đảm bảo data consistency.
- **N+1 prevention**: `Include(e => e.Position)` khi query — đã có pattern sẵn trong project (Employee Include Department, User).

**Nhược điểm:**
- Migration phức tạp hơn A: tạo bảng `position`, thêm FK `position_id` vào `employee`, migrate data `Position` string → FK.
- Cần UI CRUD cho `Position` (nhưng rất đơn giản, giống LeaveType).
- Scope Phase 3 mở rộng hơn (thêm Position CRUD + migration data).

**Rủi ro migration/data:**
- Trung bình — cần data migration: đọc `position` (text) hiện có → tạo record `Position` → update FK. Nhưng vì DB hiện chỉ có 2-3 employees test, rủi ro rất thấp.

**Rủi ro UI/UAT:** Thấp-Trung bình — cần thêm 1 trang CRUD Position (copy pattern LeaveType).

**Scope bloat:** Trung bình — nhưng là investment cho tương lai.

---

### Phương án C: Department + permission only (không thêm position/level)

**Mô tả:** Duyệt theo department + permission. Ai có quyền `APPROVE_LEAVE_REQUEST` trong cùng department thì duyệt được đơn của nhân viên department đó.

**Ưu điểm:**
- Scope nhỏ nhất, không cần entity mới.
- Dùng được ngay với code hiện tại.

**Nhược điểm:**
- **KHÔNG đáp ứng R-6**: CEO duyệt trưởng phòng như thế nào? Nếu CEO cùng department với trưởng phòng thì có thể duyệt, nhưng nếu CEO không thuộc department nào hoặc thuộc department riêng?
- **KHÔNG có phân cấp**: Trưởng phòng A có quyền `APPROVE_LEAVE_REQUEST` → duyệt được đơn của nhân viên phòng A. Nhưng đơn của trưởng phòng A thì ai duyệt? Cần cross-department scope.
- **Vẫn phải hardcode logic**: "Nếu cùng department thì duyệt được" — đây vẫn là rule cứng, chỉ là cứng theo department thay vì role name.
- **Không scale**: Thêm vai trò "Phó phòng" thì sao? Thêm department lồng thì sao?
- **KHÔNG dynamic**: Không thể cấu hình "ai duyệt ai" linh hoạt, chỉ phụ thuộc vào department.

**Rủi ro:** Thấp cho implementation, **CAO cho nghiệp vụ** — sẽ phải refactor lại khi thêm phân cấp.

---

## 3. So Sánh Tổng Hợp

| Tiêu chí | A (employee_level) | B (position entity) | C (dept + perm) |
|----------|:---:|:---:|:---:|
| Đáp ứng R-1 đến R-7 | ✅ Đầy đủ | ✅ Đầy đủ | ❌ Thiếu R-5, R-6 |
| Self-documenting | ❌ Magic number | ✅ Code + Name | N/A |
| Tái sử dụng (ngoài leave) | ❌ Không | ✅ Báo cáo, lương, org | ❌ Không |
| Clean Architecture | ⚠️ Cột thêm vào entity | ✅ Entity riêng, SRP | ✅ Không đổi |
| Migration complexity | 🟢 Thấp | 🟡 Trung bình | 🟢 Không |
| UI work | 🟢 1 dropdown | 🟡 CRUD mới + dropdown | 🟢 Không |
| Scope bloat Phase 3 | 🟢 Thấp | 🟡 Trung bình | 🟢 Thấp |
| Future-proof | ❌ Phải refactor thành Position sau | ✅ Nền tảng bền vững | ❌ Phải refactor |
| N+1 risk | 🟢 Không (cột trực tiếp) | 🟢 Thấp (Include pattern sẵn) | 🟢 Không |

---

## 4. Đề Xuất: Phương Án B (Position Entity)

### Lý do chọn B thay vì A:

1. **`Position` là master data tự nhiên hơn `employee_level`:**
   - Employee hiện đã có `Position` (string free-text). Việc chuyển thành FK đến bảng master data là evolution tự nhiên, không phải thêm concept mới.
   - Level 1/2/3 không có ý nghĩa nếu đứng một mình. `Position(Code="DEPT_MANAGER", Name="Trưởng phòng", Level=2)` tự giải thích hoàn toàn.

2. **Investment thấp, return cao:**
   - CRUD Position copy pattern LeaveType (đã có sẵn), chỉ mất ~30 phút.
   - DB hiện chỉ có 2-3 employees → migration data gần như zero risk.
   - Sau này dùng Position cho nhiều module khác (org chart, payroll, report).

3. **Clean Architecture:**
   - `Position` là entity riêng thuộc domain layer, tuân thủ SRP.
   - `Employee` chỉ reference `PositionId`, không mang logic level/approval vào entity.

4. **Tránh refactor đau đớn sau này:**
   - Nếu chọn A, khi cần thêm tên/mô tả cho level → phải tạo bảng `position` y hệt → refactor lại migration, FK, UI. Làm đúng 1 lần tốt hơn.

### Lý do không chọn C:

- Không đáp ứng R-5 (phân cấp 3 vai trò) và R-6 (CEO chỉ duyệt trưởng phòng).
- Sẽ phải refactor lại ngay khi nghiệp vụ mở rộng — technical debt tích lũy.

---

## 5. Thiết Kế Schema Đề Xuất (Phương Án B)

### 5.1. Entity `Position` (master data)

```
position
├── id (uuid PK)
├── code (varchar 50, unique, not null) — "EMPLOYEE", "DEPT_MANAGER", "CEO"
├── name (varchar 200, not null) — "Nhân viên", "Trưởng phòng", "CEO"
├── level (int, not null) — 1, 2, 3 (thấp → cao)
├── is_active (bool, default true)
└── created_date (timestamp)
```

**Seed data khởi tạo:**

| Code | Name | Level |
|------|------|-------|
| `EMPLOYEE` | Nhân viên | 1 |
| `DEPT_MANAGER` | Trưởng phòng | 2 |
| `CEO` | CEO / Giám đốc | 3 |

### 5.2. Thay đổi Entity `Employee`

```diff
Employee:
- Position (string? free-text)  ← XÓA hoặc GIỮ LÀM LEGACY
+ PositionId (uuid? FK → Position)  ← THÊM
+ Position (navigation property → Position)
```

**Quyết định cần user confirm:**
- (a) Xóa hẳn cột `position` (text) cũ? Hay
- (b) Giữ lại tạm thời, thêm `position_id` mới, deprecated dần?

Đề xuất: **(b)** — Giữ cột cũ renamed thành `position_title` (tránh trùng navigation property name), thêm `position_id` FK mới. Phase sau xóa cột cũ.

### 5.3. Entity `LeaveApprovalScope` (cấu hình dynamic)

```
leave_approval_scope
├── id (uuid PK)
├── approver_position_id (uuid FK → Position, not null)
├── target_position_id (uuid FK → Position, not null)
├── department_id (uuid? FK → Department, nullable)
├── is_active (bool, default true)
└── created_date (timestamp)

Unique constraint: (approver_position_id, target_position_id, department_id)
```

**Ý nghĩa:**
- **Row 1:** `(DEPT_MANAGER, EMPLOYEE, DeptA)` → Trưởng phòng A duyệt nhân viên phòng A.
- **Row 2:** `(DEPT_MANAGER, EMPLOYEE, null)` → Trưởng phòng duyệt nhân viên **bất kỳ phòng nào** (nếu muốn cross-dept).
- **Row 3:** `(CEO, DEPT_MANAGER, null)` → CEO duyệt trưởng phòng bất kỳ phòng nào.

**`department_id = null`** nghĩa là scope áp dụng **tất cả phòng ban** (global).

### 5.4. Quan hệ giữa các entity

```
┌──────────┐       ┌──────────┐       ┌─────────────────────┐
│ Position │◄──FK──│ Employee │       │ LeaveApprovalScope  │
│ (master) │       │          │       │                     │
│ code     │       │ posId FK─┼──────►│ approverPosId FK───►│
│ name     │       │ deptId FK│       │ targetPosId FK─────►│
│ level    │       │          │       │ deptId FK──────────►│ Department
└──────────┘       └──────────┘       └─────────────────────┘
```

### 5.5. Không cần thêm bảng/cột

- **Không thêm `employee_level`** — dùng `position.level` thay thế.
- **Không thêm `ManagerId` vào logic** — `ManagerId` giữ nguyên vai trò hiển thị/org chart.
- **Không sửa `LeaveRequest`** — schema đã đủ (`processed_by`, `processed_at`, `comment`).

---

## 6. Rule Nghiệp Vụ Đề Xuất

### 6.1. Khi approve/reject đơn

**Điều kiện duyệt đơn (tất cả phải thỏa mãn):**

```
1. Người duyệt phải có permission APPROVE_LEAVE_REQUEST.
2. Người duyệt KHÔNG phải là chủ đơn (self-approve guard).
3. Phải tồn tại ít nhất 1 record trong leave_approval_scope thỏa:
   - approver_position_id = người duyệt.position_id
   - target_position_id   = chủ đơn.position_id
   - department_id         = chủ đơn.department_id   (nếu scope.department_id NOT NULL)
     HOẶC scope.department_id IS NULL                 (scope toàn công ty)
```

### 6.2. Áp dụng cho 3 vai trò

| Người gửi đơn | Người duyệt | Scope config | Giải thích |
|---------------|-------------|-------------|------------|
| Nhân viên (L1) | Trưởng phòng cùng phòng (L2) | `(DEPT_MANAGER, EMPLOYEE, dept_id)` | Trưởng phòng chỉ duyệt nhân viên phòng mình |
| Trưởng phòng (L2) | CEO (L3) | `(CEO, DEPT_MANAGER, null)` | CEO duyệt trưởng phòng bất kỳ phòng nào |
| CEO (L3) | ❓ | Không cấu hình | CEO không có cấp trên → không ai duyệt → hoặc tự duyệt (exception) hoặc HR manual |

### 6.3. Case đặc biệt — Đơn của CEO

> **⚠️ Cần user chốt:**
> CEO nghỉ phép thì ai duyệt?
> - **(a)** CEO được phép self-approve (exception rule)?
> - **(b)** HR/Admin duyệt đơn CEO (cần scope riêng)?
> - **(c)** CEO không tạo đơn qua hệ thống (process offline)?

### 6.4. Case đặc biệt — Phó phòng / Deputy

> **⚠️ Cần user chốt (nếu có trong tương lai):**
> Nếu thêm Position `DEPUTY_MANAGER` (Level 2?) thì scope duyệt thế nào?
> Hiện tại chỉ có 3 level → ghi nhận để Phase sau.

---

## 7. Kế Hoạch Implementation Dự Kiến (Nếu Chọn B)

**Tách thành 2 sub-phase để giảm rủi ro:**

### Phase 3A: Position Master Data + Employee Migration
1. Tạo entity `Position`, `PositionId`, `IPositionRepository`, `PositionErrors`.
2. Tạo Application CRUD: GetAllPaged, Create, Update.
3. EF Configuration + Migration.
4. Seed 3 Position mặc định.
5. Thêm `PositionId?` FK vào `Employee`.
6. Migration: update employees hiện có gán PositionId phù hợp.
7. CRUD UI `/position`.
8. Sửa UI Employee form: thêm dropdown Position.
9. Seed permission: `VIEW_POSITION`, `UPDATE_POSITION`.
10. Build + UAT.

### Phase 3B: Approval Scope + Approve/Reject Flow
1. Tạo entity `LeaveApprovalScope`.
2. CRUD UI `/leave-approval-scope` (Admin config).
3. Seed scope mặc định cho 3 vai trò.
4. Sửa `ApproveLeaveRequestCommandHandler`: thêm scope check.
5. Sửa `RejectLeaveRequestCommandHandler`: thêm scope check.
6. Sửa UI: nút Approve/Reject chỉ hiện khi scope cho phép.
7. Build + UAT.

---

## 8. Câu Hỏi Cần User Trả Lời Trước Khi Code

| # | Câu hỏi | Lựa chọn | Ghi chú |
|---|---------|---------|---------|
| Q-1 | Chọn phương án nào? | A / B / C / khác | Đề xuất: B |
| Q-2 | Cột `position` (text) cũ trong Employee: xóa hay giữ? | Xóa / Giữ deprecated | Đề xuất: Giữ renamed thành `position_title`, thêm `position_id` FK |
| Q-3 | CEO nghỉ phép ai duyệt? | Self-approve / HR duyệt / Offline | Cần chốt vì ảnh hưởng scope config |
| Q-4 | `department_id = null` trong scope có nghĩa "tất cả phòng ban"? | Có / Không | Đề xuất: Có |
| Q-5 | Có cho Trưởng phòng duyệt cross-department không? | Có / Không | Đề xuất: Không (mặc định chỉ phòng mình), cấu hình thêm nếu cần |
| Q-6 | Chia Phase 3 thành 3A + 3B? | Đồng ý / Gộp 1 phase | Đề xuất: Chia 2 sub-phase |
| Q-7 | `AddUsedDays()` method riêng hay dùng `Update()`? | Method riêng / Dùng Update | Đề xuất: Method riêng |
| Q-8 | Redirect sau approve/reject về Detail hay Index? | Detail / Index | Đề xuất: Detail |

---

## 9. Ràng Buộc

- Không code Phase 3 cho đến khi user chốt phương án và trả lời Q-1 đến Q-8.
- Không thao tác Keycloak.
- Không chạy browser UAT.
- Kiến trúc: `Web.Backend → Application → Domain`. `Infrastructure → Application/Domain`.
