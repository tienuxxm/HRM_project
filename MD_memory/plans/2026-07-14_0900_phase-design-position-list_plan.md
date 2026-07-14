# Kế hoạch Thiết kế: Position List UI (Swiss International HR Compliance)

*   **Tệp kế hoạch:** `MD_memory/plans/2026-07-14_0900_phase-design-position-list_plan.md`
*   **Mã Dự án Stitch:** `17479353588209716186`
*   **Phong cách thiết kế:** Swiss International HR (Style 06)

---

## 1. Mục tiêu (Objective)
Thiết kế giao diện quản lý danh sách chức vụ (**Position List**) trên môi trường Stitch (bao gồm cả bản Desktop và Mobile) tuân thủ hệ thống token và thiết kế Thụy Sĩ (Swiss International - Style 06). Đảm bảo giao diện mật độ cao, tinh gọn và hoàn toàn không chứa hàng thống kê hay metrics thừa.

---

## 2. Tiêu chí Đầu vào & Đầu ra (Entry & Exit Criteria)

### Tiêu chí Đầu vào (Entry Criteria)
*   [x] Màn hình giao diện `Department List` đã làm sạch stats và được người dùng phê duyệt hoàn toàn (`APPROVED_BY_USER`).
*   [x] Tệp báo cáo UAT làm sạch giao diện phòng ban được cập nhật vào dự án.

### Tiêu chí Đầu ra (Exit Criteria)
*   [ ] Giao diện **Position List Desktop** được tạo/tinh chỉnh thành công trên Stitch, đạt tiêu chuẩn của Swiss International (Style 06).
*   [ ] Giao diện **Position List Mobile** được tạo/tinh chỉnh thành công trên Stitch.
*   [ ] Toàn bộ mã HTML/DOM của cả hai giao diện không chứa bất kỳ từ khóa stats nào (VD: `Total Positions`, `TOTAL:`, v.v.).
*   [ ] Cập nhật trạng thái các màn hình sang `APPROVED_BY_USER` sau khi người dùng phê duyệt.

---

## 3. Bản phác thảo Giao diện (Layout Blueprint)

### Giao diện Desktop
*   **Sidebar:** Giữ nguyên cấu trúc sidebar modular phẳng từ các màn hình đã phê duyệt trước (Dashboard, Employee List, Department List). Các mục điều hướng:
    *   Dashboard
    *   Phòng ban (Department)
    *   Nhân viên (Employee)
    *   **Chức vụ (Position) — [Active state]**
    *   Quy trình nghỉ phép (Leave Flow)
*   **Header:** Tiêu đề "Danh sách Chức vụ (Positions)" bằng font chữ Geist, phong cách tối giản phẳng, đường viền phân tách 1px.
*   **Thanh công cụ (Toolbar):** Ô tìm kiếm nhanh (Search Input), nút "Thêm chức vụ mới (Create Position)" góc phải sử dụng màu accent đỏ Thụy Sĩ (Swiss Red) tối giản.
*   **Bảng dữ liệu chính (Data Grid):**
    *   Mật độ cao (compact spacing), đường kẻ khung 1px màu xám nhạt (`#E2E8F0` hoặc tương đương).
    *   Các cột thông tin:
        *   Mã chức vụ (Position Code) - Monospace font.
        *   Tên chức vụ (Position Name).
        *   Phòng ban trực thuộc (Department).
        *   Số lượng nhân viên đang giữ chức vụ (Employee Count).
        *   Mô tả ngắn (Description).
        *   Hành động (Actions) - gồm icon/nút phẳng "Sửa" (Edit), "Xóa" (Delete).

### Giao diện Mobile
*   **Header & Search:** Tiêu đề và ô tìm kiếm nhanh được co giãn thích ứng (responsive).
*   **Danh sách dạng thẻ phẳng (Dense List/Cards):** Hiển thị chi tiết từng chức vụ với mã chức vụ, tên chức vụ, phòng ban và số lượng nhân viên, kèm theo các nút hành động nhỏ gọn.
*   **Thanh điều hướng dưới (Mobile Navigation bar):** Giữ nhất quán với các màn hình di động khác.

---

## 4. Các bước thực hiện (Action Items)
1.  **Tạo màn hình mới trên Stitch:** Gửi prompt thiết kế Position List đồng bộ với phong cách Swiss International HR.
2.  **Áp dụng Design System:** Đảm bảo sử dụng chung Token màu sắc, bo góc (chủ yếu là góc phẳng 0-2px), font chữ Geist.
3.  **Tải xuống và Audit HTML:** Kiểm tra tệp HTML để đảm bảo không dính các chuỗi liên quan đến thống kê.
4.  **Báo cáo và xin phê duyệt từ User.**
