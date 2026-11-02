# LAN Video Streaming

Ứng dụng **Stream Video qua mạng LAN** theo mô hình **Client – Server** sử dụng **C# / .NET 10 / Windows Forms / OpenCvSharp4 / UDP Socket**.

## 1. Công nghệ sử dụng

| Thành phần          | Công nghệ                   |
| ------------------- | --------------------------- |
| Ngôn ngữ            | C#                          |
| Framework           | .NET 10 (`net10.0-windows`) |
| IDE                 | Visual Studio Code / VS     |
| Giao diện           | Windows Forms               |
| Xử lý video         | OpenCvSharp4                |
| Truyền dữ liệu      | UDP Socket (`Async/Await`)  |
| Nén hình ảnh        | JPEG                        |
| Lưu video           | OpenCV `VideoWriter`        |
| Phát hiện khuôn mặt | OpenCV (mở rộng)            |

## 2. Cấu trúc thư mục

```text
LANVideoStreaming/
├── LANVideoStreaming.sln
├── README.md
├── .gitignore
├── src/
│   ├── LanStream.Client/       # (TV2) Webcam, nén JPEG, gửi video
│   ├── LanStream.Server/       # (TV1) Nhận & hiển thị video, ghi hình, nhận diện
│   ├── LanStream.Network/      # (TV3) UDP Sender/Receiver, chia & ghép gói tin
│   └── LanStream.Shared/       # (TV1 + TV3) Models & giao thức UDP dùng chung
├── tests/
│   └── LanStream.Tests/        # (TV3 + TV4) Unit tests cho giao thức & lắp ghép gói
├── assets/
│   └── models/                 # Chứa model phát hiện khuôn mặt (Haar Cascade / YuNet)
├── recordings/                 # Thư mục lưu video ghi hình từ Server
└── docs/                       # (TV4) Tài liệu yêu cầu, kiến trúc, giao thức, kiểm thử
```

## 3. Hướng dẫn chạy nhanh

### Yêu cầu cài đặt
- [.NET 10 SDK](https://dotnet.microsoft.com/download) trên Windows.

### Build & Chạy kiểm thử
```powershell
dotnet restore
dotnet build
dotnet test
```

### Chạy ứng dụng (trên 2 cửa sổ Terminal riêng biệt)

1. **Khởi động Server trước:**
   ```powershell
   dotnet run --project src/LanStream.Server
   ```

2. **Khởi động Client:**
   ```powershell
   dotnet run --project src/LanStream.Client
   ```

> Lưu ý: Khi chạy trên 2 máy tính khác nhau trong cùng mạng LAN, kiểm tra IPv4 của máy Server bằng `ipconfig` và mở cổng **UDP 5000** (mạng Private) trên Windows Firewall.

## 4. Tài liệu chi tiết
- [Yêu cầu hệ thống (REQUIREMENTS)](docs/REQUIREMENTS.md)
- [Kiến trúc hệ thống (ARCHITECTURE)](docs/ARCHITECTURE.md)
- [Đặc tả giao thức UDP (UDP_PROTOCOL)](docs/UDP_PROTOCOL.md)
- [Kế hoạch kiểm thử (TEST_PLAN)](docs/TEST_PLAN.md)
- [Hướng dẫn sử dụng (USER_GUIDE)](docs/USER_GUIDE.md)
