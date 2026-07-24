# Báo Cáo Phân Tích & Hiệu Chỉnh Kỹ Thuật Phạm Vi An Toàn (Scope-Safe RCA & Fix Report v3) — Phase 9 Approval Routing Unassign & Re-route

**Ngày thực hiện**: 2026-07-24  
**Phân vùng kỹ thuật**: Approval Routing / Dynamic Approval Engine Scope Protection  
**Tuân thủ ranh giới kĩ thuật**:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

---

## I. Phân Tích Kỹ Thuật Về Lỗi Lọc Quá Rộng (Broad Scope Filter Critique & RCA)

### 1. Phân Tích Phản Biển Kỹ Thuật (Critique of Broad Scope Filtering)
- **Vấn đề của phương án cũ**: Điều kiện `a.SnapshotPolicyId != null` trong `ApprovalReassignmentService` là **quá rộng và không an toàn**. Nó sẽ vô tình gom toàn bộ đơn Pending của người duyệt bị Unassign thuộc BẤT KỲ Chính sách (Policy) nào có SnapshotPolicyId.
- **Yêu cầu phạm vi nghiêm ngặt**: Khi thao tác Unassign 1 Level Slot, hệ thống **chỉ được phép ảnh hưởng đúng các đơn Pending thực sự thuộc Level Slot đó**, hoặc thuộc Route Instance được chứng minh trực tiếp bằng ngữ cảnh ghép cặp giữa Policy, Level và Candidate.

---

## II. Phương Án Thiết Kế Kỹ Thuật Chuẩn Phạm Vi (Scope-Safe Architecture Solution)

### 1. Mở Rộng Dữ Liệu Ngữ Cảnh Trong Command (`ReassignPendingLeaveRequestsCommand.cs`)
Bổ sung các tham số ngữ cảnh định danh chính xác phạm vi thao tác Unassign:
```csharp
public sealed record ReassignPendingLeaveRequestsCommand(
    Guid TargetEmployeeId,
    Guid? NewApproverEmployeeId,
    bool AutoRerouteUsingResolver,
    string Reason,
    Guid? TargetLevelAssignmentId = null,
    Guid? TargetPolicyId = null,
    Guid? TargetLevelId = null) : ICommand<ReassignPendingLeaveRequestsResponse>;
```

### 2. Truyền Đầy Đủ Ngữ Cảnh Từ Handler (`UnassignApprovalLevelCommandHandler.cs`)
Khi Operator bấm Unassign 1 slot, Handler xác định chính xác cả 3 định danh: `targetAssignment.Id`, `targetLevel.Id`, và `targetPolicy.Id` để truyền vào Reassign Command:
```csharp
var reassignCommand = new ReassignPendingLeaveRequestsCommand(
    unassignedEmployeeId.Value,
    NewApproverEmployeeId: request.NewApproverEmployeeId,
    AutoRerouteUsingResolver: request.AutoRerouteUsingResolver,
    Reason: $"Unassigning level slot assignment ID {request.LevelAssignmentId}: {request.Reason}",
    TargetLevelAssignmentId: request.LevelAssignmentId,
    TargetPolicyId: targetPolicy.Id.Value,
    TargetLevelId: targetLevel.Id.Value);
```

### 3. Lọc Phạm Vi Chặt Chẽ Tại Query Impact (`GetLevelAssignmentUnassignImpactQueryHandler.cs`)
Thay vì so sánh tổng quát, Query chỉ lấy các Candidate thuộc đúng `targetLevel.Id` trong `targetPolicy.Id`, sau đó đối chiếu khớp 100%:
```csharp
var targetCandidateIds = targetPolicy.Rules
    .SelectMany(r => r.Candidates)
    .Where(c => c.ApprovalRouteLevelId == targetLevel.Id)
    .Select(c => c.Id)
    .ToHashSet();

var slotPendingAssignments = pendingAssignments
    .Where(a => a.LeaveRequest != null &&
                a.LeaveRequest.Status == LeaveRequestStatus.Pending &&
                (a.SnapshotLevelAssignmentId == targetAssignment.Id ||
                 (a.SnapshotPolicyId == targetPolicy.Id &&
                  a.AssignedApproverEmployeeId == targetAssignment.AssignedEmployeeId &&
                  a.SnapshotCandidateId != null &&
                  targetCandidateIds.Contains(a.SnapshotCandidateId))))
    .ToList();
```
- **Cam kết tuyệt đối**:
  - Không bao giờ gom đơn từ Policy khác.
  - Không bao giờ gom đơn từ Level khác trong cùng Policy.
  - Không bao giờ gom đơn thuộc Candidate Slot khác.

### 4. Tầng Service Thực Thi (`ApprovalReassignmentService.cs`)
Trong `ApprovalReassignmentService`, khi `TargetLevelAssignmentId` và `TargetPolicyId` được truyền vào, Service thực thi chính xác phạm vi lọc theo Level Slot Assignment và Policy ID của Slot bị hủy, hoàn toàn loại bỏ điều kiện `SnapshotPolicyId != null`.

---

## III. Tách Biệt Việc Khôi Phục Dữ Liệu Cũ (Data Repair Isolation)

1. **Sửa code Runtime**: Đã thực hiện xong và verify 100% sạch build.
2. **Khôi phục dữ liệu CSDL lịch sử**: Đã tạo script độc lập `MD_memory/debug/2026-07-24_1805_data_repair_proposal.py`. Script này **KHÔNG tự động chạy** và chỉ được thực thi khi User/Codex xem xét và phê duyệt qua câu lệnh trực tiếp.

---

## IV. Kết Quả Kiểm Tra Kỹ Thuật Tự Động (Technical Verification)

| Hạng Mục Kiểm Tra | Lệnh / Công Cụ | Kết Quả | Trạng Thái |
| :--- | :--- | :--- | :---: |
| **1. C# Web.Backend Build** | `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore` | **Build Succeeded, 0 Error(s)** | **PASS** |
| **2. GitNexus Change Analysis** | `detect_changes` | **Risk Level: LOW (0 Affected Processes)** | **PASS** |
| **3. Git Working Tree Status** | `git status --short` | `M` các file Application / Domain / Web | **PASS** |
| **4. Git Diff Name Status** | `git diff --name-status` | Chi tiết các file C# | **PASS** |
| **5. Git Diff Whitespace Check** | `git diff --check` | **0 lỗi khoảng trắng** | **PASS** |
| **6. Report BOM & Mojibake** | `scan-mojibake.py --require-bom` | **BOM OK, 0 Mojibake, Exit Code: 0** | **PASS** |
