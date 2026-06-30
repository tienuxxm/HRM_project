# Kế hoạch Phase 3: Approval Flow (Leave Request) — v1

- **Ngày tạo:** 2026-06-29
- **Cập nhật:** 2026-06-29 15:42 (v1 — draft chờ user review)
- **Người lập kế hoạch:** Antigravity (Senior .NET Fullstack Engineer)
- **Phase:** Phase 3 Approval Flow
- **Trạng thái:** ❌ **SUPERSEDED** — Thay thế bởi `2026-06-30_1112_phase-3_approval-flow-v2.md`
- **Thư mục Build/Run:** `HRM_Leave_Management`
- **Tiền đề:** Phase 2C (v12-final) đã được User Approved ngày 2026-06-29

---

> **⚠️ PLAN NÀY KHÔNG CÒN HỢP LỆ.**
>
> Mô hình "permission alone can approve" trong plan v1 đã bị thay thế bởi mô hình dynamic approval:
> - **Position master data** (EMPLOYEE / DEPT_MANAGER / CEO)
> - **`leave_approver_assignment`** — employee cụ thể duyệt cho nhóm (department + position) cụ thể
> - Admin/HR KHÔNG mặc định là người duyệt
> - `ManagerId` KHÔNG dùng làm approval rule
>
> Xem plan mới: **`MD_memory/plans/2026-06-30_1112_phase-3_approval-flow-v2.md`**

---

## Lý do superseded

1. Plan v1 thiết kế: ai có quyền `APPROVE_LEAVE_REQUEST` thì duyệt tất cả (trừ đơn của mình). KHÔNG có approval scope, KHÔNG phân biệt nhân viên/trưởng phòng/CEO.

2. User đã chốt yêu cầu nghiệp vụ mới (2026-06-30):
   - Admin/HR là quản trị, KHÔNG mặc định là người duyệt.
   - Người duyệt phải có cả permission + assignment hợp lệ.
   - Dynamic — thay đổi người duyệt bằng cấu hình DB.
   - 3 vai trò: Nhân viên, Trưởng phòng, CEO.
   - CEO chỉ duyệt trưởng phòng, không gom toàn bộ đơn công ty.

3. Phân tích chi tiết tại: `MD_memory/plans/2026-06-30_1044_phase-3_approval-model-review.md`

---

## Nội dung gốc plan v1 (đã mất do lỗi script BOM)

Nội dung gốc của plan v1 đã bị mất do lỗi kỹ thuật (script thêm UTF-8 BOM đọc+ghi cùng file gây xóa nội dung, ngày 2026-06-30). Tuy nhiên plan v1 đã superseded và không còn hợp lệ. Mọi thiết kế Phase 3 xem tại plan v2.

---

## Tham chiếu

- Plan v2: `MD_memory/plans/2026-06-30_1112_phase-3_approval-flow-v2.md`
- Review model: `MD_memory/plans/2026-06-30_1044_phase-3_approval-model-review.md`
