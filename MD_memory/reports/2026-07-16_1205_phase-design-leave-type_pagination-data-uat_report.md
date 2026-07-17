# Report: Leave Type — Pagination + Swiss UI + Sample Data

**Ngày:** 2026-07-16
**Phase:** phase-design-leave-type
**Branch:** `main`
**Trạng thái:** UAT PASS
**Auth mode:** Keycloak thật, `UseMockAuth: false`
**Account UAT:** `admin` / `Admin@123456`
**Boundary giữ:** Web.Backend → Application → Domain; Infrastructure → Application/Domain

---

## 1. Files Đã Sửa/Tạo

| File | Hành động | Mô tả |
|------|-----------|-------|
| `Application/LeaveTypes/GetPaged/GetPagedLeaveTypesQuery.cs` | ✅ TẠO MỚI | Paged query với `Page=1`, `PageSize=5` defaults |
| `Application/LeaveTypes/GetPaged/GetPagedLeaveTypesQueryHandler.cs` | ✅ TẠO MỚI | Handler load active LeaveTypes, clamp page/pageSize, trả `PagedList<LeaveType>` |
| `Web.Backend/Controllers/LeaveTypeController.cs` | ✅ SỬA | Index action nhận `page`, `pageSize` params, dùng `GetPagedLeaveTypesQuery`, truyền ViewBag metadata |
| `Web.Backend/Views/LeaveType/Index.cshtml` | ✅ VIẾT LẠI | Swiss International HR UI: desktop table, mobile cards, server-side pagination, search |

## 2. Files KHÔNG Sửa (Confirmed)

- `_Layout.cshtml` — ❌ không đụng
- `styles.css` — ❌ không đụng
- `tailwind.config.js` — ❌ không đụng
- `_CreateLeaveTypePartial.cshtml` — ❌ không đụng (vẫn Flowbite modal cũ)
- `_UpdateLeaveTypePartial.cshtml` — ❌ không đụng (vẫn Flowbite modal cũ)
- `GetAllLeaveTypesQuery.cs` — ❌ không đụng (dropdown callers giữ nguyên)
- `GetAllLeaveTypesQueryHandler.cs` — ❌ không đụng
- Department/Position/Employee/LeaveBalance views — ❌ không đụng
- DB/Auth/Keycloak — ❌ không đụng

## 3. GitNexus Impact Analysis

### GetAllLeaveTypesQuery
- **Risk:** LOW
- **Direct callers:** 0
- **Affected processes:** 0
- **Quyết định:** Giữ nguyên 100%, tạo query mới `GetPagedLeaveTypesQuery` riêng

### GetAllLeaveTypesQueryHandler
- **Risk:** LOW
- **Direct importers (d=1):** 3
  - `LeaveTypeController.cs` (IMPORTS) — controller đang chuyển sang dùng paged query
  - `LeaveBalanceController.cs` (IMPORTS) — dùng cho dropdown, **KHÔNG bị ảnh hưởng**
  - `LeaveRequestController.cs` (IMPORTS) — dùng cho dropdown, **KHÔNG bị ảnh hưởng**
- **Quyết định:** An toàn. GetAll query/handler giữ nguyên, chỉ controller LeaveType dùng paged query mới

### LeaveTypeController
- **Risk:** LOW
- **Direct callers:** 0
- **Quyết định:** An toàn sửa Index action

## 4. Thiết Kế An Toàn — Backward Compatible

**Vấn đề:** `GetAllLeaveTypesQuery` được dùng bởi 3 controllers (LeaveType, LeaveBalance, LeaveRequest). Nếu đổi return type thành `PagedList<LeaveType>` → **BREAK 2 controllers dropdown**.

**Giải pháp:** Tạo query + handler hoàn toàn MỚI:
- `GetPagedLeaveTypesQuery(Page, PageSize)` → `PagedList<LeaveType>`
- Chỉ `LeaveTypeController.Index` dùng query mới
- `LeaveBalanceController` và `LeaveRequestController` tiếp tục dùng `GetAllLeaveTypesQuery` không thay đổi

## 5. Dữ Liệu Mẫu Đã Tạo (Qua UI)

| # | Code | Name | Default Days | Mô tả |
|---|------|------|-------------|-------|
| 1 | AL1 | Annual Leave | 12 | (đã có sẵn trước) |
| 2 | ANNUAL_LEAVE | Annual Leave | 12 | Standard annual leave entitlement |
| 3 | SICK_LEAVE | Sick Leave | 30 | Medical leave for illness or injury |
| 4 | UNPAID_LEAVE | Unpaid Leave | 0 | Leave without pay |
| 5 | MATERNITY_LEAVE | Maternity Leave | 180 | Paid leave for pregnancy and childbirth |
| 6 | PATERNITY_LEAVE | Paternity Leave | 14 | Paid leave for fathers after childbirth |
| 7 | MARRIAGE_LEAVE | Marriage Leave | 3 | Leave for employee's wedding |
| 8 | BEREAVEMENT_LEAVE | Bereavement Leave | 3 | Leave for employee's family loss |
| 9 | COMPENSATORY_LEAVE | Compensatory Leave | 0 | Time off in lieu of overtime |

**Tổng:** 9 leave types (1 có sẵn + 8 mới tạo qua UI)

## 6. UAT Evidence

### 6.1 Build
```
dotnet build Web.Backend.csproj
→ 0 Errors, 31 Warnings (NuGet warnings cũ)
```

### 6.2 Browser UAT (Keycloak real)

| Test Case | Kết quả | Chi tiết |
|-----------|---------|---------|
| Login Keycloak | ✅ PASS | admin / Admin@123456, real Keycloak |
| Mở `/leave-type` | ✅ PASS | Swiss header "LEAVE TYPES", subtitle, search bar, black Create button |
| Page 1 hiển thị | ✅ PASS | 5 rows, "SHOWING 1-5 OF 9 LEAVE TYPES" |
| PREV disabled page 1 | ✅ PASS | Muted, not clickable |
| NEXT enabled page 1 | ✅ PASS | White bg, black text, border, clickable |
| Click NEXT | ✅ PASS | Navigate to page 2 |
| Page 2 hiển thị | ✅ PASS | 4 rows, "SHOWING 6-9 OF 9 LEAVE TYPES" |
| PREV enabled page 2 | ✅ PASS | Clickable, navigate back |
| NEXT disabled page 2 | ✅ PASS | Muted, not clickable |
| Create leave type | ✅ PASS | Modal opens, create succeeds, toast + redirect |
| Console JS errors | ✅ PASS | Không có JS error |
| Swiss table style | ✅ PASS | `bg-[#F5F5F5]` header, monospace, borders, flat badges |
| Actions | ✅ PASS | EDIT + REMOVE underline text links |

### 6.3 Bug Nhỏ Đã Fix
- **Mobile disabled NEXT button** bị ghi nhầm label `PREV` thay vì `NEXT` → đã fix

## 7. Nhận Xét

### Đã hoàn thành
- ✅ Server-side pagination thật với `pageSize = 5`
- ✅ Swiss International HR UI cho desktop table
- ✅ Mobile responsive cards
- ✅ Client-side search (trong page hiện tại)
- ✅ Pagination buttons rõ chữ mọi trạng thái
- ✅ Modal partials giữ nguyên behavior (chưa refactor shell)
- ✅ Không đụng Layout/CSS/Config
- ✅ Backward compatible — dropdown callers không bị ảnh hưởng

### Chưa làm (cần duyệt riêng)
- ❌ Modal shell refactor (Phase 2)
- ❌ Script consolidation (Phase 3)
- ❌ Xóa AL1 trùng nếu user muốn

## 8. Git Status

- Branch: `main`
- Không stage/commit/push
- Files mới: 2 (GetPaged query + handler)
- Files sửa: 2 (Controller + Index.cshtml)
