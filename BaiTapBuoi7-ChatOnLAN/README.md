# Bài tập buổi 7 · ChatOnLAN · Số thứ tự 43

Sinh viên số lẻ sử dụng **TCP**. Chương trình Java dùng `JOptionPane` để nhập và hiển thị tin nhắn giữa một Client và một Server.

## Biên dịch

Mở PowerShell tại thư mục bài tập và đặt `javac`, `java` trong `PATH`. Với JDK 9 trở lên, biên dịch tương thích Java 8 bằng lệnh sau.

```powershell
New-Item -ItemType Directory -Path bin -Force | Out-Null
javac --release 8 -encoding UTF-8 -d bin ChatServer.java ChatClient.java ChatSession.java
```

Nếu dùng JDK 8, bỏ `--release 8` khỏi lệnh. Kết quả chạy được bằng Java 8 trở lên.

## Chạy trên một máy

1. Mở PowerShell thứ nhất tại thư mục bài tập và chạy `java -cp bin ChatServer`.
2. Khi Server báo đang chờ kết nối, mở PowerShell thứ hai tại cùng thư mục và chạy `java -cp bin ChatClient`.
3. Nhập câu hỏi ở hộp thoại Client và bấm **OK**. Server hiện câu hỏi. Bấm **OK** trên hộp thoại tin đến để mở hộp thoại nhập câu trả lời.
4. Client nhận câu trả lời rồi có thể gửi câu hỏi tiếp theo. Bấm **Cancel** hoặc đóng hộp thoại nhập của bên đang đến lượt để kết thúc cả hai chương trình.

## Chạy trên hai máy trong LAN

1. Biên dịch trên cả hai máy. Chạy `ChatServer` trên máy Server.
2. Dùng `ipconfig` trên Server để lấy địa chỉ IPv4 của Wi-Fi hoặc Ethernet đang dùng. Nếu tường lửa chặn kết nối, cho phép kết nối đến TCP cổng `5000` trên máy Server.
3. Trên máy Client, truyền IPv4 của Server vào lệnh chạy. Ví dụ với địa chỉ `192.168.1.25`:

   ```powershell
   java -cp bin ChatClient 192.168.1.25
   ```

## Cách hoạt động

- `ChatServer.java` mở `ServerSocket` cổng `5000` và đợi một kết nối.
- `ChatClient.java` kết nối bằng `Socket`. Nếu không truyền IP, Client kết nối tới `127.0.0.1`.
- `ChatSession.java` thực hiện lượt gửi và nhận. Mỗi tin nhắn là một dòng UTF-8. Khi một bên bấm **Cancel**, socket phía đó đóng và bên đang đợi đọc nhận tín hiệu kết thúc luồng.
