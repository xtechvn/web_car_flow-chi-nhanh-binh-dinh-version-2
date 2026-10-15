# CHANGELOG – WEB.CMS

Mỗi lần thay đổi code phải thêm một mục mới ở đầu file: ngày, mục đích, danh sách file, lưu ý vận hành.

---

## 2026-10-15 – Sửa màn "Danh sách xe đăng ký" không hiện xe

**Nguyên nhân:**
1. `SP_GetListVehicleInspection` lấy mốc đầu đợt = giờ đăng ký của xe `RecordNumber = 1`. Xe số 1–7 đợt 16/10 bị mất
   (n8n xóa message khỏi queue khi API lỗi SSL) → mốc NULL → danh sách trống.
2. CMS luôn dùng giờ reset hôm nay → xe đăng ký sau giờ chốt chỉ hiện từ 00:00.

**Sửa**

| File | Thay đổi |
|---|---|
| `Repositories/Repositories/VehicleInspectionRepository.cs` (`GetListCartoFactory`) | Đã qua giờ reset → dùng giờ reset ngày mai |
| `Scripts/SP_GetListVehicleInspection_20261015.sql` (mới, **đã chạy trên `Cargill_BinhDuong_v2`** lúc 22:58 15/10) | Lọc `[mốc − 1 ngày, mốc)`, bỏ phụ thuộc xe số 1 |
| `WEB.CMS/Docs/README.md` | Thêm mục 4.1 + sơ đồ |

Bản cũ: `binh_duong/_backup/SQL_before_window_fix_20261015/` (SP + Repository).

**Lưu ý vận hành**

- Phải deploy lại WEB.CMS (DLL `Repositories`) thì phần "hiện ngay sau giờ chốt" mới có hiệu lực; SP đã áp dụng ngay.
- Node RabbitMQ Trigger trên n8n phải đặt *Delete From Queue When = When Execution Finishes Successfully*.

---

## 2026-10-04 – Workflow n8n mẫu (chỉ tài liệu)

| File | Thay đổi |
|---|---|
| `Docs/n8n-registration-consumer.json` | Mới: Code parse `content` → HTTP Request gọi API → IF 200/400 ack, lỗi khác Stop and Error |
| `Docs/README.md` | Mục 7: hướng dẫn dán workflow vào n8n |

---

## 2026-10-04 – API nhận đăng ký xe từ n8n

**Mục đích:** n8n đọc queue `QUEUE_CAR_REGISTRATION_BINH_DUONG` (do XTECH-FRONTEND đẩy vào)
rồi gọi API này để ghi bảng `VehicleInspection`, thay cho luồng cũ gọi `api.longancargill.com`.

**Thêm mới**

| File | Nội dung |
|---|---|
| `Controllers/API/RegistrationQueueController.cs` | `POST /api/registration-queue/receive`: API key, validate, idempotent theo `MessageId`, gọi `sp_InsertVehicleInspection`, publish realtime, đẩy queue audio |
| `Models/RegistrationQueueMessage.cs` | Hợp đồng message (giống XTECH-FRONTEND) |
| `Docs/README.md`, `Docs/CHANGELOG.md` | Tài liệu |

**Sửa**

| File | Thay đổi |
|---|---|
| `Services/RedisConn.cs` | `Connect()` chỉ tạo 1 kết nối (có lock, `abortConnect=false`); thêm `EnsureConnected`, `SetIfNotExistsAsync`, `SetAsync(TimeSpan)`, `DeleteAsync` |
| `appsettings.json` | Thêm section `RegistrationConsumer` (`ApiKey`, `Site`, `RealtimeChannel`, `ProcessedKeyHours`) |

Bản sao trước khi sửa: `binh_duong/_backup/WEB.CMS_before_n8n_api_20261004/`.

**Lưu ý vận hành**

- Deploy phải bổ sung section `RegistrationConsumer` vào `appsettings.json` trên server và nhập cùng API key vào n8n.
- n8n phải ack message sau khi API trả 200 (xem README mục 7).
