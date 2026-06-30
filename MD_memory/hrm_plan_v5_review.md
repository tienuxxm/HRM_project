# Review Plan v5: HRM Mini Baseline — Huong A Co Kiem Soat

> Ngay review: 2026-06-23
> Tac gia: Anti
> File review: MD_memory/hrm_leave_management_tach_du_an_plan.md (v5)
> Quyet dinh: Huong A — giu HRM hien tai, bo sung UI shell/assets tu project goc

---

## 0. File Da Sua Trong Phien Nay

| File | Ai sua | Ghi chu |
|---|---|---|
| `Employee.cs` | **USER** | Diff header: "changes were made by the USER" |
| `EmployeeToRoleId.cs` | **USER** | Diff header: "changes were made by the USER" |
| `hrm_leave_management_tach_du_an_plan.md` | **USER** | User cap nhat plan v3 -> v5 |
| `MD_memory/gap_analysis.md` | **Anti** | Sua theo yeu cau fix mau thuan POST/GET logout |
| `MD_memory/hrm_plan_v5_review.md` | **Anti** | Tao moi + cap nhat theo yeu cau review |

Anti KHONG sua bat ky file `.cs` nao.

---

## 1. Diem Dung Cua Plan v5

| # | Noi dung | Danh gia |
|---|---|---|
| 1 | Mini baseline truoc, refactor tung lop sau | Dung |
| 2 | Khong copy module nha hang/loyalty | Dung |
| 3 | Khong copy migration cu | Dung |
| 4 | Database rieng `hrm_db` | Dung |
| 5 | Verify build/run/login/browser truoc khi bao xong | Dung |
| 6 | Phase phan tang ro rang | Dung |

---

## 2. HRM Hien Tai So Voi Plan v5

Plan v5 noi "giu hanh vi gan goc nhat". HRM hien tai da refactor som o mot so cho.

| Phan | Plan v5 muon | HRM hien tai | Lech? | Chac chan? | Bang chung |
|---|---|---|---|---|---|
| Entity | Giu User truoc | Da doi sang Employee | Lech | Chac chan | File `HRM.Domain/Employees/Employee.cs` |
| Namespace | Gan goc | Da doi tat ca sang HRM.* | Lech | Chac chan | `.csproj` va code |
| Auth | Copy pattern goc | Da viet local JWT signing HMAC-SHA256 | Lech | Chac chan | `HRM.Infrastructure/Authentication/JwtService.cs` 57 dong vs goc 218 dong |
| Controller | Copy Login/Auth goc | Da viet moi AuthController (route/flow khac goc) | Lech | Chac chan | HRM: `POST /Auth/Login` tra redirect. Goc: `POST /auth/login` tra Ok() |
| Login View | Copy LoginScreen goc | Raw HTML 23 dong | Lech | Chac chan | `Views/Auth/Login.cshtml` vs goc 172 dong |
| Layout | Copy layout goc | Default .NET 27 dong | Lech | Chac chan | `Views/Shared/_Layout.cshtml` |
| wwwroot | Copy assets goc | Chi co default assets | Lech | Chac chan | wwwroot/css chi co 1 file 362B |
| Cookie | Chua chot | `jwt_token` (goc dung `X-Access-Token`) | Lech | Chac chan | 4 cho dung `jwt_token` trong HRM |
| Database | Rieng `hrm_db` | Da co + migration InitialCreate | Dung | Chac chan | Thu muc Migrations/ |

**Ket luan:** HRM da refactor som nhung nhieu phan dung huong. Ta **chap nhan giu lai** de tiet kiem cong, va bo sung UI tu goc.

---

## 3. Tu Phan Bien Lai 2 Nhan Dinh Cu

### 3.1 EmployeeToRoleId mat IEntityId

**Nhan dinh cu:** "Mat IEntityId co the compile error."
**Sua lai:** **SAI.** Da verify:
- `Entity<TEntityId>` KHONG co constraint `where TEntityId : IEntityId` (file Entity.cs dong 3)
- Grep `IEntityId` trong toan bo HRM va Domain goc: 0 ket qua
- Xoa IEntityId khoi EmployeeToRoleId **khong gay compile error**
- **Can verify cuoi cung bang `dotnet build`**

### 3.2 Keycloak la blocker bat buoc

**Nhan dinh cu:** "Keycloak la blocker bat buoc Phase 1."
**Sua lai:** **Qua voi.** Code goc `JwtService.cs` dong 41-44 co fallback:
```csharp
if (string.IsNullOrEmpty(_keycloakOptions.TokenUrl))
    return GenerateMockToken(email);
```
Goc tu tao mock token khi khong co Keycloak URL.

**Nhung voi Huong A, diem nay khong con quan trong:** HRM hien tai da dung local JWT va da tung login thanh cong. Khong can quay lai thir copy auth goc.

---

## 4. Quyet Dinh Huong A Co Kiem Soat

### Giu lai:
- Employee entity, namespace HRM.*
- Local JWT (JwtService HMAC-SHA256)
- Cookie `jwt_token`
- Database `hrm_db` + migration + seed admin
- AuthController hien tai

### Bo sung:
- UI shell/layout tu goc (doi menu sang HRM)
- Login screen style tu goc (doi route cho HRM)
- Dashboard don gian giu style goc (KHONG copy DashboardController goc vi keo Booking/Order)
- Static assets: CSS, JS
- Toast partial + MainController + ViewModel neu can

### KHONG lam:
- Khong mo nhanh raw baseline
- Khong copy auth goc
- Khong copy module nha hang
- Khong doi lai Employee thanh User
- Khong thu nghiem Keycloak

---

## 5. Thieu Sot Trong Plan v5 Can Bo Sung

### 5.1 LoginScreen phu thuoc chua liet ke (CHAC CHAN)

LoginScreen goc dong 153-164 goi toast partial qua MainController:
- `Web.Backend/Controllers/MainController.cs` — CHUA co trong plan muc 7
- `Web.Backend/Views/Shared/_Toast.cshtml` (5KB) — CHUA co
- `Web.Backend/Models/ToastViewModel.cs` — CHUA co
- `Web.Backend/Models/LoginViewModel.cs` — CHUA co

**Phan bien:** Voi Huong A, AuthController HRM dang dung form POST tra redirect, khong dung AJAX. Neu giu flow nay, LoginScreen goc (dung AJAX) se khong tuong thich truc tiep. Co 2 lua chon:
- (a) Doi AuthController.Login(POST) sang tra Ok/BadRequest cho AJAX — phai sua C#
- (b) Doi LoginScreen.cshtml sang form POST — chi sua HTML/JS, khong can MainController/toast

Chon (a) hay (b) anh huong danh sach file can copy.

### 5.2 Views/Shared co file chua liet ke (CHAC CHAN)

- `Views/Shared/style.css` (229KB) — FILE CSS RIENG, khac wwwroot. CHUA co trong plan
- `Views/Shared/_Toast.cshtml` (5KB)
- `Views/Shared/_Pagination.cshtml` (2.4KB)
- `Views/Shared/_Layout.cshtml.css` (924B)

### 5.3 DashboardController goc khong copy nguyen duoc (CHAC CHAN)

Goc inject `GetBookingReportCommand` va `GetRevenueCommand` — keo Booking/Order module.
Phai viet DashboardController don gian cho HRM.

### 5.4 _Layout goc co menu nha hang (CHAC CHAN)

Menu JS trong _Layout chua Dashboard/Booking/Voucher/Member/Promotion.
Phai doi menuItems sang HRM ngay khi copy.

---

## 6. Rui Ro Can User Xac Nhan

| Rui ro | Chi tiet | Can user lam gi |
|---|---|---|
| Employee.cs bi vo format | Diff cho thay code bi nen thanh 1 dong. Co the file tren disk khac. | Xac nhan: cho Anti chay `view_file` de kiem tra, hoac user tu kiem tra |
| Login flow khong tuong thich | LoginScreen goc dung AJAX, HRM AuthController tra redirect | Chon (a) doi backend AJAX hay (b) doi LoginScreen sang form POST |
| _Layout menu phai doi ngay | Khong the giu menu nha hang du la baseline | User xac nhan menu HRM 8 items da chot truoc do |

---

## 7. Ke Hoach Hanh Dong — Buoc Tiep Theo

**CHUA SUA CODE. Chi la ke hoach cho user duyet.**

### Buoc 0: Verify trang thai hien tai
- Kiem tra Employee.cs tren disk (view_file)
- Chay `dotnet build` de biet co compile error khong
- **Can user xac nhan truoc khi chay**

### Buoc 1: Copy assets tinh (risk THAP)
| File goc | File HRM | Hanh dong |
|---|---|---|
| `wwwroot/css/styles.css` (64KB) | Tao moi | Copy |
| `wwwroot/css/sumoselect.css` (9.7KB) | Tao moi | Copy |
| `wwwroot/js/jquery.sumoselect.js` (31KB) | Tao moi | Copy |
| `wwwroot/css/site.css` (2KB) | Overwrite default | Copy |
| `wwwroot/js/site.js` (6.8KB) | Overwrite default | Copy |
| `Views/Shared/style.css` (229KB) | Tao moi | Copy |
| `Views/Shared/_Layout.cshtml.css` (924B) | Tao moi | Copy |

### Buoc 2: Copy + refactor Views (risk TRUNG BINH)
| File goc | File HRM | Thay doi |
|---|---|---|
| `Views/Shared/_Layout.cshtml` (41KB) | Overwrite | Doi menuItems JS sang HRM. Doi logout `<a>` sang `<form POST>`. Giu/doi route `/Auth/GetUserLogin` hoac inject username khac. Doi cookie name references tu `X-Access-Token` sang `jwt_token` |
| `Views/Shared/_Toast.cshtml` (5KB) | Tao moi | Copy nguyen neu chon huong (a) AJAX login |

### Buoc 3: Login screen (risk TRUNG BINH — phu thuoc quyet dinh a/b)

**Neu chon (a) AJAX:**
| File | Hanh dong |
|---|---|
| Copy `LoginScreen.cshtml` goc | Doi AJAX URL tu `/auth/login` sang `/Auth/Login`. Doi title/text |
| Copy `MainController.cs` | Copy, doi namespace |
| Copy `LoginViewModel.cs` | Copy, doi namespace |
| Copy `ToastViewModel.cs` | Copy, doi namespace |
| Sua `AuthController.Login(POST)` | Doi tu tra redirect sang tra Ok/BadRequest |

**Neu chon (b) Form POST:**
| File | Hanh dong |
|---|---|
| Copy `LoginScreen.cshtml` goc | Doi AJAX thanh form POST. Bo toast partial. Giu style HTML |
| KHONG can MainController | Khong copy |
| KHONG can ToastViewModel | Khong copy |
| KHONG sua AuthController | Giu nguyen |

### Buoc 4: Dashboard (risk THAP)
- Viet `DashboardController` don gian (chi tra View)
- Hoac doi HomeController thanh redirect `/Dashboard`
- Tao `Views/Dashboard/Index.cshtml` voi placeholder cards style giong goc
- Dung Chart.js CDN voi data cung

### Buoc 5: Build + Run + Verify
- `dotnet build`
- `dotnet run`
- Mo browser: login -> dashboard -> click menu
- Chup anh man hinh
- Bao loi console/network neu co

### Thu Tu Verify
1. Build pass
2. App start khong crash
3. Trang login co style giong goc
4. Login thanh cong voi admin/Admin@123456
5. Redirect ve dashboard
6. Dashboard co layout/sidebar/header giong goc
7. Menu sidebar hien thi 8 items HRM
8. Click tung menu — chua can trang con, chi can khong crash

---

## 8. Cau Hoi Can User Xac Nhan Truoc Khi Lam

1. **Employee.cs:** Cho Anti chay `view_file` + `dotnet build` de kiem tra, hay user tu fix?

2. **Login flow:** Chon (a) AJAX hay (b) Form POST?
   - (a) copy nhieu file hon, doi AuthController, nhung giong goc hon
   - (b) it file hon, khong doi AuthController, nhung mat spinner/toast

3. **Menu sidebar:** Xac nhan giu 8 items HRM da chot:
   Dashboard, Employees, Departments, Leave Requests, Leave Types, Approvals, Reports, Roles/Permissions?

4. **Dashboard:** Placeholder cards voi data cung (Pending: 5, Approved: 12, Rejected: 2, Remaining: 15) + bar chart don gian, hay chi card khong chart?

5. **Thu tu lam:** Buoc 0 (verify build) truoc, roi Buoc 1-5 theo thu tu, hay user muon doi thu tu?
