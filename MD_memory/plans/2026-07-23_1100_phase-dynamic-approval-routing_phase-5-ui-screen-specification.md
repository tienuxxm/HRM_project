# Phase 5 UI Screen Specification: Dynamic Approval Routing Module (`/approval-routing`)



**Date**: 2026-07-23

**Author**: Senior .NET Fullstack Engineer & Technical Reviewer (Anti)

**Status**: APPROVED VISIBLE CANVAS CLUSTER SPECIFICATION — WAITING FOR USER / CODEX APPROVAL

**Stitch Project ID**: `17479353588209716186`

**Design System**: Swiss International HR (`assets/f4fbeeb3791c4c52991dd52c4fb92635`)

**Evidence Source**: `MD_memory/evidence/2026-07-23_phase5_stitch/phase5_stitch_screen_evidence.html`



---



## 1. Clean Architecture Boundary & Scope Restatement



To preserve systemic stability and layer isolation across the solution, all UI components strictly respect the following layer boundaries:



```text

Web.Backend -> Application -> Domain

Infrastructure -> Application/Domain

```



- **Domain**: Base entities (`ApprovalRoutePolicy`, `ApprovalRouteLevel`, `ApprovalRouteLevelAssignment`, `ApprovalRouteRule`, `ApprovalRouteRuleCandidate`, `LeaveRequestApprovalAssignment`). No UI or framework dependencies.

- **Application**: CQRS Commands/Queries & Resolution Services (`ReassignPendingLeaveRequestsCommand`, `UnassignApprovalLevelCommand`, `GetEmployeeDeactivationImpactQuery`, `ApprovalRouteResolverService`).

- **Infrastructure**: EF Core mappings, DbContext persistence, and audit logging.

- **Web.Backend**: ASP.NET Core MVC Controllers (`ApprovalRoutingController`), ViewModels, Razor Views (`/Views/ApprovalRouting/`), and AJAX partial endpoints.



---



## 2. Canvas Cluster Analysis & Phản Biện Chuyên Môn



### 2.1 Scope Xóa Bỏ Sự Khác Biệt Giữa Report Cũ Và Canvas Reality



Dựa trên bằng chứng từ `MD_memory/evidence/2026-07-23_phase5_stitch/phase5_stitch_screen_evidence.html`:

1. Cụm canvas visible chính thức của Phase 5 hiện tại bao gồm **chính xác 7 màn hình visible**.

2. Các màn hình như `Legacy Mapping & Migration Console` (Desktop/Mobile) và `Impact Preview & Reassignment Decision` (Mobile) có tồn tại dưới dạng resource Stitch ID (`3fb828b034af45e8bf660339482cf2f7`, `a737a59f68824511985e572f5f9d0d39`, `79edd9cacfd04445aaa04638c5a29d20`) nhưng **KHÔNG nằm trong cụm visible trên canvas đã chốt**.

3. Do đó, tài liệu specification này tập trung 100% vào **7 màn hình visible chính thức**. `Legacy Mapping & Migration Console` được phân loại là *resource exists but not visible on approved canvas cluster*, không nằm trong scope UI Phase 5 trừ khi User/Codex yêu cầu regenerate.



### 2.2 Phản Biện Về Mẫu Dữ Liệu Hardcode `Engineering Policy`



Màn hình `Level Slot Assignments - Engineering Policy` trên Stitch Canvas hiển thị tiêu đề và dữ liệu mẫu của phòng "Engineering".

- **Xác nhận**: Đây thuần túy là **Sample Data** được tạo ra trên Stitch Canvas để minh họa bố cục dữ liệu thực tế.

- **Cam kết Implementation**: Ở tầng code C# / Razor View, tuyệt đối **KHÔNG HARDCODE** chuỗi "Engineering". View sẽ nhận `policyId` động (`/approval-routing/levels/assignments?policyId={id}`) và render tên phòng ban, tên policy, danh sách level slots động từ database thông qua Query Handler.



### 2.3 Phản Biện Về Việc Gộp Nghiệp Vụ Trên Màn Hình `Policy Detail & Rule Configuration`



Màn hình `Policy Detail & Rule Configuration` đang gộp 3 phân vùng nghiệp vụ chính:

1. **Policy Overview Header**: Thông tin chung policy (Tên, Phòng ban, Trạng thái, Chế độ duyệt Single-Step).

2. **Level Slots Summary Box**: Tóm tắt các level slot hiện có trong policy.

3. **Configured Position Rules & Candidates**: Danh sách cấu hình priority candidates và specific approver override cho từng vị trí công việc (requester position).



- **Đánh giá**: Việc gộp này mang lại cái nhìn tổng thể 360-degree cho HR Admin mà không phải chuyển trang liên tục. Tuy nhiên, khi triển khai Razor MVC, thao tác **thêm/sửa Position Rule** hoặc **thêm/sửa Level Slot** nên được tách thành các **Modal Partial Views** riêng biệt (`_AddEditPositionRuleModal.cshtml`, `_AddEditLevelSlotModal.cshtml`) để tránh làm phình form chính và giữ AJAX handler đơn gọn.



---



## 3. Chi Tiết Cụ Thể 7 Màn Hình Visible Trên Canvas



---



### Màn Hình 1: Approval Routing Policies - Swiss International Style (Desktop)



- **Canvas Screen ID**: `ffb3498505444a46ad87b1c37125e939`

- **Viewport**: Desktop (1440px)



#### 1. Purpose

Màn hình trung tâm quản lý danh sách các chính sách tuyến duyệt theo phòng ban (`/approval-routing/policies`). Cho phép HR Admin/System Admin xem toàn bộ chính sách đang hoạt động hoặc ngắt hoạt động, tìm kiếm theo phòng ban và khởi tạo policy mới.



#### 2. User Role / Actor

- **System Admin / HR Director**: Co quyen tao moi (`UPDATE_DEPARTMENT`), chinh sua policy, chuyen trang thai Active/Inactive.

- **HR Specialist / Department Manager**: Co quyen xem danh sach (`VIEW_DEPARTMENT`).



#### 3. Data Displayed

- Bảng dữ liệu Ledger Grid (1px border, Geist font):

  - `Policy Name` (string, e.g. "Engineering Approval Policy")

  - `Department` (string, e.g. "Engineering")

  - `Active Rules` (int count, e.g. "4 Rules")

  - `Level Slots` (string summary, e.g. "Level 1 (Tech Lead), Level 2 (VP Eng)")

  - `Status` (Badge: `ACTIVE` solid black, `INACTIVE` viền đỏ Swiss `#E62429`)

  - `Last Updated` (JetBrains Mono date, e.g. "2026-07-22")

  - `Actions` (Buttons: `[View Policy]`, `[Configure Rules]`, `[Delete]`)

- Thanh bộ lọc: Search Input (`Filter by department...`), Department Dropdown.

- Bottom Banner: Cảnh báo legacy table `LeaveApproverAssignment` đã bị ngắt ghi.



#### 4. User Interactions

- **Search / Filter**: Filter danh sách policy tức thì bằng JavaScript client-side hoặc AJAX update partial table.

- **Click `[+ Create Department Policy]`**: Mở modal/drawer tạo policy mới cho phòng ban chưa có policy.

- **Click `[View Policy]` / `[Configure Rules]`**: Điều hướng sang trang `/approval-routing/policies/detail?id={id}`.

- **Click `[Delete]`**: Hiển thị modal xác nhận xóa policy (chỉ cho phép nếu policy `INACTIVE` và không chứa pending requests).



#### 5. Business Rules Enforced

- **Department-Specific Policy Only**: Mỗi phòng ban chỉ có tối đa 1 Policy chính thức.

- **No Default / Fallback Policy**: Không cho phép tạo policy "Global Default" dùng chung toàn công ty.

- **Read-Only Legacy Table**: Khóa toàn bộ ghi trên bảng `LeaveApproverAssignment` cũ.



#### 6. Empty / Warning / Error States

- **Chưa có Policy nào**: Bảng hiển thị thông báo "No approval policies configured yet. Click '+ Create Department Policy' to start."

- **Policy Inactive**: Hiển thị badge outline Đỏ Swiss `#E62429` kèm cảnh báo "Policy is inactive. Employees in this department cannot submit leave requests."



#### 7. Implementation Mapping

- **Route**: `GET /approval-routing/policies`

- **Controller Action**: `ApprovalRoutingController.Policies(string? department, string? search)`

- **ViewModel**: `PolicyListViewModel` (chứa `List<PolicySummaryDto> Policies`)

- **Razor View**: `HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/Policies.cshtml`



#### 8. UAT Checklist

1. Đăng nhập tài khoản Admin, truy cập `/approval-routing/policies`.

2. Kiểm tra danh sách hiển thị đúng tên Policy, Phòng ban, Số rule, Số Level Slot.

3. Gõ từ khóa tìm kiếm phòng ban -> Danh sách lọc chính xác.

4. Kiểm tra Policy `ACTIVE` có badge Đen solid, Policy `INACTIVE` có badge Đỏ Swiss viền hairline.



---



### Màn Hình 2: Approval Routing Policies - Swiss Mobile Admin (Mobile)



- **Canvas Screen ID**: `23a89f4ff9514549b6d7664b42e813a3`

- **Viewport**: Mobile (390px)



#### 1. Purpose

Phiên bản Mobile Admin của danh sách chính sách tuyến duyệt, chuyển đổi bảng dữ liệu rộng thành các Card khối vuông 0px border-radius xếp chồng, phục vụ thao tác trên thiết bị di động.



#### 2. User Role / Actor

- **HR Director / Admin Mobile**: Xem và quản lý policy nhanh trên điện thoại.



#### 3. Data Displayed

- Mobile Top Bar: Breadcrumb tiêu đề `APPROVAL ROUTING POLICIES`.

- Full-width Search Input & Nút `+ CREATE POLICY` đen solid.

- Danh sách Stacked Policy Cards (0px radius, 1px border `#D1D1D1`):

  - Tên Policy, Tên Phòng ban.

  - Badge trạng thái (`ACTIVE` solid đen / `INACTIVE` outline đỏ).

  - Tóm tắt số Active Rules & Level Slots.

  - Ngày cập nhật format `YYYY-MM-DD` (font mono).

  - Nút thao tác chữ vuông `[VIEW POLICY]`, `[CONFIGURE]`.



#### 4. User Interactions

- Tap Search input để gõ tìm kiếm.

- Tap nút `[CONFIGURE]` để chuyển tiếp tới màn hình chi tiết policy.

- Tap Bottom Navigation để chuyển giữa Dashboard, Directory, Policies, Settings.



#### 5. Business Rules Enforced

- Đảm bảo tính nhất quán dữ liệu 100% với màn hình Desktop.

- Không thu gọn bỏ bớt thông tin quan trọng (Active Rules & Level Slots vẫn hiển thị rõ ràng).



#### 6. Empty / Warning / Error States

- Giữ nguyên hiển thị empty state và warning badge Đỏ Swiss như bản Desktop.



#### 7. Implementation Mapping

- **Route**: `GET /approval-routing/policies` (Responsive CSS Media Query `@media (max-width: 768px)` trong cùng Razor View `Policies.cshtml`).



#### 8. UAT Checklist

1. Mở giao diện trên mobile viewport (390px width).

2. Kiểm tra layout chuyển sang dạng stacked cards 0px radius.

3. Kiểm tra các button full-width dễ chạm tap.



---



### Màn Hình 3: Policy Detail & Rule Configuration - Swiss Intl Style (Desktop)



- **Canvas Screen ID**: `47ffe8565a154cd19fbd03f0b5eec574`

- **Viewport**: Desktop (1440px)



#### 1. Purpose

Màn hình quản lý chi tiết một chính sách tuyến duyệt phòng ban cụ thể (`/approval-routing/policies/detail?id={id}`). Cho phép thiết lập các quy tắc theo vị trí công việc người gửi đơn (Requester Position Rules), chuỗi ứng viên ưu tiên (Candidate Priority Sequence), và chỉ định người duyệt đích (Specific Approver Override).



#### 2. User Role / Actor

- **System Admin / HR Specialist**: Được phép cấu hình quy tắc duyệt, thêm/sửa/xóa Candidate, bật/tắt Specific Approver Override.



#### 3. Data Displayed

- **Header Block**: Tên Policy, Phòng ban, Policy ID (`POL-ENG-2026-01` font mono), Trạng thái (`ACTIVE`).

- **2-Column Summary Panel**:

  - Panel trái: Overview & Single-Step Superior Routing Mode.

  - Panel phải: Level Slots Summary (Level 1: Tech Lead, Level 2: VP Eng) & Nút `[Manage Level Slots]`.

- **Configured Position Rules Section**:

  - Danh sách từng vị trí requester (e.g., "Junior Software Engineer", "Engineering Manager").

  - Với chế độ Candidate Sequence: Danh sách Priority Candidates theo thứ tự (Priority 1: Level 1 Slot -> Tran Van Lead; Priority 2: Level 2 Slot -> Le Hoang VP).

  - Với chế độ Specific Approver Override: Tên & Mã NV chỉ định duyệt trực tiếp (e.g., "Le Hoang VP - EMP-002") kèm ghi chú "Direct route to VP of Engineering".

  - Actions: `[Edit Candidates]`, `[Delete Rule]` (Red text).



#### 4. User Interactions

- **Click `[Manage Level Slots]`**: Điều hướng sang trang quản lý Level Slots `/approval-routing/levels/assignments?policyId={id}`.

- **Click `[+ Add Position Rule]`**: Mở modal thêm quy tắc mới cho vị trí nhân sự trong phòng ban.

- **Click `[Edit Candidates]`**: Mở modal chỉnh sửa danh sách priority candidates hoặc bật/tắt Specific Approver Override.

- **Click `[Simulate Route Resolver (Dry-Run)]`**: Mở modal test chạy thử thuật toán resolver cho một nhân viên giả lập mà không lưu dữ liệu.



#### 5. Business Rules Enforced

- **Strict Config-Driven Routing**: Mọi quy tắc phải dựa trên Position Code và Priority Index (1, 2, 3...).

- **Specific Approver Override Integrity**: Nếu Specific Approver bị inactive hoặc không hợp lệ -> Hệ thống fail ngay lập tức, **KHÔNG TỰ ĐỘNG FALLBACK** về candidate routing.

- **No Hardcoded Rules**: Không hardcode logic auto-approve cho bất kỳ vị trí nào (kể cả CEO).



#### 6. Empty / Warning / Error States

- **Chưa có Rule nào**: Cảnh báo Đỏ Swiss: "Warning: Policy has 0 position rules. Employees in this department will fail validation when submitting leave requests."

- **Rule thiếu Candidate**: Báo lỗi inline bên dưới quy tắc: "No active candidate slot configured for this rule."



#### 7. Implementation Mapping

- **Route**: `GET /approval-routing/policies/detail/{id}`

- **Controller Action**: `ApprovalRoutingController.PolicyDetail(Guid id)`

- **ViewModel**: `PolicyDetailViewModel`

- **Razor View**: `HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/PolicyDetail.cshtml`



#### 8. UAT Checklist

1. Truy cập chi tiết policy của phòng ban.

2. Thêm rule mới cho vị trí "Junior Software Engineer" với Priority 1 (Level 1) và Priority 2 (Level 2).

3. Thêm rule có Specific Approver Override chỉ định thẳng cho một Manager.

4. Kiểm tra các button xóa rule hiển thị text màu Đỏ Swiss `#E62429`.



---



### Màn Hình 4: Policy Detail & Rule Configuration - Swiss Mobile (Mobile)



- **Canvas Screen ID**: `ac64ecf42c9947c484c4a8cdb662c428`

- **Viewport**: Mobile (390px)



#### 1. Purpose

Phiên bản Mobile Admin của trang chi tiết policy và cấu hình quy tắc duyệt, tối ưu hóa giao diện dạng các khối thông tin xếp chồng cho màn hình di động.



#### 2. User Role / Actor

- **HR Admin Mobile**: Cấu hình quy tắc duyệt hoặc kiểm tra cấu hình vị trí ngay trên điện thoại.



#### 3. Data Displayed

- Top Navigation Mobile: Tiêu đề `POLICY DETAILS & RULES`.

- Policy Overview Card: Tên policy, phòng ban, trạng thái `ACTIVE` (badge đen), các nút full-width outline `[Manage Level Slots]`, `[Edit Policy]`.

- Position Rules Section: Tiêu đề `POSITION RULES (4)`, nút `+ Add Rule` đen solid full-width.

- Stacked Rule Cards:

  - Card dạng Sequence Mode: Danh sách ứng viên đánh số thứ tự (1, 2...).

  - Card dạng Override Mode: Tên người duyệt chỉ định kèm note chữ mờ.

  - Đường link thao tác `[Edit]` (chữ đen gạch chân) và `[Delete]` (chữ Đỏ Swiss gạch chân).

- Fixed Bottom Footer: Nút full-width đen solid `[Simulate Resolver (Dry-Run)]`.



#### 4. User Interactions

- Tap `[Manage Level Slots]` để chuyển màn hình phân công slot.

- Tap `[Edit]` / `[Delete]` để thao tác với từng rule.

- Tap `[Simulate Resolver (Dry-Run)]` dính ở đáy màn hình để chạy thử resolver test.



#### 5. Business Rules Enforced

- Đảm bảo hiển thị đầy đủ thông tin Sequence vs Override mode tương đương bản Desktop.



#### 6. Empty / Warning / Error States

- Tương tự bản Desktop với giao diện full-width trên mobile.



#### 7. Implementation Mapping

- **Route**: `GET /approval-routing/policies/detail/{id}` (Mobile CSS Viewport trong `PolicyDetail.cshtml`).



#### 8. UAT Checklist

1. Mở màn hình trên màn hình mobile 390px.

2. Xác nhận nút `[Simulate Resolver]` luôn cố định ở đáy màn hình.

3. Kiểm tra các thông tin Candidate Sequence không bị đè chữ hay tràn viền.



---



### Màn Hình 5: Level Slot Assignments - Engineering Policy (Desktop - Dynamic Sample)



- **Canvas Screen ID**: `07e8ed1983434404825cfcc4076bef44`

- **Viewport**: Desktop (1440px)



> [!NOTE]

> Tiêu đề "Engineering Policy" trên canvas chỉ là mẫu dữ liệu minh họa. Triển khai code C# sẽ nhận `policyId` động cho bất kỳ phòng ban nào.



#### 1. Purpose

Màn hình phân công nhân sự cụ thể vào các vị trí Level Slot (e.g. Level 1: Tech Lead Slot, Level 2: VP Eng Slot) trong chính sách duyệt (`/approval-routing/levels/assignments?policyId={id}`).



#### 2. User Role / Actor

- **HR Admin / Operations Manager**: Thực hiện phân công (assign), thay thế (change approver), hoặc unassign nhân sự khỏi level slot.



#### 3. Data Displayed

- Header Block: `LEVEL SLOT ASSIGNMENTS - [DEPARTMENT NAME] POLICY`.

- Subtext: "Assign active employees to department level slots. Each level slot must have exactly 1 active assignment."

- Action Bar: Bộ lọc Dropdown Level Slot, Nút `+ ASSIGN EMPLOYEE TO LEVEL SLOT` đen solid.

- Spreadsheet Grid Table (1px hairline borders):

  - `LEVEL SLOT` (string, e.g. "Level 1: Tech Lead Slot")

  - `CURRENTLY ASSIGNED EMPLOYEE` (string, e.g. "Tran Van Lead - EMP-012")

  - `EFFECTIVE FROM` (JetBrains Mono date, e.g. "2026-01-01")

  - `EFFECTIVE TO` (string, e.g. "Indefinite (Active)")

  - `ASSIGNMENT STATUS` (Badge: `ACTIVE` solid đen, `UNASSIGNED` outline Đỏ Swiss `#E62429`)

  - `IMPACTED PENDING REQUESTS` (string count, e.g. "3 Pending Requests")

  - `ACTIONS` (Buttons: `[Change Approver]`, `[Unassign Slot]` chữ đỏ)

- Bottom Red Warning Callout: "Warning: Unassigning an employee from a level slot will trigger an automatic dry-run impact preview and reassign/reroute all affected pending leave requests."



#### 4. User Interactions

- **Click `[Change Approver]`**: Mở modal chọn nhân sự thay thế cho slot.

- **Click `[Unassign Slot]`**: **KÍCH HOẠT DRY-RUN IMPACT QUERY** (`GetEmployeeDeactivationImpactQuery`) và bật Modal Preview Reassignment (`Dry-Run Impact Preview & Reassignment Decision`).

- **Click `[+ ASSIGN EMPLOYEE]`** trên dòng Vacant: Mở modal gán nhân sự mới vào slot đang trống.



#### 5. Business Rules Enforced

- **Single Active Assignment Per Slot**: Một level slot tại một thời điểm chỉ có tối đa 1 phân công nhân sự active.

- **No Unassigned Slot Without Impact Handling**: Khi unassign một slot đang có pending requests phụ thuộc, **BẮT BUỘC** phải xử lý reroute hoặc chuyển `NeedsAdminAttention`.

- **No Self Approval**: Nhân viên được assign vào slot không thể tự duyệt đơn nghỉ do chính mình tạo.



#### 6. Empty / Warning / Error States

- **Slot Vacant (Chưa phân công)**: Hiển thị dòng `-- Vacant Slot --`, badge `UNASSIGNED` viền Đỏ Swiss, số đơn ảnh hưởng = 0.

- **Chưa chọn Approver hợp lệ**: Báo lỗi validation nếu chọn nhân sự đã bị deactive, chưa linked user, hoặc không có permission `APPROVE_LEAVE_REQUEST`.



#### 7. Implementation Mapping

- **Route**: `GET /approval-routing/levels/assignments?policyId={id}`

- **Controller Action**: `ApprovalRoutingController.LevelAssignments(Guid policyId)`

- **ViewModel**: `LevelAssignmentViewModel`

- **Razor View**: `HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/LevelAssignments.cshtml`



#### 8. UAT Checklist

1. Truy cập danh sách level slot của một chính sách.

2. Kiểm tra slot đã gán hiển thị tên nhân viên, mã NV, ngày hiệu lực và số đơn pending bị ảnh hưởng.

3. Click `[Unassign Slot]` -> Kiểm tra hệ thống mở Modal Dry-Run Impact Preview thay vì xóa ngay lập tức.



---



### Màn Hình 6: Level Slot Assignments - Swiss Mobile (Mobile - Dynamic Sample)



- **Canvas Screen ID**: `e2792249f5204a4fab1aab4b4d1df5d5`

- **Viewport**: Mobile (390px)



#### 1. Purpose

Phiên bản Mobile Admin quản lý phân công Level Slot, chuyển đổi bảng spreadsheet thành các thẻ card đứng trên màn hình 390px.



#### 2. User Role / Actor

- **HR Admin Mobile**: Thực hiện xem và thay đổi phân công slot duyệt trên di động.



#### 3. Data Displayed

- Top Navigation: `LEVEL SLOT ASSIGNMENTS`.

- Context Card: Tên policy, tổng số slot, phòng ban.

- Nút `+ Assign Employee to Slot` đen solid full-width.

- Stacked Mobile Level Slot Cards:

  - Slot Active: Tên slot, Tên + Mã NV được gán, Ngày hiệu lực, Badge `ACTIVE`, Số đơn pending bị ảnh hưởng, Nút `[Change Approver]` và `[Unassign Slot]` (chữ đỏ).

  - Slot Vacant: Tên slot, `-- Vacant --`, Badge `UNASSIGNED` viền đỏ, Nút `+ Assign Employee`.

- Operational Warning Box (Cạnh trái viền đỏ Swiss 4px, nền xám `#F4F3F3`).



#### 4. User Interactions

- Tap `[Unassign Slot]` trên mobile card -> Mở full-screen modal Impact Preview.

- Tap `[Change Approver]` -> Mở modal chọn nhân viên.



#### 5. Business Rules Enforced

- Đảm bảo logic nghiệp vụ nhất quán 100% với bản Desktop.



#### 6. Empty / Warning / Error States

- Cảnh báo viền đỏ Swiss nổi bật ở cuộn trang mobile.



#### 7. Implementation Mapping

- **Route**: `GET /approval-routing/levels/assignments?policyId={id}` (Mobile CSS Viewport trong `LevelAssignments.cshtml`).



#### 8. UAT Checklist

1. Mở giao diện trên thiết bị di động.

2. Kiểm tra các thẻ Level Slot xếp chồng gọn gàng, viền hairline 1px `#D1D1D1` rõ ràng.



---



### Màn Hình 7: Dry-Run Impact Preview & Reassignment Decision (Desktop Modal Overlay)



- **Canvas Screen ID**: `22c0b7c5599a40c6bcdfa43c69857c00`

- **Viewport**: Desktop (1440px Modal Overlay)



#### 1. Purpose

Modal overlay quan trọng nhất hệ thống, hiển thị kết quả **Dry-run Impact Query** khi Admin thực hiện unassign/inactivate một approver hoặc level slot. Giúp Admin xem trước danh sách đơn nghỉ bị ảnh hưởng và lựa chọn chiến lược reroute tự động hoặc chỉ định thủ công trước khi commit database.



#### 2. User Role / Actor

- **HR Admin / System Operator**: Đưa ra quyết định điều hướng lại đơn nghỉ (Reassignment Strategy Decision).



#### 3. Data Displayed

- Modal Box vuông 0px radius, viền 1px đen solid trên nền backdrop mờ 80% đen.

- Header: `DRY-RUN IMPACT PREVIEW & REASSIGNMENT DECISION` | Target: "Unassign Level 1 (Tech Lead Slot) - Tran Van Lead" | Nút đóng `[X]`.

- Summary Metrics (3-column row):

  - Card 1: `Total Impacted Pending Requests`: 3 Requests

  - Card 2: `Automatic Resolver Re-routable`: 2 Requests

  - Card 3: `Route Escalation / Needs Admin Attention`: 1 Request (Highlight con số màu Đỏ Swiss `#E62429`)

- Affected Pending Requests Data Table:

  - `LEAVE REQUEST ID` (font mono, e.g. "LR-2026-089")

  - `EMPLOYEE / REQUESTER` (string, e.g. "Nguyen Van A - Junior Dev")

  - `DATES` (string, e.g. "2026-07-25 - 2026-07-28")

  - `LEAVE TYPE` (string, e.g. "Annual Leave")

  - `CURRENT APPROVER` (string, e.g. "Tran Van Lead (Unassigning)")

  - `PROPOSED NEW APPROVER` (string, e.g. "Le Hoang VP (Level 2 VP Eng Slot)")

  - `RESOLVER STATUS` (Badge: `RESOLVED` đen solid, `NEEDS_ADMIN_ATTENTION` solid Đỏ Swiss `#E62429`)

  - `STRATEGY ACTION` (Nút `[Auto Re-route]` hoặc Dropdown chọn nhân viên duyệt thủ công cho đơn bị nghẽn)

- Decision Strategy Box (Radio Selector):

  - `(o) Strategy 1: Automatic Re-route via Resolver` (Tự động chuyển các đơn hợp lệ sang ứng viên tiếp theo; đơn không giải quyết được sẽ gán trạng thái `NeedsAdminAttention`).

  - `( ) Strategy 2: Manual Reassign All` (Chỉ định thủ công 1 người duyệt duy nhất cho toàn bộ các đơn bị ảnh hưởng).

- Footer Buttons: `[Cancel]` (Nút trắng viền đen) & `[Execute Reassignment & Deactivate Slot]` (Nút đen solid).



#### 4. User Interactions

- **Select Strategy Radio**: Đổi giữa Chiến lược Tự động (Auto Reroute) và Chiến lược Thủ công (Manual Reassign).

- **Select Manual Approver Dropdown**: Nếu có đơn bị `NEEDS_ADMIN_ATTENTION`, Admin có thể chọn trực tiếp một nhân viên active có quyền duyệt trong dropdown.

- **Click `[Execute Reassignment & Deactivate Slot]`**: Gửi AJAX POST thực thi lệnh `UnassignApprovalLevelCommand` hoặc `ReassignPendingLeaveRequestsCommand`, commit transaction nguyên tử (Atomic UnitOfWork Save) và đóng modal.



#### 5. Business Rules Enforced

- **Atomic Transaction Commit**: Cấu hình slot unassign, việc điều hướng lại các đơn pending, và ghi log audit phải diễn ra trong **1 Transaction duy nhất**. Nếu reroute fail -> Rollback toàn bộ, không để lại trạng thái dở dang.

- **Immutable Historical Requests**: Chỉ xử lý các đơn nghỉ ở trạng thái `Pending`. Đơn `Approved`, `Rejected`, `Canceled` **TUYỆT ĐỐI KHÔNG ĐỔI NGƯỜI DUYỆT**.

- **Strict Manual Approver Validation**: Người duyệt thủ công được chọn phải: Active, đã link User, User chưa xóa, có permission `APPROVE_LEAVE_REQUEST`, và không phải là người tạo đơn nghỉ đó.



#### 6. Empty / Warning / Error States

- **0 Impacted Requests**: Hiển thị thông báo "No pending leave requests are impacted by this action. Slot can be safely deactivated."

- **Nghẽn Tuyến Duyệt (`NEEDS_ADMIN_ATTENTION`)**: Hiển thị badge Đỏ Swiss `#E62429` nổi bật kèm cảnh báo yêu cầu Admin chỉ định người duyệt thủ công trước khi bấm Execute.



#### 7. Implementation Mapping

- **Route / Endpoint**: `POST /approval-routing/impact-preview` (AJAX Endpoint trả về Partial View `_ImpactPreviewModal.cshtml`)

- **Execute Endpoint**: `POST /approval-routing/execute-reassignment`

- **Controller Actions**:

  - `ApprovalRoutingController.GetImpactPreview(UnassignLevelRequest request)`

  - `ApprovalRoutingController.ExecuteReassignment(ExecuteReassignmentRequest request)`

- **ViewModel**: `ImpactPreviewModalViewModel`

- **Razor Partial View**: `HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/_ImpactPreviewModal.cshtml`



#### 8. UAT Checklist

1. Click `[Unassign Slot]` tại một slot đang có đơn pending phụ thuộc.

2. Xác nhận modal phủ mờ bật lên với số lượng đơn bị ảnh hưởng chính xác.

3. Kiểm tra các đơn tự động reroute được hiển thị đề xuất approver mới (Level 2).

4. Kiểm tra các đơn không tìm được approver hiển thị badge Đỏ Swiss `NEEDS_ADMIN_ATTENTION`.

5. Bấm `[Execute Reassignment & Deactivate Slot]` -> Xác nhận slot bị ngắt và đơn nghỉ được cập nhật người duyệt mới thành công trong 1 commit.



---



## 4. Tóm Tắt Ma Trận Implementation Razor MVC



| STT | Tên màn hình UI Phase 5 | Route URL | Controller Action | ViewModel | View File Path |

| :---: | :--- | :--- | :--- | :--- | :--- |

| **1** | Approval Routing Policies (Desktop) | `/approval-routing/policies` | `Policies(department, search)` | `PolicyListViewModel` | `/Views/ApprovalRouting/Policies.cshtml` |

| **2** | Approval Routing Policies (Mobile) | `/approval-routing/policies` | `Policies(department, search)` | `PolicyListViewModel` | `/Views/ApprovalRouting/Policies.cshtml` (Responsive) |

| **3** | Policy Detail & Rule Config (Desktop) | `/approval-routing/policies/detail?id={id}` | `PolicyDetail(id)` | `PolicyDetailViewModel` | `/Views/ApprovalRouting/PolicyDetail.cshtml` |

| **4** | Policy Detail & Rule Config (Mobile) | `/approval-routing/policies/detail?id={id}` | `PolicyDetail(id)` | `PolicyDetailViewModel` | `/Views/ApprovalRouting/PolicyDetail.cshtml` (Responsive) |

| **5** | Level Slot Assignments (Desktop) | `/approval-routing/levels/assignments?policyId={id}` | `LevelAssignments(policyId)` | `LevelAssignmentViewModel` | `/Views/ApprovalRouting/LevelAssignments.cshtml` |

| **6** | Level Slot Assignments (Mobile) | `/approval-routing/levels/assignments?policyId={id}` | `LevelAssignments(policyId)` | `LevelAssignmentViewModel` | `/Views/ApprovalRouting/LevelAssignments.cshtml` (Responsive) |

| **7** | Dry-Run Impact Preview Overlay | `POST /approval-routing/impact-preview` | `GetImpactPreview(dto)` | `ImpactPreviewModalViewModel` | `/Views/ApprovalRouting/_ImpactPreviewModal.cshtml` |



---



## 5. Dừng Lại Và Xin Ý Kiến Trình Phê Duyệt (STOP FOR APPROVAL)



> [!IMPORTANT]

> **Anti đã cập nhật toàn bộ tài liệu Quy chuẩn Màn hình UI Phase 5 theo đúng 7 màn hình visible chính thức trên Canvas.**

> Anti **KHÔNG VIẾT CODE C# / RAZOR**, **KHÔNG SỬA DB/AUTH/KEYCLOAK**, **KHÔNG STAGE/COMMIT/PUSH**.

> Anti kính trình User và Codex xem xét phê duyệt tài liệu quy chuẩn này trước khi chuyển sang bước triển khai code C# / Razor.
