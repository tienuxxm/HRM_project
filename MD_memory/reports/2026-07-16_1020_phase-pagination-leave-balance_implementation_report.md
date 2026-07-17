# Phase: LeaveBalance Pagination — Implementation Report
**Date**: 2026-07-16 10:20
**Status**: ✅ IMPLEMENTED + UAT PASS

## 1. Files đã sửa (4 files đúng scope)

| # | File | Thay đổi |
|---|------|----------|
| 1 | `Application/LeaveBalances/Get/GetLeaveBalancesQuery.cs` | Return type → `IQuery<PagedList<LeaveBalanceResponse>>`, thêm `Page=1`, `PageSize=5` |
| 2 | `Application/LeaveBalances/Get/GetLeaveBalancesQueryHandler.cs` | Return → `PagedList<T>`, dùng `CountAsync` + `Skip/Take`, page clamping, PendingDays cho subset |
| 3 | `Web.Backend/Controllers/LeaveBalanceController.cs` | Thêm `[FromQuery] int page = 1`, `[FromQuery] int pageSize = 5`, ViewBag paging metadata |
| 4 | `Web.Backend/Views/LeaveBalance/Index.cshtml` | `@model PagedList<T>`, `Model.Data` thay `Model`, pagination UI PREV/NEXT + filter preservation |

## 2. Files KHÔNG sửa (confirmed)
- _Layout.cshtml ❌
- styles.css ❌
- tailwind.config.js ❌
- Create/Update modal partials ❌
- Employee/Department/Position ❌
- Auth/Keycloak ❌
- DB schema/migration ❌

## 3. UAT Evidence

### Auth
- **Auth mode**: Keycloak thật
- **UseMockAuth**: false
- **Account**: admin / Admin@123456
- **Login**: ✅ Success

### Pagination Tests
| Test Case | Result | Evidence |
|-----------|--------|----------|
| Page 1 hiển thị 5 records | ✅ | SHOWING 1–5 OF 9 LEAVE BALANCES |
| PAGE 1 OF 2 hiển thị | ✅ | Screenshot confirmed |
| NEXT → page 2 | ✅ | SHOWING 6–9 OF 9, PAGE 2 OF 2 |
| PREV ← page 1 | ✅ | Quay lại SHOWING 1–5 |
| PREV disabled ở page 1 | ✅ | Greyed out cursor-not-allowed |
| NEXT disabled ở page cuối | ✅ | Greyed out cursor-not-allowed |
| Row numbering liên tục (1-5, 6-9) | ✅ | No renumbering per page |
| Filter year=2026 → page 1 | ✅ | Reset về page 1, data đúng |
| Filter + NEXT preserve params | ✅ | URL: `?page=2&pageSize=5&year=2026` |
| Total records = 9 (pageSize=100) | ✅ | SHOWING 1–9 OF 9 |
| Pagination ẩn khi totalPages=1 | ✅ | No PREV/NEXT shown |
| JS errors | ✅ | Console sạch |

### Data UAT
- 9 records có sẵn trong DB (đã đủ 2 trang với pageSize=5)
- Không cần tạo thêm data mẫu

## 4. Technical Notes

### Handler: Page Clamping
```csharp
int totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
if (page > totalPages) { page = totalPages; }
```
→ An toàn khi page vượt quá totalPages sau delete/filter.

### Handler: PageSize Guard
```csharp
int pageSize = request.PageSize > 0 ? Math.Min(request.PageSize, 100) : 5;
```
→ Cap max 100, default 5.

### View: Filter Preservation
```csharp
filterParams.Append($"&employeeId={ViewBag.CurrentFilterEmployeeId}");
// ... same for leaveTypeId, year
```
→ PREV/NEXT links giữ tất cả filter params.

## 5. Git Status
- Branch: `main`
- 4 files modified, chưa stage/commit/push
- Không có thay đổi ngoài scope

## 6. Rủi ro còn lại

| Risk | Mức | Ghi chú |
|------|-----|---------|
| Create/Update modal partials dùng `Model` | Thấp | Partials nhận `LeaveBalanceResponse` trực tiếp từ `@Html.Partial`, không phụ thuộc vào PagedList model |
| Delete redirect về page 1 | Chấp nhận | User đã approve |
| Employee/Department/Position chưa có pagination | Tech debt | Ngoài scope phase này |

## 7. Verification Commands
```bash
git diff --name-status  # 4 files M
dotnet build            # 0 CS errors (apphost lock exit 1 là artifact)
```
