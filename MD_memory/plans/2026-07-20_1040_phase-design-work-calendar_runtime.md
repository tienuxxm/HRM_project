# HRM Phase Design Work Calendar Runtime — Plan

## 1. Boundary & Safety Constraints
- **Architectural Boundary:**
  - `Web.Backend -> Application -> Domain`
  - `Infrastructure -> Application/Domain`
  - All modifications are purely frontend visual overrides within Razor views.
- **Allowed Target Files:**
  1. `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Index.cshtml`
  2. `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Preview.cshtml`
  3. `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Summary.cshtml`
- **Strictly Untouched:**
  - No controller, application logic, domain, infrastructure, Keycloak authentication, or permissions changes.
  - No database migration or seeding updates.
  - Preserve all original C# code-blocks, model bindings, script element tags, AJAX urls, form actions, input name/ID fields, and modal Javascript hooks.

## 2. Design Goal: Swiss International HR (Enterprise Calm)
- **Typography:** Geist font for headers/body elements, JetBrains Mono for system codes, dates, and numbers.
- **Borders & Corners:** Sharp 0px border-radius, whisper gray hairline borders (`border-gray-200` or `#E2E8F0`), no heavy box shadows.
- **Colors:** Ultra clean palette. Subdued badges, dark titles (`text-gray-950`), clean table borders, flat action panels.
- **Layout:** High-density, asymmetric, structured grids. No cards-in-cards or excessive bubbly shadows.

## 3. Screen Breakdown & Modifications

### A. Index.cshtml (Work Calendar List & Manual Day Entry)
- **Source of Truth (Stitch):** screen `ddd7fe2b585f4a16b87fba7d6534b718`
- **Actions:**
  - Header: Bold, minimal title. Flat buttons (Add single config, Download Excel Template, Import Excel).
  - Search/Filter section: Left-aligned date selectors and filter controls, structured with fine borders.
  - Table: Thin headers (`text-gray-400 font-medium`), JetBrains Mono for Dates and Type indicators.
  - Modals (Add / Edit / Import): Redesigned as sharp, flat overlays. Keeping form attributes (`method="post"`, `action="/work-calendar/..."`), input IDs, and existing validation messages.

### B. Preview.cshtml (Work Calendar Import Preview)
- **Source of Truth (Stitch):** screen `c17f55287e7b4b44b97367ac4ff763db`
- **Actions:**
  - Audit grid styling: Display row index, date, type, description, and status.
  - Errors high-density visualization: Invalid rows highlighted with fine red border and error text, keeping the tabular structure clean.
  - Navigation: Flat "Apply Import" and "Cancel" buttons.

### C. Summary.cshtml (Work Calendar Import Summary)
- **Source of Truth (Stitch):** screen `b048dc088e8647dd808a61abe8c8f6d3`
- **Actions:**
  - Recalculation Summary card: Rendered as a flat grid with whisper borders, showing Updated Days and Affected Leave Requests.
  - Affected Requests table: Redesigned table with flat status changes (`Status A -> Status B`) and duration differences.
  - Back Button: Restyled to match the Swiss primary flat button.

## 4. Verification & Handoff
- Execute `dotnet build` to ensure Razor syntax remains compile-safe.
- Provide a step-by-step UAT description for manual testing (no auto-UAT by default).
