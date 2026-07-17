# Audit & Design Proposal: Leave Request List UI Refactor & Pagination

- **Created**: 2026-07-16 14:00
- **Scope type**: Audit and design proposal only (no code edits in this phase)
- **Target route**: `/leave-request`
- **Reference plan**: `MD_memory/plans/2026-07-16_1400_phase-design-leave-request-list_plan.md`

---

## 1. Technical Boundary & Guardrails

We strictly preserve the architecture boundary:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

This proposal describes frontend-only updates to View templates (`Index.cshtml` and modal partials). It does not edit backend C# logic, authorization handlers, Keycloak setups, database schemas, or query handlers. 

---

## 2. Pre-Refactor Audit of Views

We audited the existing files in `Web.Backend/Views/LeaveRequest/`.

### 2.1 Index View (`Index.cshtml`)
- **Current Model**: `@model List<Application.LeaveRequests.Get.LeaveRequestResponse>`
  - *Gap*: The controller has been updated to return `PagedList<LeaveRequestResponse>`. Keeping the current model declaration will cause a runtime exception (`InvalidOperationException: The model item passed into the ViewDataDictionary is of type...`).
  - *Fix*: Update the model to `@model Domain.Abstractions.PagedList<Application.LeaveRequests.Get.LeaveRequestResponse>` and change iterations from `Model` to `Model.Data`.
- **Existing Filter Form**:
  - GET form directed to `/leave-request`.
  - Filter fields: `employeeId` (conditional under `canApprove`), `leaveTypeId`, and `status`.
  - Focus and styling are native browser/Flowbite defaults with rounded borders.
- **Data Table**:
  - `No` (sequential index calculated locally via `@foreach (var lr in Model.Select(...))`).
  - `Employee` (Name and EmployeeCode).
  - `Leave Type`.
  - `Period` (Formatted as `dd/MM/yyyy (DayPart)` for start and end).
  - `Duration` (Formatted via `Duration.ToString("G29")` plus "Days").
  - `Reason` (Truncated text with hover tooltip via `title`).
  - `Status` (Badges for Pending, Approved, Rejected, Canceled).
  - `Process Info` (Details on who processed the request, when, and any comment).
  - `Actions` (Details link, Cancel button for Pending requests by employee).
- **JavaScript & Event Handlers**:
  - `DOMContentLoaded` listener setting up AJAX submit on form `#createLeaveRequestForm` (POST `/leave-request/create`). Reloads page on success.
  - Global JS function `cancelLeaveRequest(id)` that constructs FormData containing the request ID and anti-forgery token, performing AJAX POST to `/leave-request/cancel`. Reloads page on success.

### 2.2 Create Modal (`_CreateLeaveRequestPartial.cshtml`)
- **Modal ID**: `createLeaveRequestModal`
- **Trigger Button**: `data-modal-target="createLeaveRequestModal" data-modal-toggle="createLeaveRequestModal"`
- **Form ID**: `createLeaveRequestForm`
- **Fields & Model Binding**:
  - `LeaveTypeId` (Dropdown populated from `ViewBag.LeaveTypes`).
  - `StartDate` & `EndDate` (Date inputs).
  - `StartDayPart` & `EndDayPart` (Dropdown selections for Full Day, Morning, Afternoon).
  - `Reason` (Required text area).
- **Buttons**:
  - Close button in header: `data-modal-hide="createLeaveRequestModal"`.
  - Cancel button in footer: `data-modal-hide="createLeaveRequestModal"`.
  - Submit button in footer: `type="submit"`.

### 2.3 Confirm Cancel Modal (`_ConfirmCancelPartial.cshtml`)
- **Model**: `Application.LeaveRequests.Get.LeaveRequestResponse`
- **Modal ID**: `confirmCancel-@Model.Id` (Unique per table row).
- **Body Content**: Dynamic details of the request (Leave Type, duration, dates) to confirm cancellation.
- **Buttons**:
  - Cancel button: `data-modal-hide="confirmCancel-@Model.Id"`.
  - Confirm button: `onclick="cancelLeaveRequest('@Model.Id')"` (Triggers global JS cancel action).

---

## 3. UI Refactor Proposal: Swiss International HR Style

We propose aligning these views with the approved Swiss International HR style tokens.

### 3.1 Page Layout & Filter Area
- Use the **Geist** font framework with square corners (`rounded-none`).
- Title: `LEAVE REQUESTS` in bold uppercase text.
- Filter bar:
  - Convert filters into a structured row with sharp hairline borders (`border-[#D1D1D1]`).
  - Use uppercase monospaced labels for select boxes.
  - Apply clean borders on focus.
  - "Request Leave" button: Styled in solid black (`bg-black text-white hover:bg-[#333] rounded-none uppercase font-mono text-[11px] tracking-wider px-4 py-2`).

### 3.2 Desktop Data Table
- **Headers**: Styled with `#F5F5F5` background, `border-b border-[#D1D1D1]`, zero rounded corners. Text must be bold, black, uppercase, `text-[11px] font-mono`.
- **Row Styling**: Alternating or light hairline borders. Hover state: `#F9F9F9`.
- **Badges**:
  - `Pending`: Flat yellow border, dark yellow text, light yellow background (`bg-[#FEF7E0] border border-[#E3B000] text-[#8A6D00] uppercase font-mono text-[10px]`).
  - `Approved`: Flat green border, dark green text (`bg-[#EAF6EC] border border-[#2B8A3E] text-[#1B6127] uppercase font-mono text-[10px]`).
  - `Rejected`: Flat red border (`bg-[#FDF2F2] border border-[#E02424] text-[#9B1C1C] uppercase font-mono text-[10px]`).
  - `Canceled`: Flat gray border (`bg-[#F3F4F6] border border-[#9CA3AF] text-[#4B5563] uppercase font-mono text-[10px]`).
- **Links**: Flat underline (`underline decoration-[#cfc4c5] underline-offset-4`).

### 3.3 Mobile Layout (Stacked Card List)
- Completely hide the desktop table on viewport `< lg`.
- Each request rendered as an independent card:
  - **Border**: Hairline `border border-[#E5E5E5] rounded-none p-4 mb-3 bg-white`.
  - **Header**: Flex container displaying the Employee Name & Code on the left, and the Status Badge on the right.
  - **Body**: Grid showing Leave Type, Duration (large bold number for emphasis), Date Range, and Reason.
  - **Footer**: Actions group left-aligned (`Details`, `Cancel` buttons/links).
- **Clearance**: Add bottom padding (`pb-24`) to ensure vertical scrolling clears the mobile bottom navigation bar.

### 3.4 Pagination Footer
- Replicate the approved Swiss pagination layout:
  - Desktop: Flex container (`flex justify-between items-center py-4 border-t border-[#E5E5E5]`).
    - Left side: Monospaced summary text, e.g., `SHOWING 1 TO 5 OF 12 ENTRIES`.
    - Right side: Pagination button group.
  - Mobile: Stacked layout with centered pagination controls.
  - **Buttons (PREV / NEXT)**:
    - Normal state: Black text, white background, `border border-[#D1D1D1]`, font-mono, text-[11px] uppercase.
    - Hover state: Black background, white text (`hover:bg-black hover:text-white transition-colors`).
    - Disabled state: Light gray text, no pointer events, `border border-[#E5E5E5] opacity-50 cursor-not-allowed`.
  - **Page indicator**: `PAGE X OF Y` centered, uppercase, monospaced text.

### 3.5 Modal Shell Refactoring (Create & Cancel)
- Apply the **Swiss Modal Frame**:
  - Modal wrapper: Centered, dark dim backdrop (`bg-black/60`).
  - Modal box: Square corners (`rounded-none`), hairline border.
  - Header: Solid black background (`bg-black px-4 py-3`), white uppercase text (`text-white font-mono text-sm tracking-wide`).
  - Close button: Top-right aligned red block (`bg-[#E62429] hover:bg-[#B81D22] text-white p-2 h-full flex items-center justify-center rounded-none`).
  - Footer Action Group:
    - Cancel button: Left side of the action group, gray border (`border border-[#D1D1D1] text-black hover:bg-gray-100 rounded-none uppercase font-mono text-[11px] px-4 py-2`).
    - Confirm / Submit button: Right side of the action group, solid black/dark red (`bg-black text-white hover:bg-gray-800 rounded-none uppercase font-mono text-[11px] px-4 py-2`).
    - *Constraint*: Remove all extraneous tags like `SYSTEM: LIVE`.

---

## 4. Technical Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Model mismatch on load | We will update the model type declaration at line 1 of `Index.cshtml` to `Domain.Abstractions.PagedList<...>` and use `Model.Data` for iterations. |
| Broken JavaScript triggers after styling change | We will keep all existing IDs (`createLeaveRequestForm`, `createLeaveRequestModal`, `confirmCancel-@Model.Id`) and event handlers completely intact. |
| Button hover text visibility | We will define explicit Tailwind hover color transitions (`bg-white text-black border-black hover:bg-black hover:text-white`) to ensure text remains 100% readable. |

---

## 5. Next Steps

1. **Step 1**: Settle design proposal with User.
2. **Step 2**: Apply Swiss refactoring to `Index.cshtml`.
3. **Step 3**: Apply Swiss refactoring to `_CreateLeaveRequestPartial.cshtml`.
4. **Step 4**: Apply Swiss refactoring to `_ConfirmCancelPartial.cshtml`.
5. **Step 5**: Run build and verify UI consistency.
