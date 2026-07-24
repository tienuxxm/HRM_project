# Runbook UAT De Hieu - Dynamic Approval Routing

Ngay lap: 2026-07-24

Muc tieu cua file nay: huong dan User mo trinh duyet, login dung tai khoan, bam dung nut, dien dung thong tin de UAT. File nay khong thay the ma tran nghiep vu chi tiet; no la ban thao tac nhanh.

Ranh gioi kien truc dang bao toan:

- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

Mat khau chung cho tat ca tai khoan UAT: `Admin@123456`

Base URL: `http://localhost:5300`

---

## 0. Dieu Can Nho Truoc Khi UAT

1. UAT bang browser thi dung Keycloak that, khong dung mock auth.
2. Khong can vao Keycloak.
3. Khong can dung SQL.
4. Neu tao cau hinh sai, dung lai bao Codex/Anti, khong tu sua DB tay.
5. Cac case co the UAT ngay bang UI hien tai:
   - Case A: Employee IT gui don -> Manager IT duyet.
   - Case B: Rule co 2 candidate, cap 1 trong/khong hop le -> leo len cap 2 theo cau hinh.
   - Case E: Test dynamic nguoc, Manager gui don -> Employee duyet.
   - Case F: Manager IT gui don -> CEO duyet.
   - TC6: Man hinh legacy read-only.
6. Cac case chua UAT day du bang UI hien tai:
   - Case D: Specific Approver Override. UI hien chua co field chon `specific_approver_employee_id` khi them rule.
   - Case G: CEO auto approve. UI hien chua co checkbox/field bat `is_auto_approve`.
   - Case H/I: Inactive approver / reroute can test rieng bang flow deactivation hoac level unassign.
   - Case J: One active employee per position per department chua implement code.

---

## 1. Tai Khoan Va Y Nghia

| Tai khoan | Vai tro trong UAT | Khi nao dung |
|---|---|---|
| `admin` | Cau hinh Approval Routing | Tao policy, tao level slot, assign approver, tao rule |
| `uat.provision80` | Nhan vien IT / Employee | Tao don nghi phep |
| `uat.provision81` | Manager IT / Approver | Vao dashboard W4/W5, mo detail va bam approve/reject |
| `ceo.test` | CEO / Company-level approver | Duyet don cua cap cao nhat phong ban trong Case F |
| `uat.provision86` | Nhan vien HRM | Dung sau neu can test phong HRM |

---

## 2. Cach Tao Mot Don Nghi Phep Mau

Dung cho cac case can tao don moi.

1. Mo `http://localhost:5300`.
2. Login bang tai khoan nguoi tao don, vi du `uat.provision80`.
3. Vao sidebar `LEAVE REQUESTS`.
4. Bam nut `Request Leave`.
5. Trong modal `SUBMIT LEAVE REQUEST`, dien:
   - `Leave Type`: chon `Sick Leave` neu con so du, neu loi thi chon `Annual Leave`.
   - `Start Date`: chon ngay tuong lai, vi du `2026-08-03`.
   - `End Date`: cung ngay voi Start Date neu test 1 ngay, vi du `2026-08-03`.
   - `Start Session`: `Full Day`.
   - `End Session`: `Full Day`.
   - `Reason`: dien `UAT dynamic approval routing`.
6. Bam `SUBMIT`.
7. Neu toast bao loi trung ngay hoac khong du so du, doi sang ngay khac trong tuong lai.
8. Sau khi tao thanh cong, dong modal/list reload.
9. Tim dong don moi trong list:
   - Status phai la `Pending`.
   - Cot `Process Info / Approver` phai hien `Assigned Approver`.
10. Bam `Details` de vao trang chi tiet.

Expected chung:

- Neu route hop le: detail hien `Approval Routing`.
- `Current Approver` phai dung nguoi duyet mong doi.
- `Routing Status` phai la `ASSIGNED`.
- `Assignment Reason` tuy case se la `DirectLevelMatch` hoac `SuperiorLevelEscalated`.

---

## 3. Case A - IT Employee Gui Don Cho IT Manager Duyet

Trang thai: da PASS, co the dung lam sanity test moi khi can.

### Buoc 1 - Tao don bang employee IT

1. Login `uat.provision80`.
2. Vao `LEAVE REQUESTS`.
3. Bam `Request Leave`.
4. Dien form theo muc 2.
5. Bam `SUBMIT`.
6. Bam `Details` tren don moi.

Expected:

- `Current Approver`: `uat.provision81 (EMP04)`.
- `Routing Status`: `ASSIGNED`.
- `Assignment Reason`: `DirectLevelMatch`.
- User `uat.provision80` khong thay nut `APPROVE REQUEST` / `REJECT REQUEST`.

### Buoc 2 - Kiem tra dashboard cua nguoi duyet

1. Logout.
2. Login `uat.provision81`.
3. Vao `DASHBOARD`.
4. Xem widget `W4 APPROVAL QUEUE`.
5. Tim don vua tao cua `uat.provision80`.
6. Bam `REVIEW`.

Expected:

- W4 co don cua `uat.provision80`.
- W5 `APPROVAL AGING` tang so luong pending theo bucket ngay phu hop.
- Trang detail hien panel `OFFICIAL DECISION PANEL`.
- Co nut `APPROVE REQUEST` va `REJECT REQUEST`.

Khong bam approve/reject neu anh muon giu don pending de test dashboard tiep.

---

## 4. TC6 - Man Hinh Legacy Chi Read-Only

Muc tieu: dam bao module cu `/leave-approver-assignment` khong con cho tao/sua/xoa.

1. Login `admin`.
2. Mo truc tiep URL: `http://localhost:5300/leave-approver-assignment`.
3. Quan sat header va table.

Expected:

- Tieu de: `APPROVAL CONFIGURATIONS (LEGACY)`.
- Co badge `LEGACY READ-ONLY`.
- Co dong: `LEGACY APPROVER ASSIGNMENTS - READ-ONLY AUDIT MODE`.
- Co nut/link `GO TO DYNAMIC APPROVAL ROUTING POLICIES`.
- Table van hien record lich su.
- Cot `Actions` chi hien `No Permission`.
- Khong co nut `Create`, `Edit`, `Remove`, `Delete`.

---

## 5. Case B - Leo Thang Theo Cau Hinh, Khong Tu Tim Manager Ngoai Config

Muc tieu nghiep vu: phong IT co the co nhieu position, nhung engine chi di theo candidate da cau hinh trong rule. Neu rule khai bao `Team Leader -> Department Header` thi he thong khong duoc tu y chen `Manager` vao giua.

Can luu y: case nay se thay doi cau hinh policy IT hien co. Nen chi lam khi anh chap nhan du lieu UAT bi thay doi.

### Buoc 1 - Admin them Level 2

1. Login `admin`.
2. Vao sidebar `APPROVAL ROUTING`.
3. Mo policy `Information Technology Approval Policy` bang nut `Detail`.
4. Trong box `LEVEL SLOTS SUMMARY`, bam `+ Add Level Slot`.
5. Dien:
   - `Level Name`: `Department Header`.
   - `Level Rank`: `2`.
   - Tick `Can Approve Leave Requests`.
6. Bam `Add Level`.

Expected:

- Trang reload.
- `LEVEL SLOTS SUMMARY` co them `Level 2: Department Header`.
- Level 2 dang `VACANT` neu chua assign.

### Buoc 2 - Assign Level 2 cho uat.provision81

1. O dong `Level 2: Department Header`, bam `Assign`.
2. Trong modal `Assign Level Slot`, chon:
   - `Approver Employee`: `uat.provision81 (EMP04)`.
   - `Assignment Reason`: `UAT Case B level 2 header assignment`.
3. Bam `Save Assignment`.

Expected:

- Level 2 hien `ASSIGNED`.
- Assigned: `uat.provision81 (EMP04)`.

### Buoc 3 - Them candidate thu 2 cho rule Employee

1. Van o policy detail, tim section `CONFIGURED POSITION RULES`.
2. Bam `+ Add Position Rule`.
3. Dien:
   - `Requester Position`: `Employee`.
   - `Target Candidate Level Slot`: `Level 2: Department Header`.
   - `Priority Order`: `2`.
4. Bam `Add Rule`.

Expected:

- Rule `Requester Position: Employee` co candidate:
  - Priority 1: level hien co.
  - Priority 2: `Level 2: Department Header`.

Neu UI bao loi rule duplicate thi nghia la hien flow `Add Rule` chua ho tro them candidate vao rule da ton tai theo cach anh dang thao tac. Dung lai va bao Codex/Anti, vi luc do la gap UI/handler limitation.

### Buoc 4 - Tao don va kiem ket qua

1. Logout.
2. Login `uat.provision80`.
3. Tao don nghi moi theo muc 2.
4. Mo detail cua don moi.

Expected mong muon cua Case B:

- Neu Level 1 khong co nguoi hop le, he thong gan sang Level 2.
- `Current Approver`: `uat.provision81 (EMP04)`.
- `Assignment Reason`: `SuperiorLevelEscalated`.
- He thong khong tu route sang `Manager` neu `Manager` khong nam trong candidate sequence cua rule.

Quan trong: Neu Level 1 hien van dang assigned cho `uat.provision81`, case nay se khong chung minh duoc escalation. Can lam Level 1 vacant/unassign trong phase rieng truoc khi test Case B that su.

---

## 6. Case E - Dynamic Reverse-Proof: Manager Gui Don Cho Employee Duyet

Muc tieu nghiep vu: chung minh engine khong hardcode "cap tren moi duoc duyet cap duoi". Neu policy cau hinh Manager -> Employee thi Employee duyet Manager.

Can luu y: day la test phi san xuat, chi dung de chung minh tinh dynamic.

### Buoc 1 - Admin tao Level cho Employee Approver

1. Login `admin`.
2. Vao `APPROVAL ROUTING`.
3. Mo `Information Technology Approval Policy` -> `Detail`.
4. Bam `+ Add Level Slot`.
5. Dien:
   - `Level Name`: `Temporary Employee Approver`.
   - `Level Rank`: `3` hoac so rank chua ton tai.
   - Tick `Can Approve Leave Requests`.
6. Bam `Add Level`.
7. O Level vua tao, bam `Assign`.
8. Chon:
   - `Approver Employee`: `uat.provision80 (EMP05)`.
   - `Assignment Reason`: `UAT Case E reverse routing test`.
9. Bam `Save Assignment`.

Expected:

- Level moi hien `ASSIGNED`.
- Assigned: `uat.provision80 (EMP05)`.

### Buoc 2 - Them rule Manager -> Employee Approver

1. O section `CONFIGURED POSITION RULES`, bam `+ Add Position Rule`.
2. Dien:
   - `Requester Position`: `Manager`.
   - `Target Candidate Level Slot`: `Level 3: Temporary Employee Approver`.
   - `Priority Order`: `1`.
3. Bam `Add Rule`.

Expected:

- Co rule `Requester Position: Manager`.
- Candidate priority 1 tro toi level gan `uat.provision80`.

### Buoc 3 - Manager tao don

1. Logout.
2. Login `uat.provision81`.
3. Vao `LEAVE REQUESTS`.
4. Bam `Request Leave`.
5. Dien form:
   - `Leave Type`: `Sick Leave` hoac `Annual Leave`.
   - `Start Date`: ngay tuong lai khong trung.
   - `End Date`: cung ngay.
   - `Reason`: `UAT Case E reverse dynamic route`.
6. Bam `SUBMIT`.
7. Mo detail don moi.

Expected:

- `Current Approver`: `uat.provision80 (EMP05)`.
- `Routing Status`: `ASSIGNED`.
- `Assignment Reason`: `DirectLevelMatch`.
- Login `uat.provision80` thi moi thay panel approve/reject cua don nay.

---

## 7. Case F - Manager IT Gui Don Len CEO Duyet

Muc tieu nghiep vu: cap cao nhat cua phong ban gui don len nguoi duyet cap cong ty, nhung khong hardcode role CEO trong code.

### Buoc 1 - Admin tao Level Company Top Approver

1. Login `admin`.
2. Vao `APPROVAL ROUTING`.
3. Mo `Information Technology Approval Policy`.
4. Bam `+ Add Level Slot`.
5. Dien:
   - `Level Name`: `Company Top Approver`.
   - `Level Rank`: `4` hoac rank chua ton tai.
   - Tick `Can Approve Leave Requests`.
6. Bam `Add Level`.
7. O Level vua tao, bam `Assign`.
8. Chon:
   - `Approver Employee`: `CEO Test (EMP-CEO-TEST)` hoac `ceo.test` neu dropdown hien username.
   - `Assignment Reason`: `UAT Case F company-level approver`.
9. Bam `Save Assignment`.

Expected:

- Level moi hien assigned cho CEO Test.

### Buoc 2 - Them rule Manager -> Company Top Approver

Neu da co rule Manager tu Case E, dung lai truoc khi lam Case F vi he thong co the khong cho tao rule Manager trung. Case E va Case F nen test tach nhau tren database reset/checkpoint, hoac sau khi co UI sua/xoa rule.

Neu chua co rule Manager:

1. Bam `+ Add Position Rule`.
2. Dien:
   - `Requester Position`: `Manager`.
   - `Target Candidate Level Slot`: `Level 4: Company Top Approver`.
   - `Priority Order`: `1`.
3. Bam `Add Rule`.

### Buoc 3 - Manager tao don, CEO kiem tra

1. Logout.
2. Login `uat.provision81`.
3. Vao `LEAVE REQUESTS`.
4. Bam `Request Leave`.
5. Dien reason: `UAT Case F manager to company approver`.
6. Bam `SUBMIT`.
7. Mo detail.

Expected:

- `Current Approver`: `CEO Test (EMP-CEO-TEST)`.
- `Routing Status`: `ASSIGNED`.
- `Assignment Reason`: `DirectLevelMatch`.

Sau do:

1. Logout.
2. Login `ceo.test`.
3. Vao `DASHBOARD`.

Expected:

- W4 co don cua `uat.provision81`.
- Mo detail thi co panel `OFFICIAL DECISION PANEL`.

---

## 8. Case C - Phong Chua Cau Hinh Policy Thi Bi Chan Tao Don

Trang thai: can test data. Khong nen lam ngay neu chua co nhan vien active thuoc phong chua co policy.

De UAT dung nghia can co:

- Mot employee active thuoc phong ban chua co `ApprovalRoutePolicy`.
- Employee do co user login va quyen `CREATE_LEAVE_REQUEST`.

Thao tac:

1. Login employee thuoc phong chua co policy.
2. Vao `LEAVE REQUESTS`.
3. Bam `Request Leave`.
4. Dien form hop le.
5. Bam `SUBMIT`.

Expected:

- Tao don bi chan.
- Thong bao loi:
  `Approval route is not configured for this department/position. Please assign an approver before submitting leave request.`
- Khong tao don pending moi.

Hien tai neu chua co account/data phu hop thi bo qua case nay, khong nen bien doi user/department chi de test neu chua duoc duyet.

---

## 9. Case D, G, H, I, J - Trang Thai Hien Tai

### Case D - Specific Employee Override

Nghiep vu can test:

- Rule co `specific_approver_employee_id`.
- Don se gan thang cho employee duoc chi dinh.
- `Assignment Reason` = `SpecificEmployeeOverride`.

Trang thai UI hien tai:

- Policy detail co hien thi specific override neu data da co.
- Nhung modal `Add Position Rule` hien chi co:
  - `Requester Position`
  - `Target Candidate Level Slot`
  - `Priority Order`
- Chua co field chon `Specific Approver`.

Ket luan: chua UAT bang UI duoc neu khong co phase bo sung UI.

### Case G - CEO Auto Approve

Nghiep vu can test:

- Rule cho terminal approver co `is_auto_approve = true`.
- CEO gui don thi don tu dong `APPROVED`, khong tao `LeaveRequestApprovalAssignment`.

Trang thai UI hien tai:

- Chua thay checkbox/field `Auto Approve` tren form tao rule.

Ket luan: chua UAT bang UI duoc neu khong co phase bo sung UI.

### Case H/I - Inactive Approver / Needs Admin Attention

Co controller va modal impact lien quan reassignment/unassign, nhung UAT case nay co rui ro lam thay doi pending requests. Chi nen test khi anh chap nhan tao du lieu rieng va thao tac deactivate/unassign theo kich ban co evidence.

### Case J - One Active Employee Per Position Per Department

Chua implement code. Khong UAT duoc.

---

## 10. Thu Tu UAT De Khong Bi Roi

Neu anh muon UAT nhanh va it pha du lieu nhat:

1. Lam Case A: da co config san, tao don `uat.provision80`, kiem `uat.provision81`.
2. Lam TC6: vao `/leave-approver-assignment`, xem legacy read-only.
3. Neu muon test dynamic nang cao:
   - Chon Case E hoac Case F, khong lam ca hai tren cung policy neu chua co UI sua/xoa rule Manager.
4. Chua nen lam Case B neu Level 1 van assigned, vi se khong chung minh duoc escalation.
5. Chua nen lam Case D/G/J vi UI/code chua du.

Khuyen nghi cua Codex:

- UAT hien tai coi la PASS cho feature duyet co ban.
- Neu muc tieu la "test tat ca business da ban", phase tiep theo nen la bo sung UI cho:
  - Specific Approver Override.
  - Auto Approve Rule.
  - Sua/xoa/deactivate rule/level an toan.
  - Test one active employee per position per department.

