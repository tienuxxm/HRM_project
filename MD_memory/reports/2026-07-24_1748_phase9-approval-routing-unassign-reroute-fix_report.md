# Báo Cáo Patch Sửa Lỗi Kỹ Thuật (Fix Report) — Phase 9 Approval Routing Unassign & Re-route

**Ngày thực hiện**: 2026-07-24  
**Phân vùng kỹ thuật**: Approval Routing / Dynamic Approval Engine  
**Tuân thủ ranh giới kĩ thuật**:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

---

## I. Tóm Tắt Patch Kỹ Thuật (Patch Summary)

1. **`Domain/ApprovalRouting/LeaveRequestApprovalAssignment.cs`**:
   - Cập nhật phương thức `Reassign(...)`:
     `SnapshotLevelAssignmentId = levelAssignmentId;`
   - **Nghiệp vụ**:
     - Khi Auto Reroute (Strategy 1): Ghi nhận `LevelAssignmentId` mới của candidate slot vừa được gán.
     - Khi Manual Reassign (Strategy 2): Thiết lập `SnapshotLevelAssignmentId = null` (xóa liên kết với level slot cũ vì đơn đã được chuyển trực tiếp cho người duyệt chỉ định).

2. **`Application/ApprovalRouting/Queries/GetLevelAssignmentUnassignImpactQueryHandler.cs`**:
   - Cập nhật điều kiện lọc các đơn Pending bị ảnh hưởng bởi slot đang Unassign:
     ```csharp
     var slotPendingAssignments = pendingAssignments
         .Where(a => a.LeaveRequest != null &&
                     a.LeaveRequest.Status == LeaveRequestStatus.Pending &&
                     (a.SnapshotLevelAssignmentId == targetAssignment.Id ||
                      a.SnapshotPolicyId == targetLevel.PolicyId))
         .ToList();
     ```
   - **Nghiệp vụ**: Đảm bảo đếm chính xác toàn bộ đơn Pending đang thuộc phạm vi chính sách (`SnapshotPolicyId`) của Level Slot đang thao tác, không bị lọt đơn do lệch Snapshot ID hoặc sau khi chuyển đổi thủ công.

3. **`Application/ApprovalRouting/Services/ApprovalReassignmentService.cs`**:
   - Cập nhật điều kiện quét đơn cần xử lý trong `ExecuteReassignmentAsync`:
     ```csharp
     var assignmentsToProcess = pendingAssignments
         .Where(a => a.LeaveRequest != null && a.LeaveRequest.Status == LeaveRequestStatus.Pending)
         .Where(a => a.AssignedApproverEmployeeId == targetEmployeeId)
         .ToList();
     ```
   - **Nghiệp vụ**: Khi thực hiện ngắt slot/reassign người duyệt `targetEmployeeId`, **toàn bộ đơn Pending hiện thuộc quyền duyệt của nhân sự đó đều được quét và xử lý**. Nếu auto-reroute không tìm thấy candidate hợp lệ, đơn được chuyển sang `NeedsAdminAttention` rõ ràng, hoàn toàn chấm dứt tình trạng đơn mồ côi giữ approver cũ.

---

## II. Danh Sách Kiểm Tra UAT Thủ Công (Manual UAT Checklist cho User)

User vui lòng tự thực hiện UAT theo danh sách kịch bản kiểm thử bên dưới (không chạy browser tự động):

### Kịch Bản 1: Unassign Level Slot Có Đơn Pending (Strategy 1 Auto Re-route)
1. Đăng nhập tài khoản `admin` (hoặc tài khoản có quyền `UPDATE_APPROVAL_ROUTE_POLICY`).
2. Truy cập `/approval-routing/levels/assignments?policyId=f756fb72-8277-4934-8a71-be724b2e83bc`.
3. Bấm nút **"Unassign Slot"** ở Level 1 (đang gán cho `uaT.provision82`).
4. **Kỳ vọng**:
   - Modal hiển thị **Dry-Run Impact Preview** chứa **2 đơn Pending của `uat.provision80`**.
   - Thống kê hiển thị `TOTAL PENDING IMPACTED: 2`, `AUTO RE-ROUTABLE: 2` (hoặc `NEEDS ADMIN ATTENTION` nếu hết slot).
   - Modal giữ cố định Header đen và Nút đỏ `[ X ]` ở đỉnh màn hình.
5. Chọn **Strategy 1 (Automatic Re-route)** và bấm **"Execute Reassignment & Deactivate Slot"**.
6. **Kỳ vọng**:
   - Hệ thống báo thành công. Level Slot hiển thị trạng thái `UNASSIGNED / VACANT`.
   - Vào `/leave-request`, 2 đơn Pending của `uat.provision80` **không còn giữ `uaT.provision82`** mà đã được chuyển sang người duyệt mới hoặc trạng thái `NeedsAdminAttention`.

### Kịch Bản 2: Unassign Slot Trường Hợp Không Có Đơn Pending (Safe Unassign)
1. Chọn một Level Slot ngẫu nhiên chưa từng có đơn Pending nào gán duyệt.
2. Bấm **"Unassign Slot"**.
3. **Kỳ vọng**: Modal hiển thị card màu trắng: `"SAFE UNASSIGNMENT - NO PENDING LEAVE REQUESTS IMPACTED"` và nút **"Deactivate Slot"**.

### Kịch Bản 3: Manual Reassignment (Strategy 2)
1. Mở modal Unassign Slot ở một Level có đơn Pending.
2. Chọn **Strategy 2 (Manual Reassign All to Specific Employee)**.
3. Dropdown hiển thị danh sách người duyệt thay thế hợp lệ (đã được lọc theo trạng thái active và quyền `APPROVE_LEAVE_REQUEST`).
4. Chọn một người duyệt thay thế và bấm thực hiện.
5. **Kỳ vọng**: Tất cả đơn Pending được gán trực tiếp cho người duyệt mới, `SnapshotLevelAssignmentId` được làm sạch để không ảnh hưởng lần Unassign slot sau.

---

## III. Kết Quả Kiểm Tra Kỹ Thuật (Verification Commands)

| Hạng Mục Kiểm Tra | Lệnh / Script | Kết Quả | Trạng Thái |
| :--- | :--- | :--- | :---: |
| **1. C# Web.Backend Build** | `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore` | **Build Succeeded, 0 Error(s)** | **PASS** |
| **2. GitNexus Change Analysis** | `detect_changes` | **Risk Level: LOW** | **PASS** |
| **3. Git Working Tree Status** | `git status --short` | `M` các file Application/Domain/Web | **PASS** |
| **4. Git Diff Name Status** | `git diff --name-status` | Chi tiết file đã sửa | **PASS** |
| **5. Git Diff Whitespace Check** | `git diff --check` | **0 lỗi khoảng trắng** | **PASS** |
| **6. Report BOM & Mojibake** | `scan-mojibake.py --require-bom` | **BOM OK, 0 Mojibake, Exit Code: 0** | **PASS** |
