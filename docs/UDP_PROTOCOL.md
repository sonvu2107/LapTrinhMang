# ĐẶC TẢ GIAO THỨC TRUYỀN VIDEO UDP (UDP PROTOCOL v1)

## 1. Thông số truyền tải mặc định

| Thông số                | Giá trị mặc định / Giới hạn | Ghi chú                                       |
| ----------------------- | --------------------------- | --------------------------------------------- |
| Cổng UDP mặc định       | `5000`                      | Unicast từ Client tới `ServerIP:5000`         |
| Thứ tự byte (Endianness)| Big-Endian (Network Order)  | Sử dụng `BinaryPrimitives` trong .NET         |
| Kích thước Datagram     | Tối đa `1.200` byte         | An toàn dưới ngưỡng MTU Ethernet (1.500 byte) |
| Kích thước Header       | Cố định `24` byte           | Xem cấu trúc bảng bên dưới                    |
| Kích thước Payload      | Tối đa `1.176` byte         | `Datagram.Length - 24`                        |
| Số chunk tối đa / frame | `256` chunk                 | Tương đương kích thước JPEG tối đa ~256 KiB   |
| Thời gian chờ ghép gói  | `200` ms                    | Quá hạn sẽ hủy bỏ frame bị thiếu gói          |

## 2. Cấu trúc Header (24 Bytes)

| Offset | Độ dài (Byte) | Tên trường    | Kiểu dữ liệu | Giá trị / Mô tả                                      |
| -----: | ------------: | ------------- | ------------ | ---------------------------------------------------- |
|      0 |             2 | `Magic`       | `ushort`     | `0x4C56` (ASCII `'L'`, `'V'`)                        |
|      2 |             1 | `Version`     | `byte`       | `1` (Phiên bản giao thức hiện tại)                   |
|      3 |             1 | `Flags`       | `byte`       | `0` = Gói dữ liệu hình ảnh (`DATA`)                  |
|      4 |             4 | `StreamId`    | `uint`       | Định danh phiên stream do Client sinh khi nhấn Start |
|      8 |             4 | `FrameId`     | `uint`       | Số thứ tự khung hình tăng dần (`0, 1, 2, ...`)       |
|     12 |             2 | `ChunkIndex`  | `ushort`     | Chỉ số mảnh hiện tại (`0 .. ChunkCount - 1`)         |
|     14 |             2 | `ChunkCount`  | `ushort`     | Tổng số mảnh của khung hình (`1 .. 256`)             |
|     16 |             8 | `TimestampMs` | `long`       | Thời điểm gửi (Unix Epoch milliseconds UTC)          |
|     24 |     `<= 1176` | `Payload`     | `byte[]`     | Dữ liệu nhị phân một phần của ảnh JPEG               |

## 3. Quy tắc kiểm tra hợp lệ (Validation Rules)
Một gói tin nhận được tại Server bị loại bỏ ngay nếu vi phạm bất kỳ điều kiện nào sau đây:
1. Tổng độ dài gói `< 24` byte hoặc `> 1200` byte.
2. `Magic != 0x4C56` hoặc `Version != 1`.
3. `ChunkCount == 0` hoặc `ChunkCount > 256`.
4. `ChunkIndex >= ChunkCount`.
5. Tổng dung lượng JPEG sau khi ghép vượt quá `262.144` byte (`256 KiB`).

## 4. Chính sách lắp ghép & Xử lý mất gói
- Khóa định danh bộ đệm khung hình: `(RemoteEndPoint, StreamId, FrameId)`.
- **Đảo thứ tự gói:** Chấp nhận lưu các `ChunkIndex` đến không theo thứ tự vào mảng tạm.
- **Trùng lặp gói:** Nếu `ChunkIndex` đã tồn tại trong buffer của cùng `FrameId`, bỏ qua gói trùng.
- **Mất gói / Quá hạn:** Không yêu cầu truyền lại (không retransmit). Các frame chưa đủ `ChunkCount` sau `200 ms` kể từ gói đầu tiên sẽ bị dọn khỏi bộ nhớ và tính vào bộ đếm `DroppedFrames`.
