# Audit & Proposal: Leave Type UI Refactor — Swiss International HR Style

**Ngày:** 2026-07-16  
**Phase:** phase-design-leave-type-refactor  
**Branch:** `main`  
**Trạng thái:** AUDIT ONLY — chưa code  
**Boundary giữ:** Web.Backend → Application → Domain; Infrastructure → Application/Domain

---

## 1. Hiện Trạng LeaveType

### 1.1 Controller — `LeaveTypeController.cs` (87 dòng)

| Action | Method | Route | Permission | Response |
|--------|--------|-------|-----------|----------|
| `Index` | GET | `/leave-type` | `VIEW_LEAVE_TYPE` | `View(List<LeaveType>)` |
| `Create` | POST | `/leave-type/create` | `UPDATE_LEAVE_TYPE` | `Ok(BooleanResponse)` |
| `Update` | POST | `/leave-type/update` | `UPDATE_LEAVE_TYPE` | `Ok(LeaveType)` |
| `Delete` | POST | `/leave-type/delete` | `UPDATE_LEAVE_TYPE` | `RedirectToAction("Index")` |

**Nhận xét:**
- Controller mỏng, đúng pattern MediatR + permission check.
- Không cần sửa C#. Giống Department/Position pattern.
- Delete redirect bằng `RedirectToAction("Index")` (full page reload), khác Create/Update (AJAX + JS redirect).

### 1.2 Domain Entity — `LeaveType.cs`

| Property | Type | Nullable | Ghi chú |
|----------|------|----------|---------|
| `Id` | `LeaveTypeId` | No | Wraps `Guid` |
| `Name` | `string` | No | |
| `Code` | `string` | No | |
| `DefaultDays` | `int` | No | ⚠️ Nghiệp vụ quan trọng — không có ở Department/Position |
| `Description` | `string?` | Yes | |
| `IsActive` | `bool` | No | |
| `CreatedDate` | `DateTime` | No | |

> **Phản biện quan trọng:** LeaveType KHÔNG giống Department/Position 100%. Nó có field `DefaultDays` (int) — đây là field nghiệp vụ liên quan đến allocation policy. Tuy nhiên, nó KHÔNG có carry-forward, annual quota, hay max accrual phức tạp. Hiện tại chỉ là simple CRUD với thêm 1 numeric field. Template Department/Position vẫn có thể dùng, chỉ thêm 1 column `Default Days`.

### 1.3 Application Layer

- **Query:** `GetAllLeaveTypesQuery` → handler trả `List<LeaveType>` (đã filter `IsActive` only)
- **Create:** `CreateLeaveTypeCommand(Name, Code, DefaultDays, Description?)` → `BooleanResponse`
- **Update:** `UpdateLeaveTypeCommand(Id, Name, Code, DefaultDays, Description?)` → `LeaveType`
- **Không có pagination** — query trả toàn bộ list. Chấp nhận được nếu số lượng LeaveType nhỏ (<20).

### 1.4 View — `Index.cshtml` (122 dòng)

**UI Pattern hiện tại:**
- Flowbite/Tailwind LUC style cũ (breadcrumbs SVG, `bg-indigo-600`, `text-blue-500`, `divide-gray-200`)
- KHÔNG có mobile responsive cards
- KHÔNG có search
- KHÔNG có pagination
- KHÔNG có Swiss International HR styling
- Dùng `@model List<Domain.LeaveTypes.LeaveType>` trực tiếp

**Cấu trúc nội dung:**

| Element | Mô tả | Swiss cần? |
|---------|--------|-----------|
| Breadcrumb | SVG chevron, 3 levels | ❌ Bỏ, đổi sang header text |
| Add button | `bg-indigo-600`, Flowbite rounded | ✅ Đổi sang black bar |
| Table header | `bg-[#F4F7FC]`, blue tints | ✅ Đổi sang `bg-[#F5F5F5]` monospace |
| Columns | NO, Code, Name, Default Days, Description, Status, Actions | ✅ Giữ all 7 columns |
| Status badge | Green/red rounded pill | ✅ Đổi sang flat Swiss badge |
| Actions | SVG edit icon + text, SVG delete icon + text | ✅ Đổi sang underline text links |
| Delete modal | Shared `_ConfirmDeletePartial` inside `<td>` | ⚠️ Cần di chuyển ra ngoài table |

**ID/Event/Function inventory:**

| ID/Target | Dùng ở đâu | Giữ/Đổi |
|-----------|------------|---------|
| `createLeaveTypeModal` | `data-modal-target/toggle` trên button + partial | ✅ Giữ nguyên ID |
| `updateLeaveTypeModal-{Id}` | `data-modal-target/toggle` per row + partial | ✅ Giữ nguyên pattern |
| `confirmDelete-{Id}` | Shared `_ConfirmDeletePartial` | ✅ Giữ nguyên |

### 1.5 Create Modal — `_CreateLeaveTypePartial.cshtml` (60 dòng)

**Pattern:**
- Flowbite modal shell (`fixed z-50 hidden`, `bg-white rounded-lg shadow`)
- Inline `<script>` dùng jQuery
- jQuery `$('#saveLtBtn').on('click', ...)` → FormData AJAX POST → toast → redirect

**JS Element IDs:**

| ID | Input/Error | AJAX URL |
|----|-------------|----------|
| `ltCode` | Input text | `/leave-type/create` |
| `ltName` | Input text | |
| `ltDefaultDays` | Input number (min=0, max=365) | |
| `ltDesc` | Textarea | |
| `ltCodeError` | Error paragraph | |
| `ltNameError` | Error paragraph | |
| `ltDaysError` | Error paragraph | |
| `saveLtBtn` | Submit button | |

**Rủi ro:**
- jQuery `$` load từ `<script src="~/lib/jquery/dist/jquery.min.js">` — duplicate load mỗi lần partial render. Tuy nhiên browser cache nên không gây lỗi, chỉ thừa.
- Toast dùng `$('#toast-container').load(...)` — cần giữ pattern này.

### 1.6 Update Modal — `_UpdateLeaveTypePartial.cshtml` (55 dòng)

**Pattern:**
- Flowbite modal shell per-item (`updateLeaveTypeModal-{Id}`)
- Inline `<script>` tạo global function `updateLeaveType(id)`
- jQuery AJAX POST → toast → redirect

**JS Element IDs per instance:**

| ID | Input/Error |
|----|-------------|
| `upLtCode-{Id}` | Input text |
| `upLtName-{Id}` | Input text |
| `upLtDays-{Id}` | Input number |
| `upLtDesc-{Id}` | Textarea |
| `upLtCodeErr-{Id}` | Error paragraph |
| `upLtNameErr-{Id}` | Error paragraph |

**Rủi ro:**
- Function `updateLeaveType(id)` được khai báo trong mỗi partial instance → multiple declarations. Trình duyệt sẽ override bằng declaration cuối, nhưng vì tất cả identically nhau nên không lỗi logic. Tuy nhiên đây là anti-pattern.
- `onclick="updateLeaveType('@Model.Id.Value')"` — inline handler, phù hợp vì mỗi modal có ID riêng.

---

## 2. So Sánh Với Template Đã Chốt

### 2.1 Department Index (template chính)

| Aspect | Department (đã refactor) | LeaveType (hiện tại) | Delta |
|--------|-------------------------|----------------------|-------|
| Header | `<h2>` bold uppercase + subtitle | Breadcrumb SVG | ✅ Đổi |
| Search bar | Desktop + Mobile with Material Icons | Không có | ✅ Thêm client-side filter |
| Create button | Black bar `+ CREATE DEPARTMENT` | Indigo rounded `Add Leave Type` | ✅ Đổi |
| Table head | `bg-[#F5F5F5]` monospace 10px bold | `bg-[#F4F7FC]` normal 14px | ✅ Đổi |
| Row hover | `hover:bg-[#f4f3f3]` with data attributes | Không có | ✅ Thêm |
| Status badge | Flat Swiss border style | Green/red rounded pill | ✅ Đổi |
| Actions | Underline text links | SVG icon + blue text | ✅ Đổi |
| Mobile cards | Stacked card layout | Không có | ✅ Thêm |
| Pagination | Client-side JS | Không có | ✅ Thêm client-side |
| Table footer | `SHOWING X-Y OF Z` | Không có | ✅ Thêm |
| Modals outside table | Rendered after table | Inside `<td>` | ⚠️ Cần di chuyển |

### 2.2 Department Modal (template chính)

| Aspect | Department Modal | LeaveType Modal | Delta |
|--------|-----------------|----------------|-------|
| Shell | Black header bar + red close button | Flowbite gray rounded | ✅ Đổi hoàn toàn |
| Backdrop | `hrm-modal-backdrop` class | Flowbite default | ✅ Đổi |
| Labels | Monospace 10px bold uppercase | Normal gray text | ✅ Đổi |
| Inputs | White bg, border `#D1D1D1`, mono | Flowbite gray-50 rounded-lg | ✅ Đổi |
| Footer | `bg-[#FBFBFB]` with CANCEL + red CREATE | Blue `bg-blue-700` full width | ✅ Đổi |
| Script position | Separate in `@section Scripts` | Inline `<script>` right after modal | ⚠️ Cần di chuyển |

### 2.3 Khác Biệt Nghiệp Vụ So Với Department

| Field | Department | LeaveType |
|-------|-----------|-----------|
| Code | ✅ text | ✅ text |
| Name | ✅ text | ✅ text |
| Description | ✅ textarea optional | ✅ textarea optional |
| DefaultDays | ❌ KHÔNG CÓ | ✅ number (int, 0-365) |
| IsActive | ✅ boolean | ✅ boolean |

**→ LeaveType cần thêm 1 field DefaultDays trong modal và 1 column trong table.** Phần còn lại giống 100%.

---

## 3. Rủi Ro Chức Năng

### 3.1 Delete modal render bên trong `<td>`

Hiện tại `@Html.Partial("_ConfirmDeletePartial")` render **bên trong `<td>`** mỗi row. Cần di chuyển ra ngoài `<table>` giống Department (L153-164).

### 3.2 Update modal render bên trong `<td>`

`@Html.Partial("_UpdateLeaveTypePartial", lt.val)` cũng render **bên trong `<td>`**. Cần di chuyển.

### 3.3 jQuery duplicate load

`_CreateLeaveTypePartial` load jQuery riêng: `<script src="~/lib/jquery/dist/jquery.min.js">`. Layout đã load jQuery rồi → duplicate. Cần bỏ dòng `<script src>` trong partial.

### 3.4 Inline script multiple declarations

`_UpdateLeaveTypePartial` khai báo `function updateLeaveType(id)` trong mỗi partial instance. Không lỗi hiện tại nhưng là anti-pattern.

### 3.5 GetAllLeaveTypesQuery filter IsActive

Handler hiện tại **chỉ trả LeaveType có `IsActive=true`**. Nếu Admin cần xem cả Inactive thì cần sửa Application layer — nhưng đây là NGOÀI SCOPE phase này. Giữ nguyên behavior.

### 3.6 Không có pagination backend

LeaveType thường có ít bản ghi (<20). Client-side pagination giống Department đủ dùng. Không cần backend pagination.

---

## 4. Đề Xuất Micro-Phases

### Phase 1: LeaveType Index Content Refactor

**Files được sửa:**
- `HRM_Leave_Management/Web.Backend/Views/LeaveType/Index.cshtml`

**Files KHÔNG được sửa:**
- `_Layout.cshtml`, `styles.css`, `tailwind.config.js`
- `_CreateLeaveTypePartial.cshtml`, `_UpdateLeaveTypePartial.cshtml`
- Controller, Application, Domain, DB, Auth, Keycloak
- Department/Position/Employee/LeaveBalance views

**Nội dung refactor:**
1. Bỏ breadcrumb SVG → thay bằng Swiss header (`<h2>` uppercase bold + subtitle)
2. Thêm Desktop Search bar + Mobile Search bar (client-side filter)
3. Đổi Create button sang black bar `+ CREATE LEAVE TYPE`
4. Đổi table header sang Swiss style: `bg-[#F5F5F5]`, monospace 10px bold, `border-r border-[#D1D1D1]`
5. Columns: NO | CODE | NAME | DEFAULT DAYS | DESCRIPTION | STATUS | ACTIONS
6. Đổi Status badge sang flat Swiss: `border border-emerald-500 text-emerald-600` / `border-[#D1D1D1] text-[#7e7576]`
7. Đổi Actions sang underline text links: `[EDIT]` black + `[REMOVE]` red
8. Di chuyển `@Html.Partial("_UpdateLeaveTypePartial")` và `@Html.Partial("_ConfirmDeletePartial")` ra NGOÀI table, render trong loop sau `</div>` container
9. Thêm Mobile card layout (lg:hidden) giống Department
10. Thêm client-side pagination (Table footer `SHOWING X-Y OF Z`, PREV/NEXT)
11. Thêm `@Html.Partial("_CreateLeaveTypePartial")` render ngoài table container

**Behavior phải giữ nguyên:**
- `data-modal-target="createLeaveTypeModal"` + `data-modal-toggle="createLeaveTypeModal"` trên Create button
- `data-modal-target="updateLeaveTypeModal-{Id}"` + `data-modal-toggle="updateLeaveTypeModal-{Id}"` trên Edit link
- `data-modal-target="confirmDelete-{Id}"` + `data-modal-toggle="confirmDelete-{Id}"` trên Remove link
- `@model List<Domain.LeaveTypes.LeaveType>` giữ nguyên
- ViewBag.Title giữ hoặc đổi thành `"Leave Types"`

**Verification:**
```powershell
# Encoding check
git diff --check -- HRM_Leave_Management/Web.Backend/Views/LeaveType/Index.cshtml
```

**Manual UAT Desktop:**
1. Mở `/leave-type`
2. Thấy header "LEAVE TYPES" uppercase bold
3. Thấy search bar + Create button black
4. Thấy table Swiss style với 7 columns
5. Status badge flat
6. Actions là text links [EDIT] + [REMOVE]
7. Gõ search → filter real-time
8. Click [EDIT] → modal mở (modal vẫn Flowbite cũ — đúng, chưa đổi)
9. Click [REMOVE] → confirm delete modal mở

**Manual UAT Mobile (390x844):**
1. Thấy mobile search bar + Create button
2. Thấy stacked cards thay vì table
3. Mỗi card hiển thị Code, Name, DefaultDays, Status, [EDIT] + [REMOVE]
4. Click [EDIT]/[REMOVE] mở đúng modal

---

### Phase 2: LeaveType Modal Shell Refactor

**Files được sửa:**
- `HRM_Leave_Management/Web.Backend/Views/LeaveType/_CreateLeaveTypePartial.cshtml`
- `HRM_Leave_Management/Web.Backend/Views/LeaveType/_UpdateLeaveTypePartial.cshtml`

**Files KHÔNG được sửa:**
- `Index.cshtml` (đã xong Phase 1)
- Tất cả file khác

**Nội dung refactor:**
1. Đổi modal shell sang Swiss: black header bar, red `×` close, `hrm-modal-backdrop`, flat inputs, monospace labels
2. Footer: `bg-[#FBFBFB]` border top, CANCEL button + red primary action button
3. Bỏ `<script src="~/lib/jquery/dist/jquery.min.js">` duplicate trong Create partial
4. Giữ nguyên tất cả JS logic (AJAX URL, FormData fields, toast, redirect)
5. Giữ nguyên tất cả element IDs: `ltCode`, `ltName`, `ltDefaultDays`, `ltDesc`, `saveLtBtn`, `upLtCode-{Id}`, `upLtName-{Id}`, `upLtDays-{Id}`, `upLtDesc-{Id}`
6. Thêm field `DefaultDays` validation error display nếu chưa có trong Update

**Behavior giữ nguyên:**
- Tất cả AJAX POST URLs: `/leave-type/create`, `/leave-type/update`
- FormData field names: `Code`, `Name`, `DefaultDays`, `Description`, `Id` (update only)
- Toast pattern: `$('#toast-container').load(...)`
- Redirect after success: `location.href="/leave-type"`

**Verification:**
```powershell
git diff --check -- HRM_Leave_Management/Web.Backend/Views/LeaveType/_CreateLeaveTypePartial.cshtml
git diff --check -- HRM_Leave_Management/Web.Backend/Views/LeaveType/_UpdateLeaveTypePartial.cshtml
```

**Manual UAT:**
1. Mở `/leave-type`
2. Click `+ CREATE LEAVE TYPE` → modal Swiss style mở
3. Nhập Code, Name, DefaultDays → CREATE → thành công → toast → redirect
4. Click [EDIT] trên row → modal Swiss style mở với data pre-filled
5. Sửa → UPDATE → thành công
6. Mobile: modal hiển thị responsive

---

### Phase 3: Script Consolidation (Nếu cần)

**Chỉ thực hiện nếu User/Codex approve.**

Nội dung:
- Di chuyển inline `<script>` từ Create/Update partials vào `@section Scripts` trong Index.cshtml
- Loại bỏ multiple declarations của `updateLeaveType(id)`
- Đánh giá: nếu LeaveType chỉ có <20 items thì đây là low priority. Không nên bắt buộc.

---

## 5. Phản Biện & Lưu Ý

### 5.1 LeaveType có giống Department/Position không?

**Đúng 90%, khác 10%:**
- Giống: Code/Name/Description/IsActive, CRUD pattern, modal shell, permission naming
- Khác: Có `DefaultDays` (int) — cần thêm 1 column trong table + 1 field trong modal + validation min=0 max=365
- Không có nghiệp vụ phức tạp như carry-forward, accrual cap, pay-in-lieu

### 5.2 Cần sửa C# không?

**KHÔNG** trong scope phase này. Controller, Application, Domain đều đã hoạt động đúng. Chỉ refactor Razor views.

### 5.3 Cần pagination backend không?

**CHƯA CẦN.** LeaveType thường <20 bản ghi. Client-side pagination giống Department đủ dùng. Nếu sau này cần, có thể áp dụng pattern từ LeaveBalance.

### 5.4 Filter IsActive chỉ trả Active items

Handler `GetAllLeaveTypesQueryHandler` line 20: `.Where(x => x.IsActive)`. Admin không thấy Inactive LeaveType. Đây là design hiện tại, giữ nguyên. Nếu user muốn xem cả Inactive cần sửa Application layer — NGOÀI SCOPE.

---

## 6. Git Status

- Branch: `main`
- Working tree: DIRTY (nhiều file modified từ phases trước)
- Phase này: chưa sửa file nào
- Không stage/commit/push
