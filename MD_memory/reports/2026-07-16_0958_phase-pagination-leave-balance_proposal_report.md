# Phase: LeaveBalance Pagination — Audit & Proposal
**Date**: 2026-07-16 09:58
**Status**: PROPOSAL — Chờ duyệt

## 1. Hiện trạng (Evidence)

### 1.1 Query & Handler
- **Query**: `GetLeaveBalancesQuery(Guid? EmployeeId, Guid? LeaveTypeId, int? Year)` → `IQuery<List<LeaveBalanceResponse>>`
- **Handler**: `GetLeaveBalancesQueryHandler` → `IQueryHandler<..., List<LeaveBalanceResponse>>`
- Return: `List<LeaveBalanceResponse>` — **KHÔNG có paging**
- Handler dùng `.ToListAsync()` rồi map → render toàn bộ

### 1.2 Controller
- `LeaveBalanceController.Index(Guid? employeeId, Guid? leaveTypeId, int? year)` → return `View(result.Value)`
- Không nhận `page`, `pageSize` params
- View model: `@model List<LeaveBalanceResponse>`

### 1.3 View (Index.cshtml)
- Footer: `SHOWING @Model.Count LEAVE BALANCES` — không có prev/next
- Toàn bộ data load ra page 1 lần

### 1.4 Repository
- `ILeaveBalanceRepository.GetAllPaged(PagedQuery<LeaveBalance, LeaveBalanceId> request, IQueryable<LeaveBalance>? queryable)` — **ĐÃ CÓ SẴN** trong interface
- Base `Repository<TEntity, TEntityId>.GetAllPaged()` — **ĐÃ CÓ SẴN** trong Infrastructure, dùng `PagedList<T>.ToPagedList()` + `Skip/Take`

### 1.5 Domain Abstractions
- `PagedList<T>`: `CurrentPage`, `TotalPages`, `PageSize`, `TotalCount`, `HasPrevious`, `HasNext`, `Data`
- `PagedQuery<TEntity, TEntityId>`: `Page`, `PageSize`, `SearchTerm`, `SortOrder`, `SortColumn`

### 1.6 Pattern HRM hiện có
- Employee, Department, Position: **CŨNG CHƯA CÓ** server-side pagination — đều dùng `List<T>` và `GetAll`
- Các module LUC cũ (Promotion, Restaurant, Order, Area, News): **ĐÃ CÓ** `PagedList` pagination đầy đủ
- Kết luận: HRM modules mới chưa triển khai pagination, nhưng infrastructure (PagedList, PagedQuery, Repository.GetAllPaged) đã sẵn sàng

## 2. GitNexus Impact Analysis

| Symbol | Risk | Direct callers | Processes affected |
|--------|------|----------------|-------------------|
| `GetLeaveBalancesQueryHandler` | **LOW** | 1 (LeaveBalanceController.cs) | 0 |
| `GetLeaveBalancesQuery` | **LOW** | 1 (constructor ref) | 0 |
| `ILeaveBalanceRepository.GetAllPaged` | **LOW** | 0 (chưa ai gọi!) | 0 |

→ **Không có risk HIGH/CRITICAL**. An toàn để refactor.

## 3. Đề xuất: Server-side Pagination

### 3.1 Approach
- Dùng infrastructure sẵn có: `PagedQuery` + `PagedList` + `Repository.GetAllPaged()`
- Giữ server-side GET params: `/leave-balance?employeeId=...&leaveTypeId=...&year=2026&page=1&pageSize=10`
- Default: `page=1`, `pageSize=10`

### 3.2 Files cần sửa

| # | File | Layer | Thay đổi |
|---|------|-------|----------|
| 1 | `Application/LeaveBalances/Get/GetLeaveBalancesQuery.cs` | Application | Thêm `int Page = 1`, `int PageSize = 10` params |
| 2 | `Application/LeaveBalances/Get/GetLeaveBalancesQueryHandler.cs` | Application | Đổi return type → `PagedList<LeaveBalanceResponse>`, dùng `Skip/Take` trên query trước khi `ToListAsync`, rồi wrap kết quả vào `PagedList` |
| 3 | `Web.Backend/Controllers/LeaveBalanceController.cs` | Web | Thêm `[FromQuery] int page = 1`, `[FromQuery] int pageSize = 10`, truyền vào query, pass ViewBag paging metadata |
| 4 | `Web.Backend/Views/LeaveBalance/Index.cshtml` | Web | Đổi `@model` → `PagedList<LeaveBalanceResponse>`, dùng `Model.Data` thay `Model`, thêm pagination UI (prev/next) |

### 3.3 Giải pháp chi tiết

**Query** — thêm `Page` và `PageSize`:
```csharp
public sealed record GetLeaveBalancesQuery(
    Guid? EmployeeId = null,
    Guid? LeaveTypeId = null,
    int? Year = null,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedList<LeaveBalanceResponse>>;
```

**Handler** — thay `.ToListAsync()` bằng paging:
- Giữ nguyên filter logic (permission, employeeId, leaveTypeId, year)
- Giữ nguyên pending days calculation
- Áp dụng `.Skip((page-1)*pageSize).Take(pageSize)` SAU filter, TRƯỚC ToList
- Count total SAU filter, TRƯỚC Skip/Take
- Wrap vào `new PagedList<LeaveBalanceResponse>(items, totalCount, page, pageSize)`

> **Lưu ý**: Handler hiện tại tính PendingDays bằng cách join với LeaveRequest sau khi load toàn bộ LeaveBalance. Với pagination, cần tính PendingDays cho subset (chỉ records trong page), KHÔNG phải toàn bộ. Logic này không thay đổi business rule — chỉ thay đổi scope data.

**Controller** — thêm params + ViewBag:
```csharp
public async Task<IActionResult> Index(
    [FromQuery] Guid? employeeId,
    [FromQuery] Guid? leaveTypeId,
    [FromQuery] int? year,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken)
```

**View** — pagination bar theo Swiss pattern:
- Desktop: `SHOWING 1-10 OF 45` + `← PREV` + page numbers + `NEXT →`
- Mobile: `SHOWING 1-10 OF 45` + `← PREV` + `NEXT →`
- Filter params phải persist qua pagination links

### 3.4 Pattern tham khảo
Không cần tham khảo Employee/Department/Position vì chúng cũng chưa có pagination.
Tham khảo pattern từ LUC modules: `GetAllAreaPagedCommand` → `GetAllAreaPagedCommandHandler` → `Repository.GetAllPaged()`.

## 4. URL format sau refactor

```
/leave-balance?employeeId=xxx&leaveTypeId=yyy&year=2026&page=1&pageSize=10
/leave-balance?page=2&pageSize=10
/leave-balance?employeeId=xxx&page=3&pageSize=10
```

## 5. Regression risks

| Risk | Mô tả | Mitigation |
|------|--------|------------|
| Filter mất khi chuyển trang | Pagination links không giữ filter params | Razor helper tạo URL kèm tất cả current filter params |
| Create/Update/Delete redirect | Các modal AJAX redirect về `/leave-balance` không kèm page | Acceptable: redirect về page 1 sau CRUD, user dùng filter lại. Hoặc có thể truyền `returnUrl` nếu muốn |
| PendingDays tính sai | Handler hiện load ALL balance rồi batch query pending → chuyển sang page subset có thể miss | Giữ logic tính PendingDays theo employeeIds/leaveTypeIds của page subset, logic không đổi |
| View `@model` breaking | Đổi từ `List<T>` sang `PagedList<T>` → `Model.Count` → `Model.TotalCount`, `foreach Model` → `foreach Model.Data` | Một lần refactor, không ảnh hưởng partial modals |

## 6. Verification plan

1. `git diff --name-status` — xác nhận chỉ 4 files
2. Build: `dotnet build` — 0 CS errors
3. Manual UAT:
   - `/leave-balance` hiển thị page 1, 10 records
   - Click page 2 → hiển thị records tiếp
   - Filter + page: `/leave-balance?year=2026&page=2` → đúng subset
   - Create → redirect về page 1 → record mới hiện
   - Update → redirect → giá trị đổi
   - Delete → redirect → record biến mất
   - Mobile: pagination bar fit viewport
   - Empty state khi filter ra 0 results

## 7. Khuyến nghị

- **Nên làm LeaveBalance trước** vì impact thấp nhất, dùng làm reference pattern
- **Sau khi LeaveBalance pagination pass UAT**, có thể apply cùng pattern cho Employee, Department, Position
- **Không cần migration** — chỉ đổi query logic, không thay đổi DB schema

## 8. Câu hỏi chờ User

1. `pageSize` mặc định 10 hay muốn số khác?
2. Sau CRUD redirect về page 1 có chấp nhận không? Hay muốn giữ page hiện tại?
3. Có muốn làm pagination cho Employee/Department/Position cùng lượt không? Hay LeaveBalance trước?
