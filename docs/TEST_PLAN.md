# KẾ HOẠCH KIỂM THỬ (TEST PLAN)

## 1. Môi trường kiểm thử
1. **Môi trường Localhost (Loopback):** Chạy đồng thời `LanStream.Server` và `LanStream.Client` trên cùng 1 máy Windows (`127.0.0.1:5000`).
2. **Môi trường mạng LAN thực tế:** 2 máy tính Windows kết nối chung Wi-Fi / Switch nội bộ, cấu hình Firewall cho phép cổng UDP `5000` (Private Network).

## 2. Danh sách kịch bản kiểm thử (Test Cases)

| Mã TC | Tên bài kiểm thử                | Các bước thực hiện                                      | Kết quả mong đợi                                     | Trạng thái |
| ----- | ------------------------------- | ------------------------------------------------------- | ---------------------------------------------------- | ---------- |
| TC01  | Mở Webcam Preview tại Client    | Mở Client, chọn camera 0, nhấn `Start Camera`           | Hiển thị hình ảnh webcam mượt, không treo giao diện  | Pending    |
| TC02  | Khởi động Server lắng nghe      | Mở Server, nhập port `5000`, nhấn `Start Server`        | Trạng thái chuyển sang đang chờ video, UI phản hồi   | Pending    |
| TC03  | Truyền video qua Localhost      | Chạy Server `5000`, Client gửi tới `127.0.0.1:5000`     | Server hiển thị video liên tục                       | Pending    |
| TC04  | Truyền video giữa 2 máy LAN     | Client nhập IPv4 LAN của Server và nhấn `Start Stream`  | Server nhận và hiển thị video ổn định (~15 FPS)      | Pending    |
| TC05  | Nhập sai định dạng IP / Port    | Nhập IP không hợp lệ (vd: `999.1.1.1`) hoặc Port âm     | Cảnh báo hợp lệ trên UI, không văng ngoại lệ         | Pending    |
| TC06  | Stop rồi Start lại nhiều lần    | Nhấn `Stop` sau đó `Start` liên tiếp ở cả Client/Server | Không lỗi chiếm dụng camera hoặc trùng cổng Socket   | Pending    |
| TC07  | Gói UDP đến sai thứ tự          | Chạy Unit Test hoán đổi thứ tự các chunk trước khi ghép | `FrameAssembler` vẫn ghép ra đúng mảng byte JPEG gốc | Pending    |
| TC08  | Thiếu chunk trong khung hình    | Bỏ 1 chunk bất kỳ và chờ quá `200 ms`                   | Hủy bỏ frame thiếu gói, tăng `DroppedFrames`         | Pending    |
| TC09  | Gói tin hỏng Magic / Version    | Gửi gói byte ngẫu nhiên hoặc sai `Magic` vào cổng 5000  | Bỏ qua gói không hợp lệ, Server không bị crash       | Pending    |
| TC10  | Ghi hình video ra file          | Bật `Start Recording` trong 10 giây rồi nhấn `Stop`     | File tạo trong `recordings/` mở xem được bình thường | Pending    |
| TC11  | Ngắt kết nối Webcam đột ngột    | Rút USB webcam hoặc tắt quyền camera khi đang stream    | Hiển thị thông báo lỗi và dọn dẹp tài nguyên an toàn | Pending    |
| TC12  | Kiểm thử chặn bởi Firewall      | Bật chặn cổng UDP 5000 trên máy Server                  | Server báo không có tín hiệu; mở rule thì nhận lại   | Pending    |
| TC13  | Chạy liên tục 5 phút (Stress)   | Giữ phiên truyền liên tục trong 5 phút                  | RAM ổn định (không rò rỉ `Bitmap`/`Mat`), không đơ   | Pending    |
| TC14  | Bật tính năng phát hiện mặt     | Tích chọn `Face Detection` trên giao diện Server        | Xuất hiện khung bao quanh khuôn mặt trong khung hình | Pending    |

## 3. Các chỉ số cần thu thập cho báo cáo
- **FPS trung bình:** Gửi tại Client vs. Nhận tại Server.
- **Tỷ lệ mất khung hình:** `DroppedFrames / (CompletedFrames + DroppedFrames) (%)`.
- **Băng thông tiêu thụ:** `KB/s` hoặc `Mbps`.
- **Tài nguyên hệ thống:** Mức sử dụng CPU (%) và RAM (MB) của cả 2 tiến trình.
