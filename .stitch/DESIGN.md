---
name: Swiss International HR
colors:
  surface: '#FAF9F9'
  surface-dim: '#DADADA'
  surface-bright: '#FAF9F9'
  surface-container-lowest: '#FFFFFF'
  surface-container-low: '#F4F3F3'
  surface-container: '#EEEEEE'
  surface-container-high: '#E8E8E8'
  surface-container-highest: '#E2E2E2'
  on-surface: '#1A1C1C'
  on-surface-variant: '#4C4546'
  inverse-surface: '#2F3131'
  inverse-on-surface: '#F1F1F0'
  outline: '#7E7576'
  outline-variant: '#CFC4C5'
  surface-tint: '#5E5E5E'
  primary: '#000000'
  on-primary: '#FFFFFF'
  primary-container: '#1B1B1B'
  on-primary-container: '#848484'
  inverse-primary: '#C6C6C6'
  secondary: '#BB0015'
  on-secondary: '#FFFFFF'
  secondary-container: '#E32127'
  on-secondary-container: '#FFFBFF'
  tertiary: '#000000'
  on-tertiary: '#FFFFFF'
  tertiary-container: '#1A1C1C'
  on-tertiary-container: '#838484'
  error: '#BA1A1A'
  on-error: '#FFFFFF'
  error-container: '#FFDAD6'
  on-error-container: '#93000A'
  background: '#FAF9F9'
  on-background: '#1A1C1C'
  surface-variant: '#E2E2E2'
typography:
  headline-lg:
    fontFamily: Geist
    fontSize: 64px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.04em
  headline-lg-mobile:
    fontFamily: Geist
    fontSize: 40px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Geist
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  headline-sm:
    fontFamily: Geist
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Geist
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Geist
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  label-md:
    fontFamily: Geist
    fontSize: 12px
    fontWeight: '600'
    lineHeight: '1'
    letterSpacing: 0.05em
  label-sm:
    fontFamily: Geist
    fontSize: 10px
    fontWeight: '500'
    lineHeight: '1'
    letterSpacing: 0.1em
spacing:
  unit: 8px
  gutter: 24px
  margin-edge: 48px
  container-max: 1440px
  stack-sm: 16px
  stack-md: 32px
  stack-lg: 64px
rounded:
  DEFAULT: 0px
  sm: 0px
  md: 0px
  lg: 0px
  full: 0px
---

# Design System: Swiss International HR
**Project ID:** 17479353588209716186
**Selected Source Screen:** DP Style 06 - Swiss International
**Selected Stitch Asset:** assets/f4fbeeb3791c4c52991dd52c4fb92635

## 1. Visual Theme & Atmosphere
The application direction is now **Swiss International HR**: objective, modular, typographic, and highly disciplined. The interface should feel like a precise enterprise operating document rather than a decorative SaaS dashboard.

The mood is clean, strict, readable, and institutional. It should use a light canvas, strong black typography, precise grid alignment, and one restrained Swiss red accent for urgent states, active focus, or rejected status. The design should not feel soft, playful, gradient-heavy, or glossy.

## 2. Color Palette & Roles
* **Primary Canvas (#FAF9F9):** Main app background, close to white but slightly warmer than pure white.
* **Pure Surface (#FFFFFF):** Tables, panels, toolbar modules, modal bodies.
* **Black Ink (#000000):** Primary actions, high-priority headers, active text, strong structure.
* **Primary Text (#1A1C1C):** Standard readable text on light backgrounds.
* **Secondary Text (#4C4546):** Metadata, table secondary lines, helper text.
* **Hairline Border (#D1D1D1 / #CFC4C5):** Thin dividers, table grid, input outlines.
* **Swiss Red (#E62429 / #BB0015):** Urgent states, rejected status, critical markers, active focus only.
* **Pale Section Fill (#FBFBFB / #F4F3F3):** Table headers and low-emphasis section backgrounds.

## 3. Typography Rules
Use **Geist** across the system. Typography is the main design tool.

* Page titles are large, bold, tight, and left-aligned.
* Table headers are small uppercase labels with increased letter spacing.
* Body data uses clean regular-weight Geist.
* Numeric values, dates, employee codes, and balances may use tabular figures or monospace treatment when implementation allows.
* Static UI text must be English.

## 4. Component Stylings
* **Buttons:** Squared-off, no pill buttons. Primary buttons are solid black with white text. Secondary buttons are white with black 1px border.
* **Cards/Containers:** Use flat panels with 1px borders. No soft shadows, no gradients, no glassmorphism.
* **Inputs/Forms:** Rectangular 1px border fields. Focus state uses black border or Swiss red only when the action is urgent/invalid.
* **Tables:** Dense spreadsheet-like grid. Thin vertical and horizontal dividers are allowed. Header row uses pale fill and uppercase labels.
* **Status Badges:** Rectangular labels. Rejected uses Swiss red; pending uses outlined/neutral treatment; approved should remain restrained, not bright green-heavy.
* **Sidebar:** Rectilinear navigation. Active state uses black text, border marker, or subtle red line. Avoid rounded active pills.

## 5. Layout Principles
* Strict 12-column modular grid on desktop.
* Strong left alignment and baseline discipline.
* Asymmetric but balanced composition is allowed.
* Use whitespace as a structural separator instead of nested cards.
* Avoid centered dashboard marketing layouts. This is an operational HR app.
* Keep data density high, especially for Employee, Leave Request, Leave Balance, and WorkCalendar screens.

## 6. Anti-Patterns
* No gradients.
* No decorative blobs.
* No heavy shadows.
* No glassmorphism.
* No rounded SaaS pill overload.
* No colorful status confetti.
* No large hero sections for internal screens.
* No Vietnamese static UI text; only database/runtime data may appear as stored.
