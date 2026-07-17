# Phase 1 Report: LeaveBalance Index Content Refactor

- **Date**: 2026-07-16 09:10
- **Branch**: `main`
- **Phase**: `phase-design-leave-balance-refactor` — Phase 1 only
- **Approval**: User approved Phase 1 only. Phase 2 (modal shell), Phase 3 (script) NOT approved.

## Exact File Path

**Sửa**: `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/Index.cshtml`

**KHÔNG sửa** (protected):
- `Views/Shared/_Layout.cshtml`
- `wwwroot/css/styles.css`
- `tailwind.config.js`
- `Views/LeaveBalance/_CreateLeaveBalancePartial.cshtml`
- `Views/LeaveBalance/_UpdateLeaveBalancePartial.cshtml`
- Any C#/Controller/Application/Domain/Infrastructure/DB/Auth/Keycloak

## Git Baseline

- `Index.cshtml`: UNMODIFIED trước khi bắt đầu
- Working tree dirty từ phases trước — không liên quan task này

## IDs/Events/Functions Cần Giữ

### Filter Form
| Element | Attribute | Dùng bởi |
|---------|----------|----------|
| Form | `method="get" action="/leave-balance"` | Server filter |
| Employee select | `id="filterEmployee" name="employeeId"` | Form submit |
| LeaveType select | `id="filterLeaveType" name="leaveTypeId"` | Form submit |
| Year input | `id="filterYear" name="year"` | Form submit |
| Submit button | `type="submit"` | Form submit |
| Clear link | `href="/leave-balance"` | Reset |

### Modal Triggers
| Element | Attribute | Dùng bởi |
|---------|----------|----------|
| Create button | `data-modal-target="createLeaveBalanceModal"` `data-modal-toggle="createLeaveBalanceModal"` | Flowbite |
| Edit trigger (per row) | `data-modal-target="updateLeaveBalanceModal-{id}"` `data-modal-toggle="updateLeaveBalanceModal-{id}"` | Flowbite |
| Delete trigger (per row) | `data-modal-target="confirmDelete-{id}"` `data-modal-toggle="confirmDelete-{id}"` | Flowbite |

### Partial Renders
| Partial | Model | Cần giữ nguyên |
|---------|-------|----------------|
| `_CreateLeaveBalancePartial` | (no model) | Modal + script (KHÔNG sửa file này) |
| `_UpdateLeaveBalancePartial` | `lb.val` (LeaveBalanceResponse) | Modal + script (KHÔNG sửa file này) |
| `_ConfirmDeletePartial` | `ConfirmDeleteViewModel` | Shared modal (KHÔNG sửa file này) |

### ViewBag Bindings
- `ViewBag.CanUpdate` → `bool canUpdate`
- `ViewBag.Employees` → `List<Domain.Employees.Employee>` (chỉ khi canUpdate)
- `ViewBag.LeaveTypes` → `List<Domain.LeaveTypes.LeaveType>` (chỉ khi canUpdate)
- `ViewBag.CurrentFilterEmployeeId` → `Guid?`
- `ViewBag.CurrentFilterLeaveTypeId` → `Guid?`
- `ViewBag.CurrentFilterYear` → `int?`

### Model Fields Used in Table
- `lb.val.Id` (Guid)
- `lb.val.EmployeeName` (string)
- `lb.val.EmployeeCode` (string)
- `lb.val.LeaveTypeName` (string)
- `lb.val.Year` (int)
- `lb.val.AllocatedDays` (decimal, format "G29")
- `lb.val.UsedDays` (decimal, format "G29")
- `lb.val.RemainingDays` (decimal, calculated, format "G29")
- `lb.val.PendingDays` (decimal, format "G29")
- `lb.val.AvailableDays` (decimal, calculated, format "G29")

## Elements Đổi Presentation (CHỈ CSS/HTML, KHÔNG đổi behavior)

| Element | Hiện tại | Đổi thành |
|---------|---------|-----------|
| Page wrapper | `<div class="w-full">` | `<div class="w-full flex flex-col font-geist">` |
| Breadcrumb | `<nav>` breadcrumb | `<div class="mb-6"><h2>` section header |
| Main card | `bg-white rounded-lg p-4 mt-5` | `border border-[#D1D1D1] bg-white` |
| Table header bg | `bg-[#F4F7FC]` | `bg-[#F5F5F5]` |
| Table header text | `text-black text-sm font-medium` | `text-[10px] font-bold text-[#4c4546] uppercase font-mono` |
| Table border | `divide-y-2 divide-gray-200` | `border-r border-[#D1D1D1]` per column |
| Table cell text | `text-gray-950 text-sm` | `text-[11px]` |
| Add button | `bg-indigo-600 rounded` | `bg-black text-white uppercase tracking-widest rounded-none` |
| Filter inputs | `bg-gray-50 border-gray-300 rounded-lg focus:ring-blue-500` | `border-[#D1D1D1] focus:border-black rounded-none font-mono text-[11px]` |
| Edit link style | Blue SVG icon + "Edit" | Text `EDIT` underline uppercase |
| Remove link style | Red SVG icon + "Remove" | Text `REMOVE` red uppercase |
| Mobile view | ❌ không có | Thêm stacked cards `lg:hidden` |

## Behavior Giữ Nguyên

1. Filter: `<form method="get" action="/leave-balance">` full-page reload
2. Permission: `@if (canUpdate)` điều khiển filter employee, add button, actions column
3. Modal triggers: `data-modal-target`/`data-modal-toggle` Flowbite attributes
4. Partial renders: `@Html.Partial()` calls (di chuyển vị trí nhưng giữ nguyên tham số)
5. Delete flow: `_ConfirmDeletePartial` shared với `ConfirmDeleteViewModel`
6. Number format: `ToString("G29")` cho decimal values
7. `RemainingDays` wording: giữ nguyên, không đổi business meaning

## Risk: Tránh Undefined/Null

| Risk | Phòng |
|------|-------|
| Modal ID mismatch sau di chuyển partial | Giữ nguyên ID pattern, Flowbite tìm bằng global DOM ID |
| Filter values mất sau reload | Giữ nguyên `name` attributes, giữ `selected`/`value` logic từ ViewBag |
| Mobile cards thiếu data | Dùng cùng `@foreach` loop và model fields |
| `ViewBag.Employees` null khi `canUpdate=false` | Wrap trong `@if (canUpdate)` — giữ nguyên logic hiện tại |

## Lý Do Không Đụng Shell/Sidebar/Header/Footer/Mobile Bottom-Nav

1. `_Layout.cshtml` chứa shell HTML: sidebar, header, bottom-nav, `<style>` block cho mobile scroll fix
2. Sửa `_Layout.cshtml` có thể regress mobile scroll fix đã UAT pass
3. Leave Balance content nằm trong `@RenderBody()` — hoàn toàn độc lập với shell
4. Plan `2026-07-16_0842` dòng 149-168 quy định rõ: "content-area and raw modal refactor only"
5. User yêu cầu rõ: "Không được đụng shell/sidebar/header/footer/mobile bottom-nav"

## Verification Commands

```powershell
git diff --check -- HRM_Leave_Management/Web.Backend/Views/LeaveBalance/Index.cshtml
git diff --name-status -- HRM_Leave_Management/Web.Backend/Views/LeaveBalance/Index.cshtml
dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore 2>&1 | Select-String -Pattern "(error|warning)" | Select-Object -First 20
git diff --name-only | Select-String -Pattern "(Shared|Employee|Department|Position|styles\.css|tailwind\.config|_Layout)"
```
