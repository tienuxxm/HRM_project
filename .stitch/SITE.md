# Site Specification: HRM Leave Management Admin Application

## 1. Site Vision
An enterprise-grade, high-density dashboard system dedicated to managing employee details, departments, roles, and leave operations. The application is built for administrative speed, clarity, and visual calm.

## 2. Project Information
*   **Stitch Project ID:** `17479353588209716186`
*   **Stitch Project Name:** HRM Leave Management UI Redesign
*   **Design System:** Swiss International HR (Asset ID: `f4fbeeb3791c4c52991dd52c4fb92635`)

## 3. Design Direction
*   **Theme:** Swiss International HR (black/white grid with restrained Swiss red accent)
*   **Font:** Geist for UI, headings, labels, and dense table data.
*   **Layout:** Strict modular grid, hard alignment, high-density data tables, 1px structural borders, no shadows.
*   **Visual Tone:** Objective, typographic, precise, editorial, high-contrast, operational.

## 4. Sitemap & Screens Roadmap
Below is the status of the screens to be designed and generated:

- [x] **Visual Palette Exploration Board** — A canvas showing 8 distinct color palette proposals (Calm Blue, Emerald HR, Teal Operations, Neutral Graphite, Warm Sand, Navy Executive, Slate Cyan, Soft Green) for visual comparison on Stitch.
- [x] **Visual Palette Applied Preview** — A side-by-side dashboard layout demonstrating the 4 strongest color directions (Calm Blue, Teal, Neutral Graphite, Navy Executive) applied to realistic sidebar, header, and data table components.
- [x] **HRM Style Exploration - Leave Request List** — A comparison board demonstrating 6 distinct design variants (Dense Operations, Calm Enterprise, Executive Review, Data Grid Pro, Calendar-Aware Workflow, Minimal Flat) using the selected Slate Cyan Data Console palette.
  - [x] **Style 01 - Dense Operations Console** (Screen ID: `2a58d5d2fafe431a88aa45f04355c024`) — Table-first, compact sidebar, optimized for batch-actions.
  - [x] **Style 02 - Calm Enterprise Admin** (Screen ID: `5645bf4d61414b20954ea1b359b3b43f`) — Approachable UI with summary cards and standard padding.
  - [x] **Style 03 - Executive Review Mode** (Screen ID: `f03c81a256174d23bcc244d11fcd5105`) — Split-pane navigation for quick decision-making.
  - [x] **Style 04 - Data Grid Pro** (Screen ID: `7c83781a6ed1449085da88a36818ea32`) — Ledger-style grid lines and monospace data fields.
  - [x] **Style 05 - Calendar-Aware Workflow** (Screen ID: `315a4d3242104733b09c154c3970f488`) — Embedded team calendar highlighting scheduling conflicts.
  - [x] **Style 06 - Minimal Flat Razor-Friendly** (Screen ID: `890821406a5d4d8c8f1510dc00bbe5ba`) — A low-complexity design designed specifically for ASP.NET Razor/Bootstrap implementation.
- [x] **Login** — HRM Portal - Institutional Login. (Desktop Screen ID: `497e239c2ac34153a0b01eacd61811fb`, Mobile Screen ID: `d8c34d394a984945a151d812e85da88b`)
- [x] **Dashboard** (APPROVED_BY_USER) — Calm, data-dense cockpit showing leave request trends, active leaves, pending counts, and quick employee actions. (Desktop Screen ID: `4d66073a97214a498d874fe732ce0d23`, Mobile Screen ID: `6077befcf61b49878ab66a73f3e17e4a`)
- [x] **Employee List** (APPROVED_BY_USER) — Dense tabular directory showing active status, department, role, and leave balance summary. (Desktop Screen ID: `81667db3ec1649018cd1133168e058e7`, Mobile Screen ID: `54cd65c41e3745edbe9836795f466155`)
- [x] **Employee Create/Edit Modal** (APPROVED_BY_USER) — Structured modal form for adding/editing employee records. (Desktop Screen ID: `d21f17b551c546c79c76b18d70cd713c`, Mobile Screen ID: `e2e8db66ddc74a6f9e8dcb20b35e638b`)
- [x] **Department List** (APPROVED_BY_USER) — Simple list of departments with employee counts and department managers, including standardized pagination. (Desktop Screen ID: `6848518012663931319`, Mobile Screen ID: `9226870575435202778`)
- [ ] **Position List** (GENERATED_FOR_REVIEW) — List of roles/positions mapped to departments, including standardized pagination. (Desktop Screen ID: `9714fc9d811e4b3e829eeeceed9195bd`, Mobile Screen ID: `e7f3855fe06442768c5faec12a4fdfec`)
- [ ] **Leave Type List** — Admin screen for leave configurations (annual, sick, unpaid, etc.).
- [ ] **Leave Balance List** — High-density tracking grid of remaining, allocated, pending, and used leave days for all employees.
- [ ] **Leave Request Create Modal** (GENERATED_FOR_REVIEW) — Strict Swiss-style modal for submitting new leave requests. (Desktop Screen ID: `782896a972f04dfc9f0bcc47f3cb4f82`, Mobile Screen ID: `f59bf6c80c6a474397a74d9d2c3b1d6e`)
- [ ] **Leave Request Cancel Modal** (GENERATED_FOR_REVIEW) — Strict Swiss-style destructive modal for confirming leave request cancellation. (Desktop Screen ID: `4d87aaefa5e544f7bb752f6a640a781d`, Mobile Screen ID: `6ef407fb37ef4ee9b32abcef2add0149`)
- [ ] **Leave Request List** — Directory of all leave applications with quick-action approve/reject controls.
- [ ] **Leave Request Detail** (GENERATED_FOR_REVIEW) — Detailed timeline and context view for specific leave request approval decisions. (Desktop Screen ID: `4028307b15f649c981342540ec3b3508`, Mobile Screen ID: `6d0a4188b1e74029889bc08b5a552b3c`)
- [ ] **Work Calendar Module** (GENERATED_FOR_REVIEW) — Management grid and import workflows for team calendar settings.
  - **Work Calendar List & Overrides** (Desktop Screen ID: `620ee6b4d320478a87b8d81ce4e4d6a8`, Mobile Screen ID: `47a25a50787e45448378c89b7d8ec88b`)
  - **Work Calendar Import Preview** (Desktop Screen ID: `f78119eb8ce845eaae5dae9c9c855a8f`, Mobile Screen ID: `f8bf01ec2ca34b6ea17e4bf648f5a5e3`)
  - **Work Calendar Import Summary** (Desktop Screen ID: `b048dc088e8647dd808a61abe8c8f6d3`, Mobile Screen ID: `4298007a8f994d81ba6bca66c32e4c32`)

## 5. Development Log
*   **2026-07-20:** Generated Work Calendar design screens (List, Preview, Summary) under Swiss International HR for Desktop and Mobile viewports at status GENERATED_FOR_REVIEW.
*   **2026-07-16:** Generated design screens for "Leave Request Detail" under Swiss International HR for Desktop and Mobile viewports at status GENERATED_FOR_REVIEW.
*   **2026-07-16:** Generated design screens for "Leave Request Create Modal" and "Leave Request Cancel Modal" under Swiss International HR for Desktop and Mobile viewports at status GENERATED_FOR_REVIEW.
*   **2026-07-14:** Refined column structure of "Position List" Desktop screen (ID: `9714fc9d811e4b3e829eeeceed9195bd`) to match exact entity database properties (Code, Name, Level), eliminating mismatch columns (Department, Count, Description).
*   **2026-07-14:** Generated mobile screen for "Position List" (Mobile ID: `e7f3855fe06442768c5faec12a4fdfec` - 780px mobile viewport) and registered desktop/mobile screens (Desktop ID: `9714fc9d811e4b3e829eeeceed9195bd`) at status GENERATED_FOR_REVIEW.
*   **2026-07-14:** Uploaded paginated, metrics-free screens for "Department List" (Desktop ID: `6848518012663931319`, Mobile ID: `9226870575435202778` - 780px mobile viewport) to the Stitch canvas at status GENERATED_FOR_REVIEW.
*   **2026-07-13:** Refined "Department List" design screens to correct sidebar menu items, completely remove all stats and metrics, and enforce the Swiss International hierarchy for Desktop (ID: `30b42e914a0a440583b2fc7de9649830`) and Mobile (ID: `0e4dcb1198ce4f9f907baa2bd14682b7`) at status GENERATED_FOR_REVIEW.
*   **2026-07-13:** Generated "Department List" design screens under Swiss International HR (Style 06) for Desktop (ID: `abfe091d49e64736b1358f675b44686c`) and Mobile (ID: `2e33b7e080ff4a6281f8b82a7e5e1d5e`) at status GENERATED_FOR_REVIEW.
*   **2026-07-10:** Initialized Stitch project, registered "Cockpit Operations" design system, and created design metadata tracking files.
*   **2026-07-10:** Generated "HRM Palette Exploration Board" and "HRM Palette Applied Preview" screens to enable direct side-by-side visual evaluation of 8 color schemes on the Stitch canvas.
*   **2026-07-10:** Generated "HRM Style Exploration - Leave Request List" comparison board containing 6 layout styles using the selected "Slate Cyan Data Console" palette.
*   **2026-07-10:** Created 6 distinct, independent visual screens on the Stitch canvas for each individual style (Style 01 through Style 06) for side-by-side evaluation of workflow layouts.
*   **2026-07-10:** User approved `DP Style 06 - Swiss International` as the final visual direction and selected its black/white/Swiss-red palette as the default design system for future HRM screens.
*   **2026-07-10:** Finalized and approved "HRM Portal - Institutional Login" (Desktop: `497e239c2ac34153a0b01eacd61811fb`, Mobile: `d8c34d394a984945a151d812e85da88b`) as the final Login design under the Swiss International theme.
*   **2026-07-10:** Generated "HRM Cockpit / Dashboard" design screens under Swiss International HR (Style 06) for Desktop (ID: `4d66073a97214a498d874fe732ce0d23`) and Mobile (ID: `6077befcf61b49878ab66a73f3e17e4a`).
*   **2026-07-10:** Generated "Employee List" design screens under Swiss International HR (Style 06) for Desktop (ID: `81667db3ec1649018cd1133168e058e7`) and Mobile (ID: `54cd65c41e3745edbe9836795f466155`) at status GENERATED_FOR_REVIEW.
*   **2026-07-10:** Generated "Employee Create/Edit Modal" design screens under Swiss International HR (Style 06) for Desktop (ID: `d21f17b551c546c79c76b18d70cd713c`) and Mobile (ID: `e2e8db66ddc74a6f9e8dcb20b35e638b`) at status GENERATED_FOR_REVIEW.

