# Báo Cáo UAT Phase 3B: LeaveApproverAssignment & Approval Flow

**Thời gian lập:** 2026-07-01 13:20  
**Cập nhật lần cuối:** 2026-07-03 10:24  
**Trạng thái UAT:** **ĐANG CHỜ USER XÁC NHẬN CUỐI CÙNG**  
**Phạm vi:** LeaveApproverAssignment CRUD, Approval/Reject flow, CEO auto-approve, User Update fix, UI modal/toast.

---

## 1. Tổng Kết User UAT Đã Thực Hiện

| # | Kịch bản | Trạng thái | Ghi chú |
|---|----------|------------|---------|
| 1 | Admin Update User gán role cho CEO | **✅ PASS** | User xác nhận 2026-07-03. Fix `disabled` → `readonly` trong `Detail.cshtml`. |
| 2 | CEO visibility — thấy đơn TP | **✅ PASS** | User xác nhận 2026-07-03. Sau khi gán role `LEAVE_APPROVER` qua UI. |
| 3 | CEO approve đơn TP | **✅ PASS** | User xác nhận 2026-07-03. |
| 4 | TP approve đơn NV | **✅ PASS** | User xác nhận 2026-07-03. |
| 5 | Delete Approver Assignment | **✅ PASS** | User xác nhận 2026-07-03. Modal confirm căn giữa, xóa thành công. |

---

## 2. Files Đã Sửa (Scope Phase 3B)

### A. Files code thay đổi (đề xuất stage)

| # | File | Thay đổi | Lý do |
|---|------|----------|-------|
| 1 | `Web.Backend/Views/LeaveApproverAssignment/Index.cshtml` | Edit modal → Flowbite Modal instance; Delete modal safe lifecycle | Fix modal lệch + TypeError |
| 2 | `Web.Backend/Controllers/LeaveApproverAssignmentController.cs` | `using Domain.LeaveApproverAssignments`; Map `DuplicateAssignment` error → tiếng Việt | UX duplicate message |
| 3 | `Web.Backend/Views/User/Detail.cshtml` | Bỏ `disabled` (giữ `readonly`); Bỏ username validation block; Fix styling | Unblock gán role qua UI |

### B. Files mới (Phase 3B — đề xuất stage)

| # | File | Mô tả |
|---|------|-------|
| 4 | `Application/LeaveApproverAssignments/` | Create, Update, Delete, GetAll commands + handlers |
| 5 | `Application/LeaveRequests/Approve/` | ApproveLeaveRequestCommand + Handler |
| 6 | `Application/LeaveRequests/Reject/` | RejectLeaveRequestCommand + Handler |
| 7 | `Application/LeaveRequests/GetById/` | GetLeaveRequestByIdQuery + Handler |





| 8 | `Domain/LeaveApproverAssignments/` | Entity, Repository, Errors, value objects |
| 9 | `Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs` | EF Core config |
| 10 | `Infrastructure/Repositories/LeaveApproverAssignmentRepository.cs` | Repository implementation |
| 11 | `Web.Backend/Controllers/LeaveApproverAssignmentController.cs` | Controller (new file + modified) |
| 12 | `Web.Backend/Views/LeaveApproverAssignment/Index.cshtml` | Assignment management view |
| 13 | `Web.Backend/Views/LeaveRequest/Detail.cshtml` | Leave request detail view |

### C. Files modified chung (từ phases trước + 3B)

| # | File | Thay đổi |
|---|------|----------|
| 14 | `Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler.cs` | Approval flow visibility filtering |
| 15 | `Application/LeaveRequests/Get/LeaveRequestResponse.cs` | Thêm `CanApprove` property |
| 16 | `Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs` | CEO auto-approve logic |
| 17 | `Domain/LeaveBalances/LeaveBalance.cs` | `AddUsedDays` method |
| 18 | `Domain/LeaveRequests/LeaveRequest.cs` | `Approve`, `Reject`, `SetApprovedForCeo` methods |
| 19 | `Domain/LeaveRequests/LeaveRequestErrors.cs` | New error constants |
| 20 | `Infrastructure/ApplicationDbContext.cs` | Register `LeaveApproverAssignment` DbSet |
| 21 | `Infrastructure/DependencyInjection.cs` | Register assignment repository |
| 22 | `Web.Backend/Controllers/LeaveRequestController.cs` | Approve/Reject actions, permission checks |
| 23 | `Web.Backend/Views/LeaveRequest/Index.cshtml` | Approve/Reject buttons conditional rendering |
| 24 | `Web.Backend/Views/Shared/_Layout.cshtml` | Sidebar link cho assignment |

### D. Files KHÔNG stage

| File | Lý do |
|------|-------|
| `MD_memory/debug/*` | Scripts local-only, không commit |
| `MD_memory/gap_analysis.md` | Ngoài scope Phase 3B |
| `MD_memory/hrm_refactor_mapping.md` | Ngoài scope Phase 3B |
| `MD_memory/project_architecture_analysis.md` | Ngoài scope Phase 3B |
| `Web.Backend/appsettings.json` (root) | Ngoài scope |
| `.agents/rules/project.md` | Rule update riêng |
| `.agents/skills/luc-hrm-refactor-guard/SKILL.md` | Skill update riêng |

---

## 3. Root Cause Analysis Các Bug Đã Fix

### Bug 1: Edit modal lệch vị trí
- **Root cause:** `openEditModal()` dùng `classList.add("flex")` — thiếu `items-center justify-center` và backdrop. Flowbite Modal class tự thêm centering + backdrop khi gọi `.show()`.
- **Fix:** Chuyển sang Flowbite `new Modal()` + `.show()/.hide()` pattern, cùng delayed backdrop cleanup.
- **File:** `Views/LeaveApproverAssignment/Index.cshtml`

### Bug 2: Delete modal TypeError
- **Root cause:** Code cũ remove `[modal-backdrop]` DOM element TRƯỚC khi gọi `.hide()` → Flowbite internal instance reference destroyed backdrop → TypeError.
- **Fix:** Gọi `.hide()` trước trên cùng instance, sau đó `setTimeout(300)` cleanup backdrop.
- **File:** `Views/LeaveApproverAssignment/Index.cshtml`

### Bug 3: Duplicate assignment message tiếng Anh
- **Root cause:** Controller trả `result.Error.Name` trực tiếp. `Error(Code, Name)` có `Name = "An active assignment..."` (English). Domain layer giữ ngôn ngữ-neutral.
- **Fix:** Map `LeaveApproverAssignmentErrors.DuplicateAssignment` → tiếng Việt tại controller layer.
- **File:** `Controllers/LeaveApproverAssignmentController.cs`

### Bug 4: User Update — "Please Enter Uername"
- **Root cause:**
  1. `<input readonly disabled>` — `disabled` khiến `$('#username').val()` trả empty trong một số browser
  2. JS `formValidate()` check username → fail → block form submit
  3. Controller `Update()` **KHÔNG dùng Username** — chỉ gửi `(Name, Email, PhoneNumber, RoleIds)`
  4. Username validation hoàn toàn dư thừa
- **Fix:** Bỏ `disabled` (giữ `readonly` + styling `bg-gray-200 cursor-not-allowed`); Bỏ username validation block.
- **File:** `Views/User/Detail.cshtml`
- **Không cần seed DB thủ công:** Sau fix, admin dùng UI Update User để gán role bình thường.

---

## 4. Checklist UAT Chi Tiết (15 Cases)

### Case 1: Admin/User Role Assignment ✅ PASS (User confirmed)
- Login admin → `/user` → chọn `ceo.test`
- Gán role `EMPLOYEE_SELF_VIEW` + `LEAVE_APPROVER` → Save
- **Expected:** Không còn lỗi "Please Enter Uername", redirect/list reload thành công

### Case 2: TP tạo đơn ✅ PASS (User confirmed)
- Login `tp.test` → `/leave-request` → tạo leave request Pending
- **Expected:** Tạo thành công, đơn hiển thị dưới tài khoản TP

### Case 3: CEO visibility ✅ PASS (User confirmed)
- Login `ceo.test` → `/leave-request`
- **Expected:** CEO thấy đơn Pending của TP nếu assignment match target position `DEPT_MANAGER`

### Case 4: CEO approve ✅ PASS (User confirmed)
- CEO mở detail đơn TP → Approve
- **Expected:** Trạng thái chuyển Approved, có người xử lý/thời gian xử lý

### Case 5: TP approve NV ✅ PASS (User confirmed)
- Login `nv.test`, tạo đơn Pending → Login `tp.test`
- TP thấy đơn NV theo assignment IT + EMPLOYEE → TP approve
- **Expected:** Approved, `UsedDays` tăng đúng `duration`

### Case 6: Reject flow ⏳ PENDING USER CONFIRM
- Tạo đơn Pending mới → Approver bấm Reject
- **Expected:** Trạng thái Rejected, KHÔNG tăng UsedDays, comment optional
- **Code verified:** `RejectLeaveRequestCommandHandler` không gọi `AddUsedDays`, chỉ gọi `leaveRequest.Reject()`

### Case 7: Self-approve guard ⏳ PENDING USER CONFIRM
- Approver tạo đơn của chính mình → thử approve
- **Expected:** Không được tự approve đơn của chính mình
- **Code verified:** `ApproveLeaveRequestCommandHandler` line 84-88: `if (leaveRequest.EmployeeId == approverEmployee.Id)` → return `CannotApproveSelf`

### Case 8: CEO auto-approve own leave ⏳ PENDING USER CONFIRM
- Login `ceo.test` → CEO tự tạo leave request
- **Expected:** Auto Approved ngay khi tạo (không cần người duyệt)
- **Code verified:** `CreateLeaveRequestCommandHandler` line 66: check `employee.Position.Code == "CEO"` → line 200-205: `SetApprovedForCeo()` + `AddUsedDays()`
- **Lưu ý:** Cần CEO employee có Position.Code = `"CEO"` trong DB

### Case 9: Approver Assignment create ⏳ PENDING USER CONFIRM
- Login admin → `/leave-approver-assignment` → tạo assignment hợp lệ
- **Expected:** Toast success (xanh), list reload

### Case 10: Approver Assignment duplicate ⏳ PENDING USER CONFIRM
- Tạo lại assignment trùng (cùng approver, department, position)
- **Expected:** Toast đỏ tiếng Việt: *"Cấu hình phê duyệt này đã tồn tại (cùng người duyệt, phòng ban và chức vụ)."*
- **Code verified:** `LeaveApproverAssignmentController.Create()` line 76-79: Map `DuplicateAssignment` → Vietnamese

### Case 11: Approver Assignment update (Edit modal) ⏳ PENDING USER CONFIRM
- Bấm Sửa
- **Expected:** Modal **căn giữa viewport**, có backdrop mờ, update thành công
- **Code verified:** `openEditModal()` dùng `new Modal()` + `.show()`

### Case 12: Approver Assignment delete ✅ PASS (User confirmed)
- Bấm Xóa → confirm modal → xóa
- **Expected:** Confirm modal căn giữa, xóa thành công, không TypeError console

### Case 13: Permission guard ⏳ PENDING USER CONFIRM
- User không có quyền CRUD vào `/leave-approver-assignment`
- **Expected:** Redirect `/NoPermission` hoặc không thao tác được

### Case 14: UI feedback — không có window.alert/confirm ✅ VERIFIED BY CODE
- **Kết quả grep:** Không tìm thấy `window.alert()` hoặc `window.confirm()` trong:
  - `Views/LeaveApproverAssignment/*.cshtml`
  - `Views/LeaveRequest/*.cshtml`
  - `Views/User/*.cshtml`
- Tất cả dùng toast/modal theo pattern project

### Case 15: Encoding ⏳ PENDING MANUAL CHECK
- `.md`, `.cs`, `.cshtml` đã sửa không mojibake
- Report `.md` có UTF-8 BOM
- **Cần chạy:** `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py`

---

## 5. Architecture Boundary Verification

**Boundary giữ nguyên:** `Web.Backend → Application → Domain` | `Infrastructure → Application/Domain`

| Layer | Thay đổi | Vi phạm? |
|-------|----------|----------|
| Domain | Entity, Error, Repository interface | ❌ Không phụ thuộc Infrastructure/Web |
| Application | Command/Query handlers, abstractions | ❌ Chỉ phụ thuộc Domain |
| Infrastructure | EF Config, Repository impl, DI | ❌ Phụ thuộc Application/Domain |
| Web.Backend | Controller, View, Model | ❌ Phụ thuộc Application + Domain types |

---

## 6. Git Status & Diff

### `git status --short --branch` (2026-07-03 10:19)

```
## main...origin/main
 M .agents/rules/project.md
 M .agents/skills/luc-hrm-refactor-guard/SKILL.md
 M HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs
 M HRM_Leave_Management/Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler.cs
 M HRM_Leave_Management/Application/LeaveRequests/Get/LeaveRequestResponse.cs
 M HRM_Leave_Management/Domain/LeaveBalances/LeaveBalance.cs
 M HRM_Leave_Management/Domain/LeaveRequests/LeaveRequest.cs
 M HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestErrors.cs
 M HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs
 M HRM_Leave_Management/Infrastructure/DependencyInjection.cs
 M HRM_Leave_Management/Web.Backend/Controllers/LeaveRequestController.cs
 M HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Index.cshtml
 M HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml
 M HRM_Leave_Management/Web.Backend/Views/User/Detail.cshtml
 M HRM_Leave_Management/Web.Backend/appsettings.json
 M MD_memory/gap_analysis.md
 M MD_memory/hrm_refactor_mapping.md
 M MD_memory/project_architecture_analysis.md
 M Web.Backend/appsettings.json
?? HRM_Leave_Management/Application/LeaveApproverAssignments/
?? HRM_Leave_Management/Application/LeaveRequests/Approve/
?? HRM_Leave_Management/Application/LeaveRequests/GetById/
?? HRM_Leave_Management/Application/LeaveRequests/Reject/
?? HRM_Leave_Management/Domain/LeaveApproverAssignments/
?? HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs
?? HRM_Leave_Management/Infrastructure/Repositories/LeaveApproverAssignmentRepository.cs
?? HRM_Leave_Management/Web.Backend/Controllers/LeaveApproverAssignmentController.cs
?? HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/
?? HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Detail.cshtml
?? MD_memory/plans/2026-07-01_0815_phase-3b_entry-checklist.md
?? MD_memory/reports/2026-07-01_1125_phase-3b_manual-uat_report.md
```

### `git diff --stat` (2026-07-03 10:19)

```
 .agents/rules/project.md                                    |   4 +
 .agents/skills/luc-hrm-refactor-guard/SKILL.md              |  34 +++++-
 Application/LeaveRequests/Create/CreateLeaveRequestCmdHdlr   |  11 +-
 Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler   | 135 ++++++++++++++++-----
 Application/LeaveRequests/Get/LeaveRequestResponse.cs        |   3 +-
 Domain/LeaveBalances/LeaveBalance.cs                         |   7 +-
 Domain/LeaveRequests/LeaveRequest.cs                         |  10 +-
 Domain/LeaveRequests/LeaveRequestErrors.cs                   |  22 +++-
 Infrastructure/ApplicationDbContext.cs                       |   4 +-
 Infrastructure/DependencyInjection.cs                        |   5 +-
 Web.Backend/Controllers/LeaveRequestController.cs            |  55 ++++++++-
 Web.Backend/Views/LeaveRequest/Index.cshtml                  |  19 ++-
 Web.Backend/Views/Shared/_Layout.cshtml                      |   8 +-
 Web.Backend/Views/User/Detail.cshtml                         |  11 +-
 Web.Backend/appsettings.json                                 |   7 +-
 MD_memory/gap_analysis.md                                    |   2 +-
 MD_memory/hrm_refactor_mapping.md                            |   2 +-
 MD_memory/project_architecture_analysis.md                   |   2 +-
 Web.Backend/appsettings.json (root)                          |  24 ++--
 19 files changed, 281 insertions(+), 84 deletions(-)
```

---

## 7. Đề Xuất Stage (Khi User Xác Nhận)

### Files đề xuất stage cho commit Phase 3B:

```powershell
# Modified files (Phase 3B scope)
git add -- HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs
git add -- HRM_Leave_Management/Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler.cs
git add -- HRM_Leave_Management/Application/LeaveRequests/Get/LeaveRequestResponse.cs
git add -- HRM_Leave_Management/Domain/LeaveBalances/LeaveBalance.cs
git add -- HRM_Leave_Management/Domain/LeaveRequests/LeaveRequest.cs
git add -- HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestErrors.cs
git add -- HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs
git add -- HRM_Leave_Management/Infrastructure/DependencyInjection.cs
git add -- HRM_Leave_Management/Web.Backend/Controllers/LeaveRequestController.cs
git add -- HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Index.cshtml
git add -- HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml
git add -- HRM_Leave_Management/Web.Backend/Views/User/Detail.cshtml
git add -- HRM_Leave_Management/Web.Backend/appsettings.json

# New files (Phase 3B)
git add -- HRM_Leave_Management/Application/LeaveApproverAssignments/
git add -- HRM_Leave_Management/Application/LeaveRequests/Approve/
git add -- HRM_Leave_Management/Application/LeaveRequests/GetById/
git add -- HRM_Leave_Management/Application/LeaveRequests/Reject/
git add -- HRM_Leave_Management/Domain/LeaveApproverAssignments/
git add -- HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs
git add -- HRM_Leave_Management/Infrastructure/Repositories/LeaveApproverAssignmentRepository.cs
git add -- HRM_Leave_Management/Web.Backend/Controllers/LeaveApproverAssignmentController.cs
git add -- HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/
git add -- HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Detail.cshtml

# Report/plan
git add -- MD_memory/plans/2026-07-01_0815_phase-3b_entry-checklist.md
git add -- MD_memory/reports/2026-07-01_1125_phase-3b_manual-uat_report.md

# Verify
git diff --cached --name-status
```

### Files KHÔNG stage:

| File | Lý do |
|------|-------|
| `MD_memory/debug/*` | Local-only scripts |
| `MD_memory/gap_analysis.md` | Ngoài scope |
| `MD_memory/hrm_refactor_mapping.md` | Ngoài scope |
| `MD_memory/project_architecture_analysis.md` | Ngoài scope |
| `Web.Backend/appsettings.json` (root) | Ngoài scope |
| `.agents/rules/project.md` | Rule update riêng |
| `.agents/skills/luc-hrm-refactor-guard/SKILL.md` | Skill update riêng |

---

## 8. Technical Debt Ghi Nhận

1. **User Create flow:** Keycloak error mapping trả `Server.Error` chung — cần map rõ hơn.
2. **Domain error messages:** Tất cả Domain errors đều tiếng Anh, controller map từng case → cần pattern chung (resource file hoặc error mapping layer).
3. **`appsettings.json` (root project):** Có thay đổi ngoài scope — cần review riêng.
4. **CEO auto-approve:** Phụ thuộc `Position.Code == "CEO"` (hardcode) — nên chuyển sang config/permission-driven trong phase sau.

---

## 9. Verification Còn Cần User Chạy

```powershell
# 1. Tắt app nếu đang chạy
taskkill /IM "Web.Backend.exe" /F 2>$null

# 2. Build
cd d:\Customer_Management_System-Cao_Thanh_Huy_01212407665\HRM_Leave_Management
dotnet build

# 3. Encoding scan
cd d:\Customer_Management_System-Cao_Thanh_Huy_01212407665
python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/plans/*.md MD_memory/reports/*.md --require-bom
```

> [!IMPORTANT]
> **Phase 3B CHƯA được tuyên bố hoàn tất.**
> Cần user xác nhận:
> - Cases 6-11, 13, 15 (pending manual verify)
> - Build pass (sau khi tắt app)
> - Encoding pass
> - Đồng ý danh sách stage files
