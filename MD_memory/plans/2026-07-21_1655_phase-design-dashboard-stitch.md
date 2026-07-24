# Phase: Design Dashboard on Stitch Canvas

> **Created**: 2026-07-21T16:55:00+07:00
> **Status**: ✅ DESIGN COMPLETE — Awaiting User Review
> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain
> **Scope**: Stitch Canvas ONLY — NO runtime code changes

---

## 1. Audit Summary — Legacy Dashboard

### Controller (`DashboardController.cs`)
| Legacy Dependency | Type | Status |
|---|---|---|
| `GetBookingReportCommand` | MediatR Command | ❌ LUC legacy — must replace |
| `GetRevenueCommand` | MediatR Command | ❌ LUC legacy — must replace |
| `RevenueDataRangeType` | Enum | ❌ LUC legacy — must replace |
| `DashboardViewModel` | ViewModel | ⚠️ Contains `BookingReportResponse` + `RevenueResponse` |

### View (`Views/Dashboard/Index.cshtml`)
| Legacy Widget | Lines | Description |
|---|---|---|
| BOOKING KPI Strip | L13-108 | 3 cards: Completed/Processing/Canceled bookings |
| ORDERS Doughnut Chart | L110-122 | Pie chart of order status distribution |
| SALES REVENUE Bar Chart | L123-165 | Revenue by week/month with dropdown |
| Chart.js Scripts | L170-241 | Doughnut + bar chart initialization |

**Verdict**: 100% of the current dashboard is LUC legacy (Bookings/Orders/Revenue). Zero HRM content.

---

## 2. Design System Created

**Name**: `Swiss International HR`
**Asset ID**: `assets/f4fbeeb3791c4c52991dd52c4fb92635`
**Project**: `projects/17479353588209716186` (HRM Leave Management UI Redesign)

### Design Tokens
| Token | Value |
|---|---|
| Primary | `#000000` (Black) |
| Secondary | `#E62429` (Swiss Red — alerts/rejected only) |
| Tertiary | `#FBFBFB` |
| Neutral | `#D1D1D1` |
| Surface | `#FAF9F9` |
| Font | Geist (headlines, body, labels) |
| Border-radius | 0px (strict rectilinear) |
| Borders | 1px solid `#D1D1D1` (subtle) / `#000000` (emphasis) |
| Shadows | None |
| Gradients | None |

### Typography Scale
| Role | Size | Weight | Spacing |
|---|---|---|---|
| headline-lg | 64px | 700 | -0.04em |
| headline-md | 32px | 600 | -0.02em |
| headline-sm | 24px | 600 | -0.01em |
| body-lg | 18px | 400 | 0 |
| body-md | 16px | 400 | 0 |
| label-md | 12px | 600 | 0.05em |
| label-sm | 10px | 500 | 0.1em |

---

## 3. Screens Generated

### Desktop: "HRM Dashboard - Swiss International HR Style"
- **Screen ID**: `d2edff672c384359b5e9c6836aa8691c`
- **Dimensions**: 2560 × 2758 px
- **Device**: DESKTOP

**Layout**:
1. **KPI Strip** (4 columns): Total Employees, Departments, Pending Requests, Upcoming Holidays
2. **Status Distribution + Attention Queue** (60/40 split): Stacked bar chart + pending approval list
3. **Monthly Trend + Calendar** (50/50 split): Line chart + upcoming non-working days
4. **Department Load** (full width): Horizontal bar chart by department

### Mobile: "HRM Dashboard - Mobile Swiss Style"
- **Screen ID**: `9f25bd6f3b1f47acb371123573cb51ad`
- **Dimensions**: 780 × 3270 px
- **Device**: MOBILE

**Layout** (single column):
1. Top app bar with "DASHBOARD" headline
2. KPI Strip (2×2 grid)
3. Status Distribution — stacked bar
4. Pending Approval — compact list
5. Monthly Leave Trend — line chart
6. Upcoming Non-Working Days — list
7. Department Leave Requests — horizontal bars
8. 80px bottom safe space

---

## 4. Data Mapping — Legacy → HRM

| Legacy Widget | HRM Replacement | Data Source (future) |
|---|---|---|
| Booking Completed/Processing/Canceled | Total Employees / Departments / Pending Requests / Upcoming Holidays | `EmployeeService`, `DepartmentService`, `LeaveRequestService`, `WorkCalendarService` |
| Orders Doughnut | Status Distribution (Approved/Pending/Rejected/Canceled) | `LeaveRequestService.GetStatusDistribution()` |
| Sales Revenue Bar Chart | Monthly Leave Trend + Department Load | `LeaveRequestService.GetMonthlyTrend()`, `LeaveRequestService.GetByDepartment()` |
| Revenue Week/Month toggle | N/A (removed — not needed for leave analytics) | — |

---

## 5. Target Audience

Dashboard designed for **Admin/HR users** with `VIEW_DASHBOARD` permission.
- Current controller already redirects users with only `VIEW_LEAVE_REQUEST` to `/leave-request`.
- No employee self-service dashboard designed in this phase.

---

## 6. Stitch Project Reference

| Item | Value |
|---|---|
| Project Name | HRM Leave Management UI Redesign |
| Project ID | `17479353588209716186` |
| Stitch URL | [Open in Stitch](https://stitch.withgoogle.com/projects/17479353588209716186) |
| Design System | Swiss International HR (`assets/f4fbeeb3791c4c52991dd52c4fb92635`) |
| Desktop Screen | `d2edff672c384359b5e9c6836aa8691c` |
| Mobile Screen | `9f25bd6f3b1f47acb371123573cb51ad` |
| Total Screens in Project | 32 (including all prior modules) |

---

## 7. Next Steps (After User Approval)

1. **User reviews** desktop + mobile designs on Stitch canvas
2. **If approved**: Plan `phase-implement-dashboard` to:
   - Create `HrmDashboardViewModel` replacing `DashboardViewModel`
   - Create new Application queries: `GetDashboardStatsQuery`, `GetLeaveStatusDistributionQuery`, `GetMonthlyLeaveTrendQuery`, `GetDepartmentLeaveLoadQuery`
   - Refactor `DashboardController.cs` to call HRM services instead of LUC commands
   - Rewrite `Views/Dashboard/Index.cshtml` using the Swiss International HR tokens
3. **If changes needed**: Edit screens on Stitch with `edit_screens` MCP tool

---

## 8. Guardrails Verified

- [x] NO runtime code modified (C#/Razor/CSS/JS)
- [x] NO DashboardController.cs touched
- [x] NO _Layout.cshtml touched
- [x] NO Auth/Keycloak/Permission changes
- [x] NO git stage/commit/push
- [x] Stitch MCP tools used exclusively (no browser/download workarounds)
- [x] Architecture boundary preserved: Web.Backend → Application → Domain
