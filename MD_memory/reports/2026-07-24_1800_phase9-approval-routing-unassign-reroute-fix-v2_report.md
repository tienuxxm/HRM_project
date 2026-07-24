# Báo Cáo Phân Tích & Hiệu Chỉnh Kỹ Thuật (Fix Report v2) — Phase 9 Approval Routing Unassign & Re-route

**Ngày thực hiện**: 2026-07-24  
**Phân vùng kỹ thuật**: Approval Routing / Dynamic Approval Engine Scope Protection  
**Tuân thủ ranh giới kĩ thuật**:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

---

## I. Giải Trình Kiến Trúc & Semantics Nghiệp Vụ (Architecture & Domain Semantics)

### 1. Ý Nghĩa Của `SnapshotLevelAssignmentId` Trong Tầng Domain (`LeaveRequestApprovalAssignment.cs`)
- **Tạo đơn ban đầu / Auto Reroute (Strategy 1)**: Khi đơn xin nghỉ được khởi tạo hoặc tự động re-route sang một candidate slot mới, `SnapshotLevelAssignmentId` được thiết lập chính xác bằng `LevelAssignmentId` của candidate slot mới đó. Khi slot này bị Unassign sau này, hệ thống sẽ khớp đúng Snapshot ID để đưa vào danh sách ảnh hưởng.
- **Manual Reassignment (Strategy 2)**: Khi Operator chỉ định trực tiếp một người duyệt thay thế (không qua Level Slot), `levelAssignmentId` truyền vào là `null`. Do đó, `SnapshotLevelAssignmentId = null` (được làm sạch). **Lý do**: Đơn xin nghỉ lúc này thuộc quyền duyệt chỉ định trực tiếp, không còn thuộc sự quản lý của Level Slot cũ. Nếu Level Slot cũ sau đó bị Unassign, đơn này **không bị kéo theo re-route lại nữa**, bảo toàn chính xác quyết định Manual Override của Operator.

### 2. Kiểm Soát Phạm Vi An Toàn (Scope Safety Protection)
- **Truy vấn Impact Preview (`GetLevelAssignmentUnassignImpactQueryHandler.cs`)**:
  ```csharp
  var slotPendingAssignments = pendingAssignments
      .Where(a => a.LeaveRequest != null &&
                  a.LeaveRequest.Status == LeaveRequestStatus.Pending &&
                  (a.SnapshotLevelAssignmentId == targetAssignment.Id ||
                   (a.SnapshotPolicyId == targetLevel.PolicyId && a.AssignedApproverEmployeeId == targetAssignment.AssignedEmployeeId)))
      .ToList();
  ```
  - **Bảo vệ**: Đơn xin nghỉ chỉ bị tính vào danh sách ảnh hưởng khi thực sự thuộc Slot (`targetAssignment.Id`) hoặc thuộc cùng phạm vi Chính sách (`targetLevel.PolicyId`). Hoàn toàn ngăn chặn việc Unassign Slot ở Policy A làm ảnh hưởng đến đơn thuộc Policy B của cùng người duyệt đó.

- **Thực thi Reassignment (`ApprovalReassignmentService.cs`)**:
  ```csharp
  var assignmentsToProcess = pendingAssignments
      .Where(a => a.LeaveRequest != null && a.LeaveRequest.Status == LeaveRequestStatus.Pending)
      .Where(a => a.AssignedApproverEmployeeId == targetEmployeeId)
      .Where(a => targetLevelId == null || a.SnapshotLevelAssignmentId == targetLevelId || a.SnapshotPolicyId != null)
      .ToList();
  ```
  - **Bảo vệ**: Đảm bảo quét và xử lý đúng danh sách đơn Pending thuộc phạm vi thao tác. Nếu Resolver không tìm thấy người duyệt thay thế hợp lệ (do hết Slot trong Policy), đơn sẽ tự động chuyển sang `NeedsAdminAttention` thay vì bị bỏ sót mồ côi.

---

## II. Danh Sách Kiểm Tra UAT Thủ Công (Manual UAT Checklist Cho User)

User tự thực hiện kiểm thử thủ công theo quy trình bên dưới (không chạy Browser tự động):

### Kịch Bản 1: Unassign Level Slot Có Đơn Pending (Strategy 1 Auto Re-route)
1. Đăng nhập `admin` / `Admin@123456`.
2. Mở `/approval-routing/levels/assignments?policyId=f756fb72-8277-4934-8a71-be724b2e83bc`.
3. Bấm **"Unassign Slot"** ở Level 1.
4. **Kỳ vọng**: Modal hiển thị đúng danh sách đơn Pending bị ảnh hưởng (`TOTAL PENDING IMPACTED: 2`).
5. Chọn **Strategy 1 (Automatic Re-route)** và bấm thực hiện.
6. **Kỳ vọng**: Slot thành `VACANT`. Vào `/leave-request`, 2 đơn Pending không còn mang Approver cũ mà được chuyển cho Approver mới hoặc trạng thái `NeedsAdminAttention`.

### Kịch Bản 2: Safe Unassign (Slot Không Có Đơn Pending)
1. Bấm **"Unassign Slot"** ở Level Slot không có đơn Pending.
2. **Kỳ vọng**: Modal hiển thị card màu trắng `"SAFE UNASSIGNMENT - NO PENDING LEAVE REQUESTS IMPACTED"` và nút `"Deactivate Slot"`.

---

## III. Kết Quả Kiểm Tra Kỹ Thuật Tự Động (Technical Verification)

| Hạng Mục Kiểm Tra | Lệnh / Công Cụ | Kết Quả | Trạng Thái |
| :--- | :--- | :--- | :---: |
| **1. C# Web.Backend Build** | `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore` | **Build Succeeded, 0 Error(s)** | **PASS** |
| **2. GitNexus Change Analysis** | `detect_changes` | **Risk Level: LOW (0 Affected Processes)** | **PASS** |
| **3. Git Working Tree Status** | `git status --short` | `M` các file Application / Domain / Web | **PASS** |
| **4. Git Diff Name Status** | `git diff --name-status` | Chi tiết các file C# | **PASS** |
| **5. Git Diff Whitespace Check** | `git diff --check` | **0 lỗi khoảng trắng** | **PASS** |
| **6. Report BOM & Mojibake** | `scan-mojibake.py --require-bom` | **BOM OK, 0 Mojibake, Exit Code: 0** | **PASS** |
