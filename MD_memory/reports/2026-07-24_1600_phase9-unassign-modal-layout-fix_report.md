# Phase 9 - Unassign Level Slot Modal UI Layout Fix Report

**Date**: 2026-07-24  
**Author**: Antigravity AI  
**Scope**: `Web.Backend/Views/ApprovalRouting/_ImpactPreviewModal.cshtml`, `Web.Backend/Views/ApprovalRouting/LevelAssignments.cshtml`  
**Status**: **PASS (Verified via Desktop & Mobile 390x844 Browser UAT)**

---

## 1. Rejection & Remediation History

> [!IMPORTANT]
> **Codex & User Rejection Notice**:
> The initial draft of this report and patch was **REJECTED by Codex and User** due to:
> 1. **Mojibake / Unicode Character Corruption**: Use of special em-dashes that risk character corruption.
> 2. **Swiss Design Palette Violation**: Inclusion of non-Swiss color accents (such as historical green/emerald references) which breaks the project's Swiss monochrome + red design system.
> 3. **Heavy Shadow Violation**: Usage of drop shadows (such as historical shadow-2xl references) which violates Swiss flat border-only styling.
> 4. **Insufficient Responsive Evidence**: Lack of explicit dual-device (Desktop + Mobile 390x844) evidence proving modal responsiveness, header layout stability, and scroll containment.

*Note on Scan Terminology*: These occurrences are historical defect descriptions in the report only, not runtime UI usage.

---

## 2. Issue & Root Cause Analysis (RCA)

### Technical Root Causes
1. **Outer Backdrop & Container Overflow**:
   - `_ImpactPreviewModal.cshtml` previously used an outer backdrop with overflow-y-auto while the inner container used my-8 max-h-[90vh]. This caused double scrollbars and layout breaking on medium/small viewports.
   - The z-index on backdrop was set to z-50, placing it below sticky headers (z-40 / z-[1000]).

2. **JavaScript Handler Scope (innerHTML Dynamic Injection)**:
   - When partial view HTML was fetched via AJAX POST /approval-routing/impact-preview and set via container.innerHTML = html, inline event handlers (onclick="handleBackdropClick(event)", onclick="submitReassignmentExecution()") failed to locate functions because script tags inside innerHTML do not execute globally in browser DOM specs.

3. **Swiss Palette & Styling Non-Compliance**:
   - Previous draft used green (text-emerald-700) and soft drop shadow (shadow-2xl) which violated Swiss design system rules (border-2 border-black, shadow-none, black/white/gray/red only).

---

## 3. Implemented Code Solution

### 1. `Web.Backend/Views/ApprovalRouting/_ImpactPreviewModal.cshtml`
- Refactored overlay backdrop to `fixed inset-0 z-[9999] flex items-center justify-center bg-black/75 p-3 sm:p-4 font-sans`.
- Constrained modal dialog box to `w-full max-w-3xl rounded-none border-2 border-black shadow-none max-h-[85vh] flex flex-col my-auto overflow-hidden` (Swiss flat border style).
- Removed all non-Swiss colors (text-emerald-700) and replaced safe state badge with Swiss black badge (`bg-black text-white text-[10px] font-mono font-bold uppercase`).
- Replaced all Unicode em-dashes with standard ASCII dashes (-) to prevent encoding corruption.
- Added `flex-shrink-0 whitespace-nowrap` to the close button `[ X ]` and `truncate` to header titles to prevent text wrapping on mobile devices.
- Added `min-w-[500px]` to inner affected requests table to support smooth horizontal table scrolling on small viewports without breaking modal width bounds.
- Styled sticky black header (`bg-black text-white px-4 sm:px-5 py-3 flex justify-between items-center flex-shrink-0 border-b border-black`).
- Created an internally scrollable body (`flex-1 overflow-y-auto p-4 sm:p-5 space-y-4 text-[#111111] bg-[#FAF9F9]`).
- Created a sticky footer (`bg-white border-t border-[#D1D1D1] px-4 sm:px-5 py-3 flex flex-row justify-end gap-2.5 flex-shrink-0`) holding `Cancel` and `Execute Reassignment & Deactivate Slot` buttons.
- Updated all inline event bindings to explicit `window.*` references (`window.closeImpactModal()`, `window.handleBackdropClick(event)`, `window.submitReassignmentExecution()`, `window.toggleManualApprover(...)`).

### 2. `Web.Backend/Views/ApprovalRouting/LevelAssignments.cshtml`
- Exposed all modal event handlers (`closeImpactModal`, `handleBackdropClick`, `handleImpactModalKeyDown`, `toggleManualApprover`, `showModalError`, `submitReassignmentExecution`) both globally and attached directly to `window`.
- Added keydown event listener for Escape key (Esc) to cleanly close the modal.
- Ensured backdrop clicks (`id="impactModalBackdrop"`) dismiss the modal instantly without leaving residual DOM nodes.

---

## 4. Verification & UAT Findings

| Test Case / Verification Step | Expected Behavior | Observed Result | Status |
| :--- | :--- | :--- | :---: |
| **TC-UI-1: Navigation Context** | URL remains `/approval-routing/levels/assignments...` when Unassign is clicked | URL did not change; page remained on assignments view | **PASS** |
| **TC-UI-2: Modal Rendering & Centering** | Compact modal overlay appears centered with dark backdrop (`z-[9999]`) | Centered `max-w-3xl` modal box rendered inside `z-[9999]` dark overlay | **PASS** |
| **TC-UI-3: Swiss Design & Palette Compliance** | Flat Swiss border (`border-2 border-black`), no `shadow-2xl`, no `emerald`/`green` | Fully compliant with Swiss monochrome + red palette and flat borders | **PASS** |
| **TC-UI-4: Screen Bounds & Overflow (Desktop)** | Modal stays within viewport (`max-h-[85vh]`), no horizontal page overflow | Modal fit neatly inside viewport without overflowing screen | **PASS** |
| **TC-UI-5: Screen Bounds & Overflow (Mobile 390x844)** | Modal stays within 390x844 viewport, body scrolls internally | Modal fit neatly inside 390x844 viewport with internal scrolling | **PASS** |
| **TC-UI-6: Mobile Footer Action Visibility** | Sticky footer actions (Cancel, Execute Reassignment) clearly visible on scroll | Footer actions visible, unblocked by content, easy to tap | **PASS** |
| **TC-UI-7: Cancel / Close Interaction** | Clicking `Cancel` or `[ X ]` or backdrop closes modal immediately | Modal removed from DOM immediately; returns to active table | **PASS** |

---

## 5. Visual Limitations & Mobile Table Scroll Notes

1. **Inner Table Horizontal Scroll on Mobile**:
   - On 390px mobile viewports, the Affected Requests table includes 6 columns (Code, Requester, Type, Dates, Proposed Approver, Status).
   - To preserve table readability, an inner horizontal scroll container (`overflow-x-auto min-w-[500px]`) is used.
   - The modal overlay itself remains strictly fixed within 390px width and does not cause page-level horizontal overflow.

---

## 6. Evidence Artifacts

- **Desktop Open Preview**: `MD_memory/evidence/2026-07-24_phase9-unassign-modal/TC7_desktop_unassign_modal_open_pass.png`
- **Desktop Closed State**: `MD_memory/evidence/2026-07-24_phase9-unassign-modal/TC7_desktop_unassign_modal_closed_pass.png`
- **Mobile 390x844 Open Header & Body**: `MD_memory/evidence/2026-07-24_phase9-unassign-modal/TC7_mobile_unassign_modal_open_pass.png`
- **Mobile 390x844 Open Footer Actions**: `MD_memory/evidence/2026-07-24_phase9-unassign-modal/TC7_mobile_unassign_modal_footer_pass.png`

---

## 7. Verification Commands Run

1. `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore` (Build Succeeded, 0 Errors)
2. Runtime UI Ripgrep Scan: `rg -n "emerald|green|blue|yellow|amber|shadow-2xl" HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/_ImpactPreviewModal.cshtml HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/LevelAssignments.cshtml` -> **0 matches (100% Clean)**
3. Report Mojibake Scan: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/reports/2026-07-24_1600_phase9-unassign-modal-layout-fix_report.md --require-bom` -> **BOM OK, 0 Mojibake**
4. Dual-device Browser UAT (Desktop + Mobile 390x844) on `http://localhost:5300/approval-routing/levels/assignments?policyId=f756fb72-8277-4934-8a71-be724b2e83bc`
