# Báo cáo: Swiss HR Color Token Purge - Work Calendar
> **Thời gian:** 2026-07-21 08:40
> **Phase:** phase-design-work-calendar-runtime (color token remediation)
> **Trạng thái:** Hoàn thành kiểm tra độc lập sau phản hồi UAT

---

## 1. Scope thực hiện

Chỉ sửa 3 file Razor view, không chạm C#/Controller/Application/Domain/DB/Auth/Layout:

| File | Nội dung sửa |
|---|---|
| `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Index.cshtml` | Đồng bộ badge Day Type, Status, nút Edit, warning/no-effect trong manual preview |
| `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Preview.cshtml` | Đồng bộ badge Day Type, Valid/Invalid, error row |
| `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Summary.cshtml` | Đồng bộ metric badges, status text, duration diff |

---

## 2. Màu đã loại bỏ

Các màu ngoài hệ Swiss HR đã được loại bỏ khỏi Work Calendar views:

- `green-*`
- `blue-*`
- `amber-*`
- `yellow-*`
- `indigo-*`
- `emerald-*`
- `sky-*`
- `teal-*`
- `lime-*`
- `orange-*`
- `purple-*`

Quy tắc còn lại:

- **Black / white / gray:** dùng cho trạng thái normal, active, valid, info.
- **Red `#E62429`:** chỉ dùng cho lỗi, destructive, invalid, warning thật sự.
- **Edit action:** chuyển về text đen, underline, không dùng xanh dương.

---

## 3. Verification

| Bước | Lệnh | Kết quả |
|---|---|---|
| Color scan | `rg -n "green-\|blue-\|amber-\|yellow-\|indigo-\|emerald-\|sky-\|teal-\|lime-\|orange-\|purple-\|text-blue\|bg-blue\|border-blue" HRM_Leave_Management/Web.Backend/Views/WorkCalendar` | Không còn match |
| Scope check | `git diff --name-status` | Chỉ có 3 WorkCalendar views và report này |
| Report encoding | `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/reports/2026-07-21_0840_phase-design-work-calendar_swiss-color-purge_report.md --require-bom` | Cần chạy lại sau khi áp BOM |

---

## 4. Ghi chú

- `Global /NotFound` không nằm trong scope Work Calendar, nên ghi nhận backlog UI riêng.
- Không stage/commit/push trong lượt sửa này.
- Lưu ý phản biện: phản hồi trước đó của Anti bị mojibake ở chính file report, nên báo cáo này được ghi lại bằng tiếng Việt sạch để tiếp tục theo dõi phase.
