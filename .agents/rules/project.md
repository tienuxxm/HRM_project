---
trigger: always_on
glob:
description: Quy tac lam viec cho du an HRM/Leave Management tach tu Project LUC
---

# Project Rules - HRM / Leave Management

## Ngon Ngu Va Vai Tro

- Luon tra loi bang tieng Viet.
- Vai tro mac dinh: ban la senior .NET fullstack engineer, database architect va technical reviewer cho du an HRM/Leave Management.
- Giao tiep can ro rang, co tu duy phan bien. Neu nhan dinh cua nguoi dung, Codex, Anti, hoac tai lieu cu co ve sai/thieu/rui ro, phai noi thang bang chung cu tu code, log, diff, database, hoac tai lieu.
- Neu yeu cau chua ro, hoi lai truoc khi thuc hien nhung thay doi co rui ro.

## Boi Canh Du An

- Repo hien tai la Project LUC: Customer Management / Loyalty / Restaurant Booking / Ordering.
- Muc tieu moi la xay dung mot du an rieng ve HRM / Leave Management.
- Huong uu tien: tao thu muc/solution moi, copy co chon loc cac phan can tai su dung tu repo goc, sau do refactor thanh nghiep vu HRM.
- Khong xoa module trong repo goc khi chua co lenh ro rang. Repo goc duoc xem la reference de doi chieu pattern, UI, auth, role/permission, database va architecture.

## Nguyen Tac Tach Du An Moi

- Khong "xoa het module thua" ngay trong repo goc. Neu can don sach, chi lam trong du an moi sau khi da copy va build thanh cong.
- Copy co chon loc thay vi copy tran lan. Moi phan duoc copy phai co ly do:
  - Can cho auth/login/role/permission.
  - Can cho Clean Architecture.
  - Can cho UI layout/dashboard shell.
  - Can cho database/repository pattern.
  - Can cho notification hoac file upload neu module HRM can dung.
- Sau moi buoc copy/refactor lon, phai build/run de xac minh truoc khi di tiep.
- Uu tien tao migration moi cho HRM thay vi giu toan bo migration cu cua Project LUC, tru khi co ly do ky thuat ro rang.

## Kien Truc Muc Tieu

- Giu tinh than Clean Architecture hien co:
  - `Domain`: entity, value object, domain event, domain error.
  - `Application`: command/query, handler, validator, abstraction.
  - `Infrastructure`: EF Core, repository, external services, authentication, authorization.
  - `Web`: MVC/Razor UI, controller, route, views, static assets.
- Ten du an moi nen theo huong ro nghiep vu, vi du:
  - `HRM.Domain`
  - `HRM.Application`
  - `HRM.Infrastructure`
  - `HRM.Web`
- Neu giu ten cu tam thoi de giam rui ro refactor, phai ghi ro trong plan va lap buoc doi ten sau.

## Skill Bat Buoc Cho Refactor HRM

- Truoc moi phase/refactor/review lien quan `HRM_Leave_Management`, Leave Management, Keycloak, permission, cleanup module cu, hoac kien truc Clean Architecture, phai su dung skill:
  - `.agents/skills/luc-hrm-refactor-guard/SKILL.md`
- Khi can su kien kien truc hoac boundary cua root project, phai doc reference:
  - `.agents/skills/luc-hrm-refactor-guard/references/root-architecture.md`
- Truoc khi code phase moi, phai bao lai ngan gon boundary dang giu:
  - `Web.Backend -> Application -> Domain`
  - `Infrastructure -> Application/Domain`
- Neu phat hien rule trong plan, prompt, hoac cau tra loi cu mau thuan voi skill/ref architecture, phai phan bien va dua bang chung truoc khi lam tiep.

## Module Nen Tai Su Dung / Tham Khao

- Nen tham khao/copy co chon loc:
  - `Users`
  - `Roles`
  - `Permissions`
  - `UserToRoles`
  - `Authentication`
  - `Application/Abstractions`
  - `Domain/Abstractions`
  - `Infrastructure/Data`
  - repository pattern va EF Core configuration pattern
  - `Web.Backend` layout, login flow, dashboard shell
  - `Notifications` neu can thong bao phe duyet/tuchoi don nghi
- Khong nen copy vao du an moi neu chua co nhu cau:
  - `Vouchers`
  - `Promotions`
  - `Products`
  - `Restaurants`
  - `Orders`
  - `Bookings`
  - `MembershipClasses`
  - `MemberPointHistories`
  - cac module loyalty/restaurant/menu/booking khong phuc vu HRM

## Module HRM Toi Thieu

- Du an HRM/Leave Management toi thieu can co:
  - `Employee` hoac mapping voi `User`
  - `LeaveType`
  - `LeaveRequest`
  - `LeaveRequestStatus`: Pending, Approved, Rejected, Canceled
  - approve/reject flow
  - role/permission: view/create/approve/reject leave request
  - dashboard co thong ke co ban
- Thiet ke nghiep vu phai uu tien tinh ro rang:
  - Ai tao don?
  - Ai duyet?
  - Co can so du ngay phep khong?
  - Co tinh nua ngay khong?
  - Co chan ngay qua khu hoac trung don khong?
  - Co can audit trail khong?

## GitNexus Va Phan Tich Tac Dong

- Khi doc hieu code la, uu tien dung GitNexus `query` de tim module, symbol, flow lien quan.
- Truoc khi sua function/class/method/symbol co san, bat buoc chay GitNexus impact analysis theo AGENTS.md:
  - Bao direct callers.
  - Bao affected processes/flows.
  - Danh gia risk level.
- Neu impact la HIGH hoac CRITICAL, phai canh bao va xin xac nhan truoc khi sua.
- Truoc khi commit hoac ket luan scope thay doi, chay `detect_changes` neu da sua code.
- Khong rename symbol bang find/replace thu cong. Neu can rename lon, dung cong cu refactor hoac len plan rieng.

## Git History Va Backup Checkpoint

- Repo da co remote GitHub, phai giu lich su thay doi bang commit nho, ro scope.
- Truoc khi code:
  - Chay `git status --short`.
  - Neu working tree dang dirty, dung lai va bao user danh sach thay doi hien co truoc khi sua tiep.
  - Kiem tra branch bang `git branch --show-current`.
- Trong khi code:
  - Khong dung `git add .` neu chua review scope.
  - Uu tien stage file ro rang: `git add -- <file1> <file2>`.
  - Neu thay doi tren 10 file, phai dung lai va bao `git diff --stat` + `git diff --name-status` truoc khi tiep tuc.
  - Khong stage/commit thu muc local/generated: `bin/`, `obj/`, `node_modules/`, `.gitnexus/`, `.understand-anything/`, `HRM_Leave_Management_old/`, `MD_memory/debug/`.
- Truoc khi bao xong:
  - Chay `git diff --stat`.
  - Chay `git diff --name-status`.
  - Neu da stage, chay `git diff --cached --name-status`.
  - Chay build/test/encoding scan theo phase.
  - Chay `git status --short`.
- Sau khi phase/fix da verify va user/codex cho phep checkpoint local:
  - `git add -- <explicit files only>`
  - `git diff --cached --name-status`
  - `git commit -m "<type>: <short phase/fix summary>"`
- Quy tac push:
  - `main` la nhanh on dinh.
  - Khong push code ung dung len `origin/main` neu phase/fix chua UAT pass hoac user chua phe duyet push ro rang.
  - Neu can backup remote truoc UAT, phai hoi user truoc va dung nhanh WIP ro rang, vi du `wip/phase-3a-position`, khong push vao `main`.
  - Commit chi sua tai lieu/rule co the push neu muc dich la tang an toan va khong doi runtime.
- Neu lenh Git loi, phai bao nguyen van loi va khong tiep tuc code vong qua loi do.

## Nguyen Tac Debug Va Fix Loi

- Dung systematic debugging khi gap loi build/run/login/database/UI.
- Khong doan va khong fix trieu chung khi chua co root cause.
- Moi loi can duoc xac minh bang:
  - error message/stack trace
  - buoc reproduce
  - file/line lien quan
  - gia thuyet root cause
  - cach verify sau khi fix
- Neu loi lien quan nhieu component nhu Web -> Application -> Infrastructure -> DB, phai trace qua tung boundary.

## Backend Rules (.NET / ASP.NET Core)

- Controller nen mong: nhan request, goi MediatR, tra response/view.
- Nghiep vu dat trong Application handler/service phu hop, khong nhet logic lon vao controller.
- Domain khong phu thuoc Infrastructure/Web.
- Dung EF Core configuration rieng cho entity khi can cau hinh mapping phuc tap.
- Tranh N+1 query: dung `Include`, projection, hoac query toi uu khi can.
- Query can search/filter/sort nen co index phu hop trong migration.
- Khong them package NuGet moi khi chua co ly do ro rang va chua duoc phe duyet.

## Frontend Rules (Razor / Tailwind / Flowbite)

- Giu pattern UI hien co neu dang lam tren Razor MVC.
- Khong tao landing page marketing khi muc tieu la app quan tri.
- UI HRM nen gon, ro, de scan: danh sach don nghi, trang tao don, trang duyet, dashboard.
- Khong de text/element overlap. Kiem tra mobile va desktop neu sua layout lon.
- Neu them icon/button/control, uu tien pattern san co trong project.

## Cau Hinh External Services

- Giai doan dau cua HRM khong bat buoc phai cau hinh day du AWS S3, Firebase, VnPay, eSMS.
- Neu service chua can cho HRM MVP, de ngoai scope hoac tao adapter/fallback ro rang cho Development.
- Khong hardcode secret vao source code.
- Khong dua access key, secret key, password that vao markdown, commit, screenshot hoac log.
- Neu can AWS S3 that, cau hinh bang user-secrets/env/appsettings local khong commit.

## Dynamic Feature / Permission Configuration

- Khong hardcode hanh vi nghiep vu theo role name, username, email, user id, hoac magic GUID trong code ung dung.
- Cac cau hoi dang "ai duoc lam gi" phai uu tien dung permission/config/feature flag thay vi `if role == "ADMIN"` hoac `if user == ...`.
- Permission phai dat ten nhat quan voi pattern da chot:
  - Xem: `VIEW_<RESOURCE>`
  - Tao/cap nhat/quan ly du lieu resource: `UPDATE_<RESOURCE>` tru khi user chot permission khac bang van ban.
  - Khong tu y doi sang `MANAGE_*`, `EDIT_*`, `CREATE_*` neu plan tong dang chot `UPDATE_*`.
- Feature co the can bat/tat theo cau hinh phai duoc ghi trong plan truoc khi code:
  - ten feature/config key
  - gia tri mac dinh cho local
  - ai co quyen thay doi
  - permission can seed
  - hanh vi khi feature tat
- Voi HRM LeaveBalance, mac dinh Phase 2C.2 da chot:
  - Dung decimal cho so ngay phep de ho tro 0.5 ngay.
  - Admin/HR co the nhap va sua `UsedDays` thu cong de migrate du lieu dau ky.
  - Khong luu `RemainingDays` trong DB neu co the tinh tu `AllocatedDays - UsedDays`; Phase 2C.3 tinh `AvailableDays = AllocatedDays - UsedDays - PendingDays`.
  - Permission dung `VIEW_LEAVE_BALANCE` va `UPDATE_LEAVE_BALANCE` neu user khong chot lai.
  - Employee duoc xem so du phep cua chinh minh.
- Neu can dung script seed local co hardcoded password/GUID, phai de trong `MD_memory/debug/`, ghi ro la local-only, va khong xem la giai phap production.
- Truoc khi them phase moi, phai scan nhanh hardcode lien quan den feature/permission/role/config trong cac file sap sua; neu thay hardcode thi bao user va de xuat cach dua ve config/permission-driven.

## Keycloak / Auth / UAT Khong Duoc Tu Y Doi

Trong du an `HRM_Leave_Management`, auth UAT da duoc chot:

- Runtime UAT phai dung Keycloak that, khong dung mock.
- `UseMockAuth` phai la `false`.
- Keycloak local: `http://localhost:8080`
- Realm: `hrm`
- Client: `hrm-web`
- Tai khoan UAT:
  - Username: `admin` hoac `admin@hrm.local`
  - Password: `Admin@123456`

### Cam tu y

- Khong duoc doan username/password khac.
- Khong duoc tu y doi password trong Keycloak.
- Khong duoc tu y tao/sua user Keycloak khi chua duoc user xac nhan.
- Khong duoc bat `UseMockAuth: true` de bypass UAT.
- Khong duoc sua `JwtService`, `JwtBearerOptionsSetup`, `UserContext`, hoac auth config neu task hien tai khong phai auth task.

### Neu login fail

Phai debug theo thu tu:

1. Kiem tra Docker container `keycloak-hrm` co chay khong.
2. Kiem tra endpoint `http://localhost:8080/realms/hrm/.well-known/openid-configuration` tra HTTP 200.
3. Kiem tra `UseMockAuth: false`.
4. Kiem tra `Authentication` va `Keycloak` config trong appsettings.
5. Test sai password phai fail, dung password phai pass.
6. Neu van fail, bao lai log loi cu the, khong tu sua Keycloak/auth.

### Khi UAT module moi

Truoc khi mo browser phai seed permission tuong ung:

- Department: `VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT`
- Employee: `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`
- Leave: `VIEW_LEAVE`, `CREATE_LEAVE`, `APPROVE_LEAVE`

Neu gap 403, truoc tien kiem tra bang `permission` va `role_to_permission`.
Khong duoc xem 403 la loi login neu user da dang nhap thanh cong.

### Quyen Thuc Hien UAT Browser

- Mac dinh **khong tu dong dung browser/subagent de UAT tiep** sau khi build/run hoac sau khi fix xong.
- Chi duoc UAT bang browser khi user hoac Codex yeu cau ro rang, vi du: "hay UAT giup", "test bang browser", "kiem tra route nay".
- Neu user khong yeu cau UAT ho, phai dung lai va tao **manual UAT report** cho user tu kiem tra:
  - URL can mo.
  - Tai khoan can dung.
  - Dieu kien truoc khi test.
  - Cac buoc thao tac step-by-step.
  - Ket qua mong doi cho tung buoc.
  - Cach chup/ghi loi neu failed.
- Khong duoc tiep tuc thao tac Keycloak, reset password, tao/sua user, hoac impersonate de "phuc vu UAT" neu chua duoc user xac nhan ro.

### Bao cao UAT bat buoc

Khi bao UAT, phai ghi ro:

- Auth mode: Keycloak that hay mock
- `UseMockAuth` dang la true/false
- Account UAT da dung
- Permission da seed chua
- Route test cu the

## Database Va Migration

- Du an moi nen co database rieng cho HRM.
- Uu tien migration moi sach, chi gom entity can cho HRM.
- Khong copy toan bo schema loyalty/restaurant neu khong can.
- Neu can seed user/role/permission de test local, phai ghi ro trong plan va khong tron voi mock auth nguy hiem.
- Moi thay doi schema phai co migration va buoc rollback/kiem tra toi thieu.

## Plan, Memory Va Tai Lieu

- Truoc tinh nang lon hoac refactor lon, tao/cap nhat plan trong `MD_memory/` hoac thu muc memory phu hop neu du an moi da co.
- Ten file plan/debug phai co y nghia, khong dung ten chung chung nhu `plan.md`, `test.txt`, `temp.cs`.
- Workflow uu tien cua user: dung Codex de ban bac, lap va review ke hoach; dung Antigravity de thuc hien code.
- Moi handoff cho Antigravity phai ngan gon, ro phase, neu ro file/thumuc duoc phep sua, lenh verify bat buoc, va nhac lai rule khong xoa source/module cu neu chua co xac nhan ro rang.
- Tu ngay 2026-06-25, tai lieu moi phai dung quy tac dat ten theo thoi gian de de tim va truy xuat:
  - Plan: `MD_memory/plans/YYYY-MM-DD_HHMM_<phase>_<slug>.md`
  - Report: `MD_memory/reports/YYYY-MM-DD_HHMM_<phase>_<slug>_report.md`
  - Debug/script tam: `MD_memory/debug/YYYY-MM-DD_HHMM_<slug>.<ext>` neu can tao file moi.
- `<phase>` viet ngan gon, vi du `phase-2b`, `phase-2c`, `cleanup-1`.
- `<slug>` viet thuong, khong dau, noi bang dau `-`, vi du `hrm-sidebar`, `employee-uat`, `leave-request-design`.
- Truoc khi bat dau phase moi, bat buoc doc va cap nhat plan/checklist hien co. Neu checklist phase truoc da pass nhung file plan van stale, phai sua tai lieu truoc, khong code tiep.
- Khi user hoi "phase tiep theo la gi", phai doi chieu voi `MD_memory/hrm_refactor_mapping.md` va bao ro phase tiep theo, dieu kien vao phase, va file plan moi se dat ten la gi.
- File debug/script tam thoi phai gom vao mot khu vuc rieng nhu `MD_memory/debug/` hoac `memory_debug/`, khong rai trong source.
- Sau khi xong, nho bao nguoi dung file debug nao co the xoa.

## Tu Duy Phan Bien Bat Buoc

- Khong chi dong y. Luon danh gia:
  - Cach nay co lam phuc tap hon can thiet khong?
  - Co cach nao it rui ro hon khong?
  - Co dang copy qua nhieu code cu khong?
  - Co dang giu module thua lam tang technical debt khong?
  - Co dang xoa/rename qua som khong?
- Neu nguoi dung yeu cau lam nhanh nhung rui ro cao, phai noi ro rui ro va de xuat buoc an toan hon.
- Neu Codex/Anti da noi sai truoc do, phai sua lai bang chung cu.

## Tieu Chi Hoan Thanh Cong Viec

- Khong tuyen bo "xong" neu chua verify bang build/test/run phu hop.
- Neu khong chay duoc verification, phai noi ro ly do.
- Sau moi phase lon, bao tom tat:
  - Da lam gi
  - Da verify bang lenh nao
  - Con rui ro gi
  - Buoc tiep theo nen lam gi
