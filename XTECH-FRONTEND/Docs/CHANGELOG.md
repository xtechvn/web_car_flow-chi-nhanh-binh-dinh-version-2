# CHANGELOG – XTECH-FRONTEND

Mỗi lần thay đổi code phải thêm một mục mới ở đầu file: ngày, mục đích, danh sách file, lưu ý vận hành.

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
