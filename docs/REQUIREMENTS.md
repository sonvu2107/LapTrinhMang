# TÀI LIỆU ĐẶC TẢ YÊU CẦU (REQUIREMENTS)

## 1. Giới thiệu
Dự án **LAN Video Streaming** xây dựng hệ thống truyền phát video thời gian thực qua mạng nội bộ (LAN) theo mô hình Client – Server sử dụng giao thức UDP.

## 2. Yêu cầu chức năng (Functional Requirements)

| Mã  | Chức năng                 | Độ ưu tiên | Mô tả chi tiết                                                        | Phụ trách |
| --- | ------------------------- | ---------- | --------------------------------------------------------------------- | --------- |
| F01 | Mở và hiển thị Webcam     | Bắt buộc   | Thu hình ảnh từ webcam máy Client (640×480) và hiển thị preview       | TV2       |
| F02 | Nhập IP, Port Server      | Bắt buộc   | Cho phép nhập IPv4 của Server và cổng UDP (mặc định `5000`)           | TV2       |
| F03 | Nén frame JPEG            | Bắt buộc   | Resize về 640×480 và nén ảnh định dạng JPEG (chất lượng 60–70)        | TV2       |
| F04 | Gửi video bằng UDP Async  | Bắt buộc   | Gửi các datagram UDP bất đồng bộ không làm nghẽn luồng giao diện      | TV3       |
| F05 | Chia và ghép gói tin      | Bắt buộc   | Chia frame JPEG thành các gói <= 1.200 byte và ghép lại ở phía nhận   | TV3       |
| F06 | Server nhận video         | Bắt buộc   | Lắng nghe cổng UDP, kiểm tra header, loại bỏ gói lỗi/quá hạn (200 ms) | TV1 + TV3 |
| F07 | Hiển thị video Server     | Bắt buộc   | Giải mã JPEG và cập nhật hình ảnh lên `PictureBox` mượt mà            | TV1       |
| F08 | Start/Stop an toàn        | Bắt buộc   | Dừng/khởi động lại luồng camera và socket an toàn qua `CancellationToken` | TV1 + TV2 |
| F09 | Ghi video MP4             | Mở rộng    | Ghi lại luồng video nhận được ra file `.mp4` (hoặc `.avi` dự phòng)   | TV1       |
| F10 | Phát hiện khuôn mặt       | Mở rộng    | Vẽ khung chữ nhật quanh khuôn mặt phát hiện được bằng OpenCV          | TV1 + TV2 |
| F11 | FPS và thống kê mất frame | Mở rộng    | Tính toán và hiển thị FPS gửi/nhận, số frame hoàn chỉnh và bị bỏ      | TV3       |
| F12 | Kiểm thử và lập báo cáo   | Bắt buộc   | Thực hiện kiểm thử trên localhost & 2 máy LAN, tổng hợp số liệu       | TV4       |

## 3. Yêu cầu phi chức năng (Non-Functional Requirements)
- **Hiệu năng:** Duy trì tốc độ ~15 FPS ở độ phân giải 640×480 trong mạng LAN ổn định.
- **Độ phản hồi UI:** Toàn bộ tác vụ đọc camera, nén/giải mã ảnh, gửi/nhận UDP chạy nền (`async/await` / background task), không gây đơ giao diện WinForms.
- **Chịu lỗi:** Không crash khi mất gói UDP, sai thứ tự gói tin, rút webcam đột ngột hoặc nhập sai IP/Port.
- **Quản lý bộ nhớ:** Giải phóng đúng `Mat`, `Bitmap`, `VideoCapture`, `VideoWriter` và buffer quá hạn.
