# BÁO CÁO KIỂM THỬ UAT CHI TIẾT - CHUẨN HÓA MODAL SHELL & BACKDROP DIM (EMPLOYEE UI)
**Ngày lập**: 13/07/2026  
**Môi trường thử nghiệm**: Localhost:5300 (Active Runtime)  
**Trạng thái kiểm thử**: **SUCCESSFUL**

---

## 1. Phân Loại Trạng Thế Kiểm Thử & Giải Pháp Sửa Lỗi Overlay

| Tiêu Chí Đánh Giá | Trạng Thái | Kết Quả Thực Tế & Minh Chứng |
| :--- | :---: | :--- |
| **Panel Alignment (Căn giữa)** | **PASS** | Cả 4 modal (**Add, Edit, Provision, Delete**) đều được căn giữa tuyệt đối theo cả 2 chiều dọc và ngang trong khung hình (Viewport Centered). Không bị lệch lên top-left trên mobile hay desktop. |
| **Backdrop Dimming (Màu nền mờ phía sau)** | **PASS** | - **Sự cố ban đầu**: Tailwind CSS build hiện tại của dự án không tự sinh ra class opacity động `bg-black/40`, khiến cho phần tử backdrop hiển thị nhưng bị trong suốt.<br>- **Giải pháp chuẩn hóa**: Định nghĩa một shared CSS class chung `.hrm-modal-backdrop` tại style block toàn cục trong `Views/Shared/_Layout.cshtml` với giá trị màu được xác định rõ ràng: `.hrm-modal-backdrop { background-color: rgba(0, 0, 0, 0.4) !important; }`. Cập nhật cả 4 file modal partials sử dụng chung class này.<br>- **Kết quả**: Đo computed style thực tế qua trình duyệt trả về giá trị màu chính xác **`rgba(0, 0, 0, 0.4)`** (tối mờ 40%), mang lại hiệu quả dim rõ rệt như Stitch approved screens. |
| **Console Errors (Lỗi Trình Duyệt)** | **PASS** | Không phát sinh lỗi `$ is not defined` hoặc lỗi Flowbite modal initialization (`has not been initialized`). |
| **Visual Match (Kiểu Dáng Swiss/Stitch)** | **PASS** | - **Header**: Nền đen (`bg-black`), nút đóng đỏ Swiss (`bg-swiss-accent-red hover:bg-swiss-red`).<br>- **Colors**: Không còn màu xanh primary (`blue-600` / Bootstrap primary) cũ.<br>- **Borders & Corners**: Viền hairline đen (`border-black`), góc vuông hoàn toàn (`rounded-none` / 0px radius), phẳng hoàn toàn (`shadow-none`). |
| **Mobile Viewport (Tương Thích Di Động)** | **PASS** | Chiều rộng tự động co giãn vừa vặn (max-w-[640px] thu về khoảng 343px trên viewport 375px), không có hiện tượng tràn viền (overflow-x) hay lệch layout. |

---

## 2. Hình Ảnh Minh Chứng Thực Tế (UAT Evidence)

* **Bằng chứng 1: Add Employee Modal & Backdrop Dim (Desktop View)**  
  ![Add Employee Modal with Backdrop](file:///C:/Users/Tienht/.gemini/antigravity/brain/543bf4d5-2f60-43bb-adc8-0a5ec941849a/add_employee_modal_1783929574037.png)  
  *Nhận xét*: Giao diện nền phía sau bị làm mờ tối rõ rệt bởi lớp phủ đen mờ tĩnh `rgba(0, 0, 0, 0.4)`. Modal panel nổi bật ở trung tâm.

* **Bằng chứng 2: Add Employee Modal (Mobile View - Centered & Fit)**  
  ![Add Employee Mobile View](file:///C:/Users/Tienht/.gemini/antigravity/brain/543bf4d5-2f60-43bb-adc8-0a5ec941849a/add_employee_modal_mobile_1783929673670.png)  
  *Nhận xét*: Trình duyệt ở kích thước di động hiển thị modal cân đối ở giữa, có phần dim đen ở các mép viền ngoài panel.

* **Bằng chứng 3: Confirm Delete Modal (Desktop View)**  
  ![Confirm Delete Modal with Backdrop](file:///C:/Users/Tienht/.gemini/antigravity/brain/543bf4d5-2f60-43bb-adc8-0a5ec941849a/confirm_delete_modal_1783929650515.png)  
  *Nhận xét*: Backdrop mờ tối và panel Confirm Delete cân đối ở giữa màn hình.

---

## 3. Đối Chiếu So Với Thiết Kế Stitch Approved
* **Vị trí**: Đạt yêu cầu căn giữa (Centered).
* **Backdrop**: Đạt yêu cầu dim đen tối giản (`rgba(0, 0, 0, 0.4)`), loại bỏ sự phân tán sự chú ý.
* **Màu sắc header & nút bấm**: Đạt yêu cầu nền đen (`bg-black`), nút đóng dùng `bg-swiss-accent-red`. Đã gỡ bỏ toàn bộ màu xanh dương cũ.
* **Độ dày viền & Bo góc**: Đường viền mỏng (`border-black`), không bo góc (`rounded-none` / 0px), không đổ bóng (`shadow-none`) đúng theo phong cách công nghiệp tối giản.
* **Mobile layout**: Không có sự dịch chuyển layout (no layout shift) hay tràn màn hình.
