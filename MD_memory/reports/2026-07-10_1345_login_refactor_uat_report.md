# Manual UAT Report: Login UI Refactor to Swiss International Style

* **Date:** 2026-07-10
* **Phase:** Login UI Refactor
* **Visual Direction:** Swiss International HR (DP Style 06)
* **Auth System:** Keycloak Real Auth (`UseMockAuth: false`)
* **UAT Account:** `admin` / `Admin@123456`

---

## Pre-requisites & Verification Conditions

1. **Keycloak Service:** Ensure the Docker container `keycloak-hrm` is running.
   * Verify by opening `http://localhost:8080/realms/hrm/.well-known/openid-configuration` in your browser (should return HTTP 200 JSON content).
2. **App Running:** The backend application is running.
3. **Database Connectivity:** Local SQL Server / database is active.

---

## Step-by-Step Test Procedure

### TC-01: Visual Layout & Style Verification
* **Steps:**
  1. Open a browser and navigate to the Login URL (e.g., `http://localhost:5000/` or your local dev port).
  2. Review the layout against the approved **Swiss International HR** tokens.
* **Expected Result:**
  * Background canvas is light off-white (`#FAF9F9`).
  * Card is centered, white (`#FFFFFF`), with `0px` border-radius (sharp square corners) and a hairline border (`#D1D1D1`).
  * A `2px` horizontal Swiss Red (`#E62429`) accent line is visible at the very top edge of the card.
  * Typography matches Geist & JetBrains Mono fonts.
  * Large, bold uppercase title `HRM PORTAL` in solid black.
  * Uppercase subtitle `SECURE LOGON COCKPIT` in muted gray.
  * Inputs and submit button are completely flat, with `0px` border-radius (no rounded corners, no gradients, no heavy drop shadows).
  * Footers at the bottom of the card display confidential security policy, Keycloak realm connection status, and copyright.

### TC-02: Client-side Validation (Empty Fields)
* **Steps:**
  1. Leave both "Username" and "Password" fields empty.
  2. Click the black **SIGN IN** button.
* **Expected Result:**
  * No request is sent to the server.
  * Username and Password input fields highlight with red borders (`#BB0015`).
  * Clear validation messages appear below the inputs: *"Please input username"* and *"Please input password"*.

### TC-03: Invalid Credentials Check
* **Steps:**
  1. Enter an incorrect username (e.g., `invalid_user`) and password (e.g., `wrong_pass`).
  2. Click the black **SIGN IN** button.
* **Expected Result:**
  * The login button state changes briefly showing spinner/signing in status.
  * Upon receiving the authentication failure, the input fields highlight with red borders.
  * Error message *"Incorrect Username or Password"* is displayed.
  * A failure toast loads at the top right of the page stating *"Login Failed"*.

### TC-04: Successful Authentication & Redirect
* **Steps:**
  1. Enter the correct UAT credentials:
     * **Username:** `admin` (or `admin@hrm.local`)
     * **Password:** `Admin@123456`
  2. Click the black **SIGN IN** button.
* **Expected Result:**
  * The button turns disabled, showing a spinner and *"Signing in..."* label.
  * A success toast loads at the top right stating *"Login Successful"*.
  * The full-page loading overlay is shown.
  * After a 1.5s delay, the page redirects successfully to `/dashboard`.

---

## Troubleshooting & Failure Logging
If any test step fails, please collect the following information:
1. **Browser Console logs (F12):** Look for failed AJAX POST requests to `/auth/login` or CORS/network issues with `http://localhost:8080`.
2. **Docker Keycloak Status:** Run `docker ps` in terminal and confirm `keycloak-hrm` status is `Up`.
3. **Application Logs:** Capture any exceptions thrown in the dotnet terminal window.
