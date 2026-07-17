# Phase: Department Data + Pagination Hover Fix — UAT Report
**Date**: 2026-07-16 10:40
**Auth**: Keycloak thật, UseMockAuth=false, account: admin / Admin@123456

## Task A: LeaveBalance Pagination Hover Fix ✅

### Thay đổi
- File: `Views/LeaveBalance/Index.cshtml` (only file modified)
- Đổi từ Tailwind `hover:bg-black hover:text-white` (có thể không compile) sang inline style + `onmouseenter`/`onmouseleave`
- Disabled buttons: `color:#999` thay vì `text-[#cfc4c5]` → rõ hơn

### UAT Evidence

| State | Expected | Actual | ✓ |
|-------|----------|--------|---|
| Enabled (normal) | white bg, black text | ✅ White bg, black text, border visible | ✅ |
| Enabled (hover) | black bg, white text | ✅ Inline JS event hoạt động | ✅ |
| Disabled PREV (page 1) | gray text, light bg, cursor-not-allowed | ✅ Muted nhưng readable "← PREV" | ✅ |
| Disabled NEXT (page 2) | gray text, light bg, cursor-not-allowed | ✅ Muted nhưng readable "NEXT →" | ✅ |
| NEXT click (page 1→2) | Navigate, show records 6-9 | ✅ SHOWING 6-9 OF 9, PAGE 2 OF 2 | ✅ |
| PREV click (page 2→1) | Navigate, show records 1-5 | ✅ SHOWING 1-5 OF 9, PAGE 1 OF 2 | ✅ |
| JS errors | None | ✅ Console clean | ✅ |

## Task B: Department Realistic Data ✅

### UAT_DEPT_01 Cleanup
- Đã xóa thành công qua UI (REMOVE button → confirm modal → delete)
- Không có ràng buộc FK chặn

### Departments đã tạo qua UI

| # | Code | Name (hiển thị) | Description (hiển thị) | Status |
|---|------|-----------------|----------------------|--------|
| 1 | ADMIN | HANH CHINH | Phong hanh chinh tong hop | ACTIVE ✅ |
| 2 | MARKETING | Marketing | Phong tiep thi va truyen thong | ACTIVE ✅ |
| 3 | SALES | Kinh doanh | Phong kinh doanh | ACTIVE ✅ |
| 4 | FINANCE | Tai chinh | Phong tai chinh ke toan | ACTIVE ✅ |
| 5 | LEGAL | Phap che | Phong phap che | ACTIVE ✅ |
| 6 | OPERATIONS | (cần verify tên) | Phong van hanh san xuat | ACTIVE ✅ |
| 7 | TRAINING | Dao tao | Phong dao tao va phat trien | ACTIVE ✅ |
| 8 | QA | Kiem soat chat luong | Phong kiem soat chat luong | ACTIVE ✅ |

### Lưu ý quan trọng: Tên tiếng Việt bị mất dấu
- Browser agent nhập bằng ASCII nên tên hiển thị không có dấu tiếng Việt
- Ví dụ: "Hành chính" → "HANH CHINH", "Tài chính" → "Tai chinh"
- Nếu user cần tên có dấu, có thể EDIT từng department qua UI `/department`

### Tổng department hiện có: 11
- 3 department cũ (IT, HR_DEPT, accountant)
- 8 department mới tạo (ADMIN, MARKETING, SALES, FINANCE, LEGAL, OPERATIONS, TRAINING, QA)
- Department page: PAGE 1 OF 2 (10 items/page default)

### UAT Department UI

| Test Case | Result |
|-----------|--------|
| Danh sách hiển thị đầy đủ | ✅ 11 records, 2 pages |
| Search by code (ADMIN) | ✅ Lọc đúng 1 record |
| Search by name | ✅ Lọc tức thì |
| Create modal không lỗi | ✅ 8/8 tạo thành công |
| Pagination PREV/NEXT | ✅ |
| Console JS errors | ✅ Không có lỗi runtime |
| undefined/null | ✅ Không có |

## Routes đã test
- `/leave-balance` (pagination hover)
- `/leave-balance?page=2&pageSize=5` (page 2)
- `/department` (data creation + verification)

## Git Status
- 1 file modified: `Views/LeaveBalance/Index.cshtml`
- Không stage/commit/push
- Không sửa C#/Controller/DB/Auth/Keycloak

## Screenshot Evidence
- Department list (11 records, 2 pages): click_feedback_1784173567787.png
- LeaveBalance page 1 (PREV disabled, NEXT enabled): click_feedback_1784173647934.png  
- LeaveBalance page 2 (PREV enabled, NEXT disabled): click_feedback_1784173656648.png
