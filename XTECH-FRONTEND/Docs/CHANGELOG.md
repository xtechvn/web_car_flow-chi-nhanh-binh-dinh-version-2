# CHANGELOG – XTECH-FRONTEND

Mỗi lần thay đổi code phải thêm một mục mới ở đầu file: ngày, mục đích, danh sách file, lưu ý vận hành.

---

## 2026-10-17 – Fallback cấp số thứ tự khi Redis lỗi (Mongo)

**Mục đích:** Tắt Redis vẫn đăng ký được; bật lại Redis thì số tiếp tục từ max đã cấp (không trùng/nhảy lùi).

**Cơ chế**

1. Ưu tiên Redis Lua: sync `floor` từ Mongo → rate limit → khóa biển → `INCR`.
2. Redis lỗi / mất kết nối → Mongo atomic counter (`RegistrationCounters`) + khóa biển (`RegistrationPlateLocks`).
3. Mỗi lần Redis cấp số thành công → `EnsureCounterAtLeast` Mongo (best-effort) để lần fallback sau không thấp hơn.
4. Publish RabbitMQ lỗi mà Redis outbox cũng fail → vẫn trả thành công cho user; log Telegram (dùng CMS Recovery Mongo→SQL).

**Sửa**

| File | Thay đổi |
|---|---|
| `Services/Registration/RedisQueueNumberAllocator.cs` | Lua thêm `ARGV[5]=floor`; không còn implement `IQueueNumberAllocator` trực tiếp |
| `Services/Registration/MongoQueueNumberStore.cs` | **Mới** – floor/max, allocate fallback, sync counter |
| `Services/Registration/ResilientQueueNumberAllocator.cs` | **Mới** – Redis rồi fallback Mongo |
| `Services/Registration/IQueueNumberAllocator.cs` | `AllocateAsync(..., RegistrationWindow, ...)` |
| `Controllers/API/CarRegistration/CarRegistrationController.cs` | Truyền `window`; bọc outbox khi Redis down |
| `Services/Queue/PendingRegistrationStore.cs` | Kiểm tra `IsConnected` trước khi push |
| `Program.cs` | DI: Redis + Mongo store + `ResilientQueueNumberAllocator` |
| `Docs/README.md` | Mục cấp số / fallback |

**Lưu ý vận hành:** Deploy lại **XTECH-FRONTEND** và restart IIS/app pool. Test: tắt Redis → đăng ký vẫn ra số; bật Redis → số tăng tiếp từ max. Collection Mongo tự tạo: `RegistrationCounters`, `RegistrationPlateLocks` (cùng DB `catalog_log`).

---

## 2026-10-16 – Đẩy lỗi đăng ký lên Telegram chi tiết hơn

**Mục đích:** Khi form báo "Lỗi hệ thống", Telegram nhận đủ biển số / IP / loại exception / stack
(trước đó chỉ gửi `ex.Message` và thường im lặng vì đọc `appsettings.json` sai đường dẫn dưới IIS).

**Sửa**

| File | Thay đổi |
|---|---|
| `Utilities/LogHelper.cs` | Đọc `appsettings.json` từ `BaseDirectory`; trim `bot_group_id`; cắt tin ≤ 4000 ký tự; nếu gửi Telegram lỗi thì ghi `Logs/telegram_fail.log`; thêm `InsertRegistrationError` |
| `Controllers/API/CarRegistration/CarRegistrationController.cs` | `registerV2`: kiểm tra thiếu trường bắt buộc; catch gửi Telegram đầy đủ ngữ cảnh |

**Lưu ý vận hành:** Deploy lại XTECH-FRONTEND. Kiểm tra nhóm Telegram `BotSetting:bot_group_id`. Nếu vẫn không thấy tin, mở `Logs/telegram_fail.log` trên server.

---

## 2026-10-16 – Khôi phục quy tắc gốc của dự án

Bỏ các quy tắc tự thêm ở bản 2026-10-04 mà dự án gốc không có.

| File | Thay đổi |
|---|---|
| `Controllers/API/CarRegistration/CarRegistrationController.cs` | `registerV2` bỏ `ValidateRegistrationV2` và bỏ chặn "chốt sổ 5 phút"; giữ rate limit 20 request/phút/IP |
| `Services/Registration/IRegistrationScheduleService.cs`, `RegistrationScheduleService.cs` | `RegistrationWindow` chỉ còn `BusinessDate`, `PeriodStart`, `PeriodEnd`; số thứ tự về 1 lúc `TIME_RESET − 5 phút` như bản gốc |
| `Services/Registration/RegistrationOptions.cs`, `appsettings.json` | `CloseBeforeResetMinutes` → `CounterResetOffsetMinutes` (5) |
| `Repositories/ValidationService.cs`, `IRepositories/IValidationService.cs` | Chép lại bản gốc (xóa `ValidateRegistrationV2`) |
| `Docs/README.md` | Mục 4, 5, 8, 9 theo quy tắc gốc |

**Lưu ý vận hành:** `appsettings.json` trên server phải đổi key `Registration:CloseBeforeResetMinutes` thành
`Registration:CounterResetOffsetMinutes` (không đổi thì dùng mặc định 5).

Xe bị thiếu (có trong MongoDB, chưa vào SQL) được đẩy lại ở WEB.CMS – xem `source/WEB.CMS/Docs/README.md` mục 4.2.

---

## 2026-10-04 – Consumer chuyển sang n8n + API WEB.CMS (chỉ tài liệu)

Không đổi code XTECH-FRONTEND. Cập nhật `Docs/README.md` mục 1, 4.1, 7: consumer là n8n,
ghi SQL qua `POST /api/registration-queue/receive` của WEB.CMS (xem `source/WEB.CMS/Docs/`).

---

## 2026-10-04 – Chuyển đăng ký sang RabbitMQ, cấp số thứ tự bằng Redis

**Mục đích:** Ngắt luồng gọi API ngoài `api.longancargill.com/api/vehicleInspection/insert`.
Tài xế nhận số thứ tự ngay (Redis `INCR` atomic), dữ liệu đẩy vào RabbitMQ để consumer ghi SQL,
chịu được nhiều người đăng ký đồng thời.

**Thêm mới**

| File | Nội dung |
|---|---|
| `Services/Registration/RegistrationOptions.cs` | Cấu hình section `Registration` |
| `Services/Registration/IRegistrationScheduleService.cs`, `RegistrationScheduleService.cs` | Đọc `AllCode.TIME_RESET` từ SQL (cache 60s), tính ngày làm việc và khoảng chốt sổ |
| `Services/Registration/IQueueNumberAllocator.cs`, `RedisQueueNumberAllocator.cs` | Script Lua atomic: rate limit IP → chống trùng biển số → `INCR` số thứ tự |
| `Services/Queue/IRegistrationQueuePublisher.cs`, `RabbitMqRegistrationPublisher.cs` | Publisher RabbitMQ singleton, queue durable, message persistent, publisher confirm |
| `Services/Queue/PendingRegistrationStore.cs`, `PendingRegistrationRepublisher.cs` | Outbox Redis + job nền publish lại khi RabbitMQ lỗi |
| `Model/RegistrationQueueMessage.cs` | Hợp đồng message gửi vào queue |
| `Docs/README.md`, `Docs/CHANGELOG.md` | Tài liệu dự án |

**Sửa**

| File | Thay đổi |
|---|---|
| `Program.cs` | Đăng ký `IConnectionMultiplexer` singleton, các service mới, job outbox; bỏ `InsertWorker` |
| `Controllers/API/CarRegistration/CarRegistrationController.cs` | Viết lại `registerV2` theo luồng mới; xóa endpoint công khai `POST insert`; bỏ tạo `RedisConn`/`WorkQueueClient` mỗi request |
| `Controllers/HomeController.cs` | Giờ reset đọc từ SQL Bình Dương thay vì gọi `api.longancargill.com/get-time-countdown` |
| `Repositories/ValidationService.cs`, `IRepositories/IValidationService.cs` | Thêm `ValidateRegistrationV2` (biển số, SĐT, độ dài, chặn `< >`) |
| `Repositories/GoogleSheetsService.cs`, `IRepositories/IGoogleSheetsService.cs` | Xóa `GetDailyQueueCountRedis` (tạo kết nối Redis mỗi request, key dùng chung với Bình Định) |
| `Services/MongoService.cs` | Dùng chung 1 `MongoClient` |
| `Views/Home/Index.cshtml` | `API_BASE_URL = '/api'` (trước đây trỏ cứng `https://cargillbinhdinh.com/api`); hiển thị thông báo lỗi do server trả về |
| `appsettings.json` | Thêm `Queue:AmqpPort`, `Queue:QueueRegistration`, section `Registration` |
| `XTECH-FRONTEND.csproj` | Thêm package `Microsoft.Data.SqlClient` 5.1.5 |

**Xóa**

`Services/BackgroundQueue/InsertWorker.cs`, `InsertQueue.cs`, `IInsertQueue.cs`, `Model/InsertJob.cs`,
`Services/WorkQueueClient.cs`, `Services/RedisWorker/RedisConn.cs`.
Bản sao trước khi sửa: `binh_duong/_backup/XTECH-FRONTEND_before_queue_20261004/`.

**Lưu ý vận hành**

- Cần consumer đọc queue `QUEUE_CAR_REGISTRATION_BINH_DUONG` và ghi `VehicleInspection` (xem README mục 7).
  Khi chưa có consumer, CMS sẽ không thấy xe mới.
- Key Redis mới có tiền tố `BINH_DUONG`, không còn dùng chung `counter:daily_car_count_Pro_Binh_Dinh` với Bình Định.
- `appsettings.json` bị `.gitignore`: khi deploy phải bổ sung tay các key mới.
