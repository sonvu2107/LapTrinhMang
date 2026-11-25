# HƯỚNG DẪN CÀI ĐẶT & SỬ DỤNG (USER GUIDE)

## 1. Yêu cầu hệ thống
- Hệ điều hành: **Windows 10 / Windows 11 (64-bit)**.
- SDK / Runtime: **.NET 10 SDK** (kiểm tra bằng lệnh `dotnet --version`).
- Thiết bị: Máy Client có kết nối Webcam (tích hợp trên laptop hoặc USB Webcam).

## 2. Cài đặt & Biên dịch mã nguồn

Mở PowerShell hoặc Terminal của VS Code tại thư mục gốc của dự án:

```powershell
dotnet restore
dotnet build
```

## 3. Thiết lập mạng LAN & Tường lửa (Khi chạy trên 2 máy)

1. Đảm bảo máy **Server** và máy **Client** kết nối vào cùng một mạng Wi-Fi / LAN nội bộ (đặt cấu hình mạng ở chế độ **Private Network**).
2. Trên máy **Server**, lấy địa chỉ IPv4 bằng lệnh:
   ```powershell
   ipconfig
   ```
   Ghi lại dòng `IPv4 Address` (ví dụ: `192.168.1.100`).
3. Mở cổng **UDP 5000** trên Windows Defender Firewall của máy **Server** (chạy PowerShell với quyền Administrator nếu bị chặn):
   ```powershell
   New-NetFirewallRule -DisplayName "LAN Video Stream Server UDP 5000" -Direction Inbound -Protocol UDP -LocalPort 5000 -Profile Private -Action Allow
   ```

## 4. Vận hành ứng dụng

### Bước 1: Chạy ứng dụng Server
```powershell
dotnet run --project src/LanStream.Server
```
1. Kiểm tra ô **Listening Port** (mặc định `5000`).
2. Nhấn **Start Server** để bắt đầu lắng nghe luồng video.
3. (Tùy chọn) Nhấn **Record** để lưu video vào thư mục `recordings/`.
4. Tích **Phát hiện khuôn mặt** để Server vẽ khung xanh quanh khuôn mặt. Có thể bật hoặc tắt trong lúc đang nhận video. Nếu bộ phát hiện tạm mất dấu, OpenCV dùng các điểm ảnh còn theo dõi được để di chuyển khung trong tối đa 250 ms kể từ lần phát hiện gần nhất. Tệp ghi hình lưu hình ảnh gốc.
5. Mô hình [YuNet của OpenCV](https://github.com/opencv/opencv_zoo/tree/main/models/face_detection_yunet) được sao chép tự động khi build hoặc publish Server. Khi chuyển bản phát hành sang máy khác, chép cả thư mục `assets/models` đi kèm Server.

### Bước 2: Chạy ứng dụng Client
```powershell
dotnet run --project src/LanStream.Client
```
1. Nhập địa chỉ IP của máy Server vào ô **Server IP** (dùng `127.0.0.1` nếu chạy thử trên cùng một máy).
2. Nhập **Port** `5000`, chọn thiết bị **Camera** và mức chất lượng nén JPEG.
3. Có thể nhấn **Start Camera** để xem preview cục bộ; nhấn **Stop** để dừng preview.
4. Nhấn **Start Stream** để mở webcam, nén JPEG và truyền hình ảnh sang Server.
5. Nhấn **Stop** khi muốn kết thúc phiên truyền.

## 5. Ghi chú vận hành
- Client giữ tối đa một frame đang chờ gửi; nếu gửi chậm hơn tốc độ camera thì frame mới sẽ bị bỏ để hạn chế độ trễ tăng dần.
- UDP không đảm bảo giao nhận. Server ghép các chunk trong thời gian chờ cấu hình; frame thiếu chunk sẽ bị loại bỏ, không truyền lại.
- FPS/băng thông là ước tính theo dữ liệu chạy thực tế. Kiểm thử camera, codec ghi video, firewall và mạng LAN cần thực hiện trên máy Windows có thiết bị tương ứng.
