# Báo Cáo Phân Tích Nguyên Nhân Gốc (RCA) — Phase 9 Approval Routing Unassign & Re-route Bug

**Ngày thực hiện**: 2026-07-24  
**Phân vùng kỹ thuật**: Approval Routing / Leave Request Assignment Engine  
**Tuân thủ ranh giới kĩ thuật**:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

---

## I. Tóm Tắt Hiện Tượng & Nguyên Nhân Gốc (RCA Summary)

### 1. Hiện tượng UAT ghi nhận
1. Trong chính sách phòng IT (`/approval-routing/policies/detail/{policyId}`), cả 2 Level Slots đều hiển thị `VACANT` (`IsActive = false`).
2. Tuy nhiên trong danh sách đơn (`/leave-request`), các đơn Pending của nhân viên `uat.provision80` vẫn hiển thị `ASSIGNED APPROVER: uat.provision82 (EMP005)` và không được giải phóng.
3. Sau khi thực hiện hủy gán slot (Unassign Slot), các đơn Pending không tự động chuyển đổi trạng thái hoặc re-route.
4. Một số trường hợp Modal Unassign không hiển thị màn hình chọn Strategy 1 / Strategy 2.

---

## II. Bằng Chứng Thực Tế Từ CSDL Read-Only (Empirical Database Evidence)

Đã chạy truy vấn kiểm tra CSDL trực tiếp (`MD_memory/debug/2026-07-24_1734_audit-all-policies.py`):

1. **Trạng thái Policy IT (`f756fb72-8277-4934-8a71-be724b2e83bc`)**:
   - Level 1 (`Department Manager`):
     - Slot Assignment `94a99216-39a0-4db7-9bb0-8d842a291e8b` (`uat.provision81`): `IsActive = False`
     - Slot Assignment `1c897787-d136-4e60-8c5b-834508c09535` (`uaT.provision82`): `IsActive = False`
   - Level 2 (`Department Header`):
     - Slot Assignment `82858bc0-04cb-4abe-9cb0-c0e693d1d922` (`uat.provision81`): `IsActive = False`
     - Slot Assignment `9a6bd57a-6aa3-4a2e-816d-e1e30f113282` (`uat.provision81`): `IsActive = False`
   - **Hiện trạng Policy**: Cả 2 Level Slots đều đã ngắt kích hoạt (**VACANT**).

2. **Trạng thái các đơn Pending của `uat.provision80` trong CSDL**:
   - Đơn `a934199b-c467-4819-a69e-d77424a26d63` & Đơn `576b26ae-36da-4350-ba5d-6f272029ad18`:
     - `assigned_approver_employee_id`: `c19ff7b5-a4e4-43ab-ac18-786818afedfc` (`uaT.provision82`)
     - `assignment_status`: `1` (`Assigned`)
     - `snapshot_level_assignment_id`: `94a99216-39a0-4db7-9bb0-8d842a291e8b` (Slot cũ của `uat.provision81`!)

---

## III. Phân Tích Chi Tiết Nguyên Nhân (Technical Deep-Dive RCA)

### 1. Lỗi lệch Snapshot ID trong Query (`GetLevelAssignmentUnassignImpactQueryHandler.cs`)
- Trong `GetLevelAssignmentUnassignImpactQueryHandler.cs` (dòng 72-78):
  ```csharp
  var pendingAssignments = await _leaveAssignmentRepository.GetPendingAssignmentsByApproverAsync(targetAssignment.AssignedEmployeeId, cancellationToken);

  var slotPendingAssignments = pendingAssignments
      .Where(a => a.LeaveRequest != null &&
                  a.LeaveRequest.Status == LeaveRequestStatus.Pending &&
                  a.SnapshotLevelAssignmentId == targetAssignment.Id) // <-- LỖI LỆCH ID
      .ToList();
  ```
- **Phân tích**: Khi Operator thực hiện Unassign cho `uaT.provision82` (LevelAssignment ID `1c897787-d136-4e60-8c5b-834508c09535`), query lại kiểm tra điều kiện `SnapshotLevelAssignmentId == targetAssignment.Id`. Trong CSDL, giá trị `SnapshotLevelAssignmentId` của các đơn này vẫn là `94a99216-39a0-4db7-9bb0-8d842a291e8b` (ID từ lần gán trước đó với `uat.provision81`).
- **Hậu quả**: `slotPendingAssignments.Count` trả về `0`.

### 2. Lý do Modal không hiện màn hình Strategy 1/2
- Vì `slotPendingAssignments.Count` bị tính sai thành `0`, Handler trả về `TotalPendingRequestsCount = 0`.
- File Razor `_ImpactPreviewModal.cshtml` kiểm tra `hasImpact = (TotalPendingRequestsCount > 0)`. Vì `hasImpact = false`, giao diện Render thẻ thông báo:
  `"SAFE UNASSIGNMENT - NO PENDING LEAVE REQUESTS IMPACTED"`
  thay vì hiển thị Form chọn Strategy 1 / Strategy 2.

### 3. Lý do Đơn Pending không được cập nhật sau khi Unassign
- Khi Operator bấm "Deactivate Slot", lệnh `UnassignApprovalLevelCommandHandler` được kích hoạt và gọi `ApprovalReassignmentService.ExecuteReassignmentAsync`.
- Trong `ApprovalReassignmentService.cs` (dòng 94-95):
  ```csharp
  .Where(a => !request.TargetLevelAssignmentId.HasValue
              || (a.SnapshotLevelAssignmentId != null && a.SnapshotLevelAssignmentId.Value == request.TargetLevelAssignmentId.Value))
  ```
- Do `SnapshotLevelAssignmentId` trong CSDL (`94a9921...`) không trùng với `TargetLevelAssignmentId` (`1c89778...`), danh sách `assignmentsToProcess` bị rỗng (`0`).
- Dẫn đến **không có đơn xin nghỉ nào được re-route hoặc đánh dấu `NeedsAdminAttention`**. Level Slot bị ngắt kích hoạt thành `VACANT`, nhưng 2 đơn Pending trong CSDL vẫn bị "mồ côi" (orphaned) giữ nguyên trạng thái `Assigned` tới `uaT.provision82`.

### 4. Lỗi lưu vết Snapshot ID trong Domain Entity (`LeaveRequestApprovalAssignment.cs`)
- Trong `LeaveRequestApprovalAssignment.cs` (dòng 118):
  `SnapshotLevelAssignmentId = levelAssignmentId ?? SnapshotLevelAssignmentId;`
- Khi thực hiện Manual Reassignment (hoặc khi `levelAssignmentId` nhận giá trị `null`), entity không xóa/cập nhật Snapshot ID cũ mà giữ nguyên ID cũ từ quá khứ, khiến các lần Unassign tiếp theo bị sai lệch logic lọc.

---

## IV. Đề Xuất Giải Pháp Sửa Đổi Tối Thiểu (Proposed Minimal Patch Plan)

1. **Cập nhật `GetLevelAssignmentUnassignImpactQueryHandler.cs`**:
   - Lọc tất cả đơn Pending được gán cho `AssignedEmployeeId` của target slot, không phụ thuộc duy nhất vào `SnapshotLevelAssignmentId`.
2. **Cập nhật `ApprovalReassignmentService.cs`**:
   - Đảm bảo quét và xử lý toàn bộ đơn Pending đang thuộc quyền duyệt của `targetEmployeeId` khi Unassign Level Slot.
3. **Cập nhật Domain Entity `LeaveRequestApprovalAssignment.cs`**:
   - Cập nhật `SnapshotLevelAssignmentId = levelAssignmentId` khi thực hiện `Reassign(...)` để tránh lưu giữ Snapshot ID lỗi thời từ quá khứ.

---

## V. Trạng Thái Mã Nguồn & Kiểm Tra An Toàn

- **Trạng thái CSDL**: Chưa thực hiện mutation/seed/insert/update/delete. CSDL nguyên bản read-only.
- **Trạng thái Auth/Keycloak**: Giữ nguyên `UseMockAuth = false`.
