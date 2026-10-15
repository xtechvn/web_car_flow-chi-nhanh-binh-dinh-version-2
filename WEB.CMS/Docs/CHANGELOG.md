# CHANGELOG – WEB.CMS

Mỗi lần thay đổi code phải thêm một mục mới ở đầu file: ngày, mục đích, danh sách file, lưu ý vận hành.

---

## 2026-10-16 – Đẩy lại xe bị thiếu từ MongoDB + khôi phục quy tắc gốc

**Mục đích:** xe có trong MongoDB (FE đã cấp số) nhưng chưa vào SQL (message mất / n8n lỗi) được hiển thị
bằng icon nhấp nháy có số đếm cạnh tiêu đề "Danh sách xe đăng ký"; bấm icon mở lightbox để đẩy vào hệ thống.

**Khôi phục (theo yêu cầu, không đổi quy tắc dự án):**

- `SP_GetListVehicleInspection` trên `Cargill_BinhDuong_v2` đã ALTER lại đúng bản gốc (đã đối chiếu khớp).
- `Repositories/Repositories/VehicleInspectionRepository.cs` chép lại bản gốc.
- Xóa `source/Scripts/SP_GetListVehicleInspection_20261015.sql`.
- Mục 2026-10-15 bên dưới **không còn hiệu lực**.

**Thêm mới**

| File | Nội dung |
|---|---|
| `Services/RegistrationImportService.cs` | Kiểm tra xe đã có trong SQL + insert (có khóa), publish realtime, queue audio. Dùng chung cho API n8n và đẩy lại |
| `Services/RegistrationRecoveryService.cs` | Tính đợt hiện tại từ `TIME_RESET`, đối chiếu MongoDB ↔ SQL, đẩy xe thiếu theo số thứ tự |
| `Controllers/RegistrationRecoveryController.cs` | `Count`, `Popup`, `Push` (`[CustomAuthorize]`) |
| `Views/RegistrationRecovery/Popup.cshtml` | Lightbox danh sách xe thiếu + nút "Đẩy vào hệ thống" |
| `wwwroot/modules/registrationRecovery.js` | Đếm mỗi 30 giây, hiện/ẩn icon, mở lightbox, đẩy |

**Sửa**

| File | Thay đổi |
|---|---|
| `Controllers/API/RegistrationQueueController.cs` | Dùng `RegistrationImportService`; xe đã có trong SQL → `200 DUPLICATE` |
| `Views/Car/CartoFactory.cshtml` | Icon nhấp nháy + số đếm cạnh tiêu đề, CSS, nạp `registrationRecovery.js` |
| `Program.cs` | Đăng ký `RegistrationImportService`, `RegistrationRecoveryService` (singleton) |
| `appsettings.json` | Thêm section `MongoServer` |
| `Docs/README.md` | Thay mục 4.1 bằng quy tắc gốc, thêm mục 4.2 + sơ đồ, bảng cấu hình |

Bản sao trước khi sửa: `binh_duong/_backup/WEB.CMS_before_recovery_20261015/`.

**Lưu ý vận hành**

- Deploy phải thêm section `MongoServer` vào `appsettings.json` trên server; server CMS phải kết nối được MongoDB `103.163.216.60:27017`.
- Kiểm tra đọc dữ liệu thật lúc triển khai (không ghi): đợt 15/10 17:18 → 16/10 17:18 thiếu 4 xe – số 1 `30E45321`, 2 `30E11111`, 6 `30E4555`, 7 `30E852`.

---

## 2026-10-15 – Sửa màn "Danh sách xe đăng ký" không hiện xe (ĐÃ KHÔI PHỤC ngày 2026-10-16)

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
