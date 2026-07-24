# Implementation Plan — Phase A2C: Dashboard Swiss Alignment & Patch Plan

> **Plan Location**: `MD_memory/plans/2026-07-22_1110_phase-a2c-dashboard-widgets-swiss-purge.md`
> **Phase**: Dashboard Phase A2C (Swiss Visual Compliance & Mismatch Patch)
> **Status**: PENDING USER/CODEX APPROVAL (NO CODE EDITS UNTIL APPROVED)
> **Stitch Source of Truth**:
> - Desktop: `a86bf840299542ccab3243adab896721` (`HRM Dashboard - Swiss Operational Redesign`)
> - Mobile: `a7f62e7b9dd449ac9cdc8adaa3a1d61f` (`HRM Dashboard Mobile - Swiss Analytical Redesign`)

---

## 1. RCA & Mismatch Audit (Runtime vs. Stitch Final)

A line-by-line comparison between runtime `Views/Dashboard/Index.cshtml` + `wwwroot/css/dashboard.css` and the HTML/design from Stitch Desktop (`a86bf840299542ccab3243adab896721`) revealed the following **6 critical mismatches**:

| # | Feature / Widget | Current Runtime Implementation | Stitch Final Design (`a86bf840299542ccab3243adab896721`) | Root Cause / Mismatch Diagnosis |
|---|---|---|---|---|
| 1 | **W2: Status Distribution** | Renders 4 separate count cards (`.hrm-status-card`) with colored left borders (green, amber, red, gray). | Renders a single horizontal stacked bar (Black 65%, Gray 20%, Swiss Red 10%, Light Gray 5%) with a 4-column JetBrains Mono legend below. | Runtime retained an earlier card-based layout proposal instead of adopting the final Swiss stacked bar layout. |
| 2 | **W3: 6-Month Trend** | Renders vertical bar chart columns (`.hrm-trend-bar`). | Renders an SVG trend line chart (`<polyline stroke="#111" stroke-width="1.5">`) with data points (`<circle fill="#111">`) and mono month labels. | Implemented HTML bar chart columns instead of translating Stitch SVG trend line graphics. |
| 3 | **W6: My Leave Balance** | Renders a 4-card grid (`.hrm-balance-item`) with green numbers (`#065F46`) and orange numbers (`#D97706`). | Renders a single horizontal progress bar (`bg-[#F5F5F5]` with `bg-black` fill + white mono text `14.5/20`) and 1 line of mono text metadata below. | Used stat-card-like grid instead of flat hairline progress bar and single line mono stats. |
| 4 | **Empty States** | Contains HTML entity emojis (`&#128203;`, `&#128202;`, `&#128200;`, `&#9989;`, `&#128276;`, `&#128178;`, `&#128197;`). | Minimal dashed/bordered container with text & optional monochrome line icon (e.g. `content_paste_off` in `#D1D1D1`). | Temporary HTML entity emojis were left in Razor templates. |
| 5 | **CSS Palette** | Contains green (`#065F46`, `#ECFDF5`, `#A7F3D0`), amber (`#92400E`, `#FFFBEB`, `#FDE68A`), and orange (`#D97706`). | Strict Swiss Monochrome: Black (`#111111`), White (`#FFFFFF`), Grays (`#E2E2E2`, `#F4F3F3`, `#999999`), and Swiss Red (`#E62429` for errors/rejections ONLY). | Legacy color tokens remained in `dashboard.css` from pre-Swiss iterations. |
| 6 | **Mojibake / Encoding** | Risk of corrupted unicode characters (`â‰¤`, `â€”`). | Pure UTF-8 encoding with standard dashes and symbols. | Requires UTF-8 BOM verification and scan via `scan-mojibake.py --require-bom`. |

---

## 2. Minimal Patch Plan Proposal

### 2.1 CSS Purge & Swiss Token Setup (`wwwroot/css/dashboard.css`)
- **Purge**: Completely remove `#065F46`, `#ECFDF5`, `#A7F3D0`, `#FFFBEB`, `#92400E`, `#FDE68A`, `#D97706`.
- **Badge Classes**:
  - `Approved`: `.hrm-status-approved` -> `background-color: #111111; color: #FFFFFF;`
  - `Pending`: `.hrm-status-pending` -> `background-color: transparent; border: 1px solid #D1D1D1; color: #111111;`
  - `Rejected`: `.hrm-status-rejected` -> `background-color: #E62429; color: #FFFFFF;`
  - `Canceled`: `.hrm-status-canceled` -> `background-color: #F3F4F6; color: #9CA3AF;`

### 2.2 W2 Layout Alignment
- Replace 4 count cards in `Index.cshtml` with:
  ```html
  <div class="h-4 flex border border-standard">
      <div class="h-full bg-black" style="width: @approvedPct%"></div>
      <div class="h-full bg-[#D1D1D1]" style="width: @pendingPct%"></div>
      <div class="h-full bg-[#E62429]" style="width: @rejectedPct%"></div>
      <div class="h-full bg-[#EEEEEE]" style="width: @canceledPct%"></div>
  </div>
  <div class="grid grid-cols-4 gap-2 text-[10px] mono-text font-bold mt-2">
      <div>Approved: @Model.StatusDistribution.ApprovedCount</div>
      <div>Pending: @Model.StatusDistribution.PendingCount</div>
      <div>Rejected: @Model.StatusDistribution.RejectedCount</div>
      <div>Other: @Model.StatusDistribution.CanceledCount</div>
  </div>
  ```

### 2.3 W3 Trend Line Alignment
- Replace vertical bar grid in `Index.cshtml` with SVG line chart:
  ```html
  <div class="h-[120px] border-standard flex items-end justify-between px-4 pb-2 relative overflow-hidden bg-white">
      <svg class="absolute inset-0 w-full h-full" viewBox="0 0 400 120">
          <polyline fill="none" points="@svgPoints" stroke="#111" stroke-width="1.5"></polyline>
          @foreach (var pt in points) {
              <circle cx="@pt.X" cy="@pt.Y" fill="#111" r="2.5"></circle>
          }
      </svg>
      <div class="flex justify-between w-full text-[8px] mono-text text-[#BBB] pt-24 z-10">
          @foreach (var item in Model.MonthlyTrend) { <span>@item.MonthLabel</span> }
      </div>
  </div>
  ```

### 2.4 W6 Balance Alignment
- Replace 4 stat items with single progress bar:
  ```html
  <div class="h-6 w-full bg-[#F5F5F5] border border-standard flex items-center">
      <div class="h-full bg-black flex items-center justify-end pr-2" style="width: @availPct%">
          <span class="text-white text-[10px] mono-text font-bold">@Model.MyLeaveBalance.AvailableDays / @Model.MyLeaveBalance.AllocatedDays</span>
      </div>
  </div>
  <div class="flex justify-between text-[12px] mono-text font-medium text-[#666] mt-2">
      <span>Tổng: @Model.MyLeaveBalance.AllocatedDays</span>
      <span>Đã dùng: @Model.MyLeaveBalance.UsedDays</span>
      <span>Đang chờ: @Model.MyLeaveBalance.PendingDays</span>
      <span class="text-black font-bold">Còn lại: @Model.MyLeaveBalance.AvailableDays</span>
  </div>
  ```

### 2.5 Empty States Purge
- Remove all `&#...` emojis in `Index.cshtml`.
- Replace with clean monochrome icon/text empty state matching Stitch.

### 2.6 Verification Sequence
1. Run `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml --require-bom`.
2. Run `git diff --check`.
3. Run `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj`.

---

## 3. Guardrails Checklist

- ❌ No DB schema/migration/Auth/Keycloak modifications.
- ❌ No DB seed.
- ❌ No hardcoded role names (`ADMIN`, `HR`, `CEO`, `MANAGER`).
- ❌ No stat cards.
- ❌ No green/blue/yellow/orange/emoji in dashboard.
- ❌ No git stage/commit/push.
