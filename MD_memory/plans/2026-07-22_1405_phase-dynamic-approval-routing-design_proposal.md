# Proposal & Architecture Design — Phase: Dynamic Approval Routing Engine



> **File Location**: `MD_memory/plans/2026-07-22_1405_phase-dynamic-approval-routing-design_proposal.md`

> **Phase**: `phase-dynamic-approval-routing-design`

> **Date**: 2026-07-22

> **Status**: 📋 DESIGN PROPOSAL (Phase phân tích & thiết kế kiến trúc — KHÔNG CODE SOURCE / KHÔNG DB MIGRATION)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Phân Tích Hiện Trạng & Bằng Chứng Từ Code (RCA & Code Evidence)



### 1.1. Thực trạng `LeaveApproverAssignment`

Dựa trên kiểm tra code thực tế tại `Domain/LeaveApproverAssignments/LeaveApproverAssignment.cs`:

- **Chức năng hiện tại**: Mapping `ApproverEmployeeId` với `TargetDepartmentId` (optional) và `TargetPositionId` (optional) kèm khung thời gian `EffectiveFrom`/`EffectiveTo`.

- **Bản chất**: Đây chỉ là **Filter phạm vi duyệt tĩnh** ("Nhân viên X được duyệt đơn của Phòng Y / Vị trí Z"), hoàn toàn không có thông tin về thứ tự duyệt (Step/Order).

- **Hạn chế**:

  1. Không hỗ trợ quy trình duyệt nhiều cấp (Multi-step approval chain).

  2. Không có khái niệm thứ tự (Order/Sequence).

  3. Không có cơ chế chỉ định đích danh theo từng bước hoặc fallback tự động khi người duyệt inactive/nghỉ việc.

  4. Không hỗ trợ cấu hình quy trình riêng theo từng phòng ban/chính sách.



### 1.2. Vai trò của `Employee.ManagerId`

Dựa trên kiểm tra `Domain/Employees/Employee.cs`:

- `ManagerId` (`public EmployeeId? ManagerId { get; private set; }`) hiện tại chỉ là một **trường dữ liệu báo cáo hành chính (reporting line)** trên hồ sơ nhân sự.

- **Bằng chứng**: `ManagerId` hoàn toàn **KHÔNG được sử dụng** trong bất kỳ handler duyệt đơn phép hay query dashboard W4/W5 nào (`GetPendingApprovalsQueryHandler`, `ApproveLeaveRequestCommandHandler`). Do đó, Manager hiện tại không đồng nghĩa với người duyệt đơn.



### 1.3. Cấu trúc `LeaveRequest` hiện tại

Dựa trên `Domain/LeaveRequests/LeaveRequest.cs`:

- **Trường hiện có**: `EmployeeId`, `LeaveTypeId`, `StartDate`, `EndDate`, `Duration`, `Status` (Pending, Approved, Rejected, Canceled), `CreatedAt`, `ProcessedAt`, `ProcessedBy`, `Comment`.

- **Trường còn thiếu**:

  - Không có `CurrentStepOrder` (đang ở bước duyệt số mấy).

  - Không có `AssignedApproverId` hoặc danh sách ứng viên duyệt hiện tại.

  - Không lưu vết lịch sử duyệt qua từng cấp (chỉ lưu 1 người xử lý cuối cùng qua `ProcessedBy`).



---



## 2. So Sánh Phương Án Kỹ Thuật (Trade-off Analysis)



| Tiêu chí | Option A: Mở rộng `LeaveApproverAssignment` | Option B: Approval Routing Engine Mới (Khuyên dùng) |

|---|---|---|

| **Cấu trúc dữ liệu** | Thêm cột `StepOrder`, `Sequence`, `FallbackMode` vào `LeaveApproverAssignment`. | Xây dựng bộ Domain Entity mới chuyên biệt: `ApprovalPolicy`, `ApprovalStep`, `ApprovalInstance`, `ApprovalStepInstance`. |

| **Tính linh hoạt** | Kém. Phải join phức tạp giữa assignment, position, department. | Rất cao. Phân tách rõ **Rule cấu hình (Policy/Step)** và **Dữ liệu thực thi (Instance/StepInstance)**. |

| **Đa cấp & Fallback** | Khó xử lý. Dễ bị trùng lặp query và race condition khi có nhiều assignment đè nhau. | Tự động phân giải candidate, hỗ trợ 3 cơ chế fallback (`SkipIfNoApprover`, `EscalateToNextStep`, `BlockAndRaiseException`). |

| **Audit Trail** | Chỉ biết ai duyệt cuối (`LeaveRequest.ProcessedBy`). Không lưu vết từng step. | Lưu chi tiết từng step trong `ApprovalStepInstance` và `ApprovalAuditLog` (ai duyệt, lúc nào, bước mấy, lý do). |

| **Tải hệ thống & Dashboard W4/W5** | Trực tiếp query join phức tạp ở runtime -> Chậm khi scale. | Đọc trực tiếp từ `ApprovalStepInstance` đang `Pending` -> Tối ưu index, query siêu nhanh. |

| **Rủi ro Migration** | Rủi ro nợ kỹ thuật (Technical Debt) rất lớn, làm rối entity hiện có. | An toàn. Có thể giữ `LeaveApproverAssignment` song song trong giai đoạn chuyển đổi. |



> **Khuyên dùng (Anti Recommendation)**: **Lựa chọn Option B**. Việc thiết kế một Engine duyệt quy trình riêng biệt (Policy-driven Approval Engine) giúp giải quyết triệt me 100% các scenario phức tạp, bảo toàn kiến trúc Clean Architecture và dễ mở rộng về sau.



---



## 3. Thiết Kế Kiến Trúc & Domain Model (Option B)



### 3.1. Các Domain Entities Đề Xuất



1. **`ApprovalPolicy` (Aggregate Root)**: Cấu hình quy trình duyệt.

   - `Id` (Guid)

   - `Name` (string, e.g. "Quy trình duyệt nghỉ phép IT")

   - `DepartmentId` (Guid?, null = Global policy)

   - `Priority` (int, thứ tự ưu tiên áp dụng policy)

   - `IsActive` (bool)



2. **`ApprovalStep` (Entity thuộc `ApprovalPolicy`)**: Các bước duyệt trong policy.

   - `Id` (Guid)

   - `PolicyId` (Guid)

   - `StepOrder` (int, 1, 2, 3...)

   - `StepName` (string, e.g. "Trưởng nhóm duyệt", "Giám đốc khối duyệt")

   - `ApproverSourceType` (Enum: `ByPosition`, `BySpecificEmployee`, `ByManagerChain`, `ByAssignmentPool`)

   - `TargetPositionId` (Guid?, khi source = `ByPosition`)

   - `SpecificEmployeeId` (Guid?, khi source = `BySpecificEmployee`)

   - `FallbackBehavior` (Enum: `SkipIfNoApprover`, `EscalateToNextStep`, `BlockAndRaiseException`)

   - `AllowSelfApproval` (bool, mặc định `false`)



3. **`ApprovalInstance` (Entity gắn với từng `LeaveRequest`)**: Thể hiện thực thi của quy trình khi đơn được tạo.

   - `Id` (Guid)

   - `LeaveRequestId` (Guid)

   - `PolicyId` (Guid)

   - `CurrentStepOrder` (int)

   - `Status` (Enum: `InProgress`, `Approved`, `Rejected`, `Canceled`, `Blocked`)

   - `CreatedAt` (DateTime)



4. **`ApprovalStepInstance` (Entity thuộc `ApprovalInstance`)**: Trạng thái từng bước duyệt của đơn cụ thể.

   - `Id` (Guid)

   - `ApprovalInstanceId` (Guid)

   - `StepOrder` (int)

   - `StepName` (string)

   - `AssignedApproverEmployeeId` (Guid?, người được chỉ định hoặc phân giải cho bước này)

   - `Status` (Enum: `Pending`, `Approved`, `Rejected`, `Skipped`, `Escalated`)

   - `ProcessedByUserId` (Guid?, User ID người thực tế thao tác duyệt/từ chối)

   - `ProcessedAt` (DateTime?)

   - `Comment` (string?)



---



## 4. Text-Based ERD & Database Schema Proposal



```

+-------------------+        1:N        +-----------------------+

|  ApprovalPolicy   | ----------------> |     ApprovalStep      |

+-------------------+                   +-----------------------+

| Id (PK)           |                   | Id (PK)               |

| Name              |                   | PolicyId (FK)         |

| DepartmentId (FK) |                   | StepOrder             |

| Priority          |                   | ApproverSourceType    |

| IsActive          |                   | TargetPositionId (FK) |

+-------------------+                   | SpecificEmployeeId(FK)|

                                        | FallbackBehavior      |

                                        +-----------------------+



+-------------------+        1:1        +-----------------------+        1:N        +---------------------------+

|   LeaveRequest    | ----------------> |   ApprovalInstance    | ----------------> |   ApprovalStepInstance    |

+-------------------+                   +-----------------------+                   +---------------------------+

| Id (PK)           |                   | Id (PK)               |                   | Id (PK)                   |

| EmployeeId (FK)   |                   | LeaveRequestId (FK)   |                   | ApprovalInstanceId (FK)   |

| Status            |                   | PolicyId (FK)         |                   | StepOrder                 |

| ...               |                   | CurrentStepOrder      |                   | AssignedApproverEmpId(FK) |

+-------------------+                   | Status                |                   | Status                    |

                                        +-----------------------+                   | ProcessedByUserId (FK)    |

                                                                                    +---------------------------+

```



---



## 5. Giải Quyết 7 Scenarios Nghiệp Vụ Yêu Cầu



1. **Phòng IT (Header -> Manager -> Leader -> Employee)**:

   - Cấu hình Policy IT với 3 `ApprovalStep`: Step 1 (`ByPosition` Leader), Step 2 (`ByPosition` Manager), Step 3 (`ByPosition` Header).

2. **Phòng HRM (Header -> Employee)**:

   - Cấu hình Policy HRM với 1 `ApprovalStep`: Step 1 (`ByPosition` Header).

3. **Phòng Kinh doanh (Leader -> Manager -> Employee)**:

   - Cấu hình Policy Sales với thứ tự Step 1 (Manager), Step 2 (Leader). Engine chạy hoàn toàn theo `StepOrder`, không hardcode tên chức danh.

4. **Chỉ định đích danh người duyệt (Specific Approver)**:

   - `ApprovalStep.ApproverSourceType = BySpecificEmployee`, chọn trực tiếp `SpecificEmployeeId`. Không cần thay đổi position của nhân viên đó.

5. **Dynamic / Fallback khi người duyệt nghỉ/không có quyền**:

   - Khi phân giải candidate cho Step X, Engine kiểm tra 4 điều kiện:

     1. Employee active (`IsActive == true`).

     2. Linked User active.

     3. Có quyền `APPROVE_LEAVE_REQUEST`.

     4. Không phải chính người tạo đơn (trừ khi policy cho phép).

   - Nếu KHÔNG thỏa mãn, thực thi `FallbackBehavior`:

     - `SkipIfNoApprover`: Tự động chuyển Step X thành `Skipped` và chuyển sang Step X+1.

     - `EscalateToNextStep`: Đổi candidate của Step X thành cấp tiếp theo.

     - `BlockAndRaiseException`: Chuyển đơn sang trạng thái `Blocked` để Admin/HR can thiệp.

6. **Case Duyệt ngược (Employee duyệt cho Header)**:

   - Cấu hình Step 1 với `SpecificEmployeeId` = Nhân viên bình thường. Engine giải quyết dựa theo policy mà không hề chặn cứng cấp bậc.

7. **Nguồn dữ liệu cho Dashboard W4/W5**:

   - W4/W5 đọc trực tiếp từ `ApprovalStepInstance` có `Status == Pending` và `AssignedApproverEmployeeId == currentEmployee.Id`. Dữ liệu chính xác 100% người đang giữ lượt duyệt.



---



## 6. Chiến Lược Snapshot vs Dynamic & Migration



- **Snapshot Strategy (Khuyên dùng)**: Khi đơn nghỉ phép được tạo (`CreateLeaveRequestCommand`), Engine tìm Policy phù hợp nhất và **snapshot toàn bộ danh sách `ApprovalStepInstance`** cho đơn đó.

- **Admin Recalculate Command**: Nếu nhân viên nghỉ việc hoặc Policy thay đổi giữa chừng, Admin có thể chạy lệnh `RecalculatePendingApprovalRoutingCommand` để phân giải lại candidate cho các đơn đang Pending.

- **Migration từ `LeaveApproverAssignment`**:

  - Giai đoạn 1: Giữ `LeaveApproverAssignment` làm fallback cho các đơn cũ.

  - Giai đoạn 2: Tạo default `ApprovalPolicy` từ các record `LeaveApproverAssignment` hiện có qua script migration.

  - Giai đoạn 3: Deprecate `LeaveApproverAssignment`.



---



## 7. Các Rủi Ro Kỹ Thuật (Risk Matrix)



1. **Circular Manager Chain**: Vòng lặp duyệt khi cấu hình Manager. *Giải pháp*: Giới hạn max depth = 5 và validate vòng lặp khi lưu Policy.

2. **No Approver Found**: Không tìm được ai duyệt ở tất cả các step. *Giải pháp*: Default fallback về Admin/HR Global Pool.

3. **Self-Approval**: Người tạo đơn trùng với người duyệt. *Giải pháp*: Tự động `Skip` bước đó hoặc báo lỗi nếu là step duy nhất.

4. **Duplicate Candidates**: Nhiều người cùng thỏa mãn 1 step. *Giải pháp*: Gán cho Pool hoặc chọn người có seniority cao hơn.



---



## 8. Các Câu Hỏi Cần User / Codex Chốt Trước Khi Code



1. **Xác nhận lựa chọn Option B**: User có đồng ý phát triển bộ Domain Entity `ApprovalPolicy` / `ApprovalStep` mới thay vì cố sửa `LeaveApproverAssignment` không?

2. **Chiến lược Snapshot**: User có đồng ý snapshot danh sách người duyệt vào từng đơn khi tạo đơn (`ApprovalStepInstance`) để tối ưu hiệu năng Dashboard W4/W5 không?

3. **Cơ chế Fallback mặc định**: Khi tất cả các bước duyệt đều không tìm thấy người duyệt hợp lệ, đơn có nên tự động chuyển về cho Admin/HR (`BlockAndRaiseException`) không?
