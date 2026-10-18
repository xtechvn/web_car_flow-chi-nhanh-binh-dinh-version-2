# CHANGELOG – WEB.CMS

Mỗi lần thay đổi code phải thêm một mục mới ở đầu file: ngày, mục đích, danh sách file, lưu ý vận hành.

---

## 2026-10-17 – WeighedInput: multi-SKU Hàng xá + cột đúng wireframe

**Mục đích:** Chọn **Hàng xá** → nhập nhiều SKU (cân nặng thực tế) trên 1 dòng xe; thêm/xóa SKU; lưu DB. Sắp cột theo wireframe (13 cột).

**DB:** `Scripts/vehicle_weigh_in_sku.sql` — bảng `VehicleWeighInSku` + SP get/insert/update/delete (đã apply).

**Rule:** `GoodsType = 1` (Hàng xá) bật panel SKU; `= 0` (Hàng bao) giữ 1 ô `VehicleWeightIn`. Tổng SKU đồng bộ `VehicleWeightIn`.

**Cột:** STT | Giờ xử lý đăng tải | Biển số | OBD | Net cân đầu vào/SKU | Ship-to Party | Net Weight OBD | Tải ĐK | Check | Nhóm tài | Ghi chú | Loại hàng/Tiến độ | Tình trạng.

---

## 2026-10-17 – WeighedInput bổ sung theo wireframe

**Mục đích:** Giữ bảng cũ, bổ sung cột wireframe: OBD widget chung, Ship-to Party (= CustomerName), Net weight OBD, tải đăng kiểm, Check tải trọng, ghi chú, Loại hàng (Hàng bao/Hàng xá).

**DB:** chạy `Scripts/weighed_input_goods_type.sql` (đã apply trên Cargill_BinhDuong_v2): cột `VehicleInspection.GoodsType`, AllCode `GOODS_TYPE`, mở rộng `SP_GetListVehicleWeighedInput` + `sp_UpdateVehicleInspection`.

**Sửa / thêm**

| File | Thay đổi |
|---|---|
| `wwwroot/modules/obd-token.js` | Widget OBD dùng chung CartoFactory + WeighedInput |
| `Views/Car/ListWeighedInput.cshtml`, `WeighedInput.cshtml`, `weighedinput.js` | Cột mới + select GoodsType + Check |
| `cartofactory.js` / `CartoFactory.cshtml` | Gọi `_obdToken` |
| Entity/DAL/Repo/Controller | `GoodsType`, AttachObdList WeighedInput, UpdateStatus type=14 |

**Lưu ý:** Deploy WEB.CMS; Ctrl+F5. Tiến độ multi-SKU để sau (placeholder HTML).

---

## 2026-10-16 – CartoFactory: cột OBD nhận hàng (multi)

**Mục đích:** Gắn nhiều mã OBD vào từng xe ở bảng Chưa xử lý; suggest từ ObdSetting (ngày nghiệp vụ + version mới nhất); tạo mới NetWeight=0; lưu bảng quan hệ.

**Thêm mới**

| File | Nội dung |
|---|---|
| `Scripts/vehicle_inspection_obd.sql` | Bảng `VehicleInspectionObd` + SP suggest/get/attach/detach |
| `Entities/ViewModels/Car/VehicleInspectionObdModel.cs` | Model gắn OBD / suggest |

**Sửa**

| File | Thay đổi |
|---|---|
| `DAL/ObdConfigDAL.cs`, `Repositories/*ObdConfig*` | SuggestLatest, GetByVehicleIds, Attach/Detach |
| `Repositories/VehicleInspectionRepository.cs` | Load OBD khi list + Export Excel thêm cột |
| `Controllers/CarController.cs` | `SuggestObd`, `AttachVehicleObd`, `DetachVehicleObd` |
| `Views/Car/ListCartoFactory.cshtml`, `cartofactory.js`, `cms-list-page.css` | Token input cột **OBD nhận hàng** |

**Vận hành:** Chạy script `Scripts/vehicle_inspection_obd.sql` trên DB trước khi deploy CMS.

## 2026-10-16 – Chỉnh layout section + nút Xuất Excel

**Sửa**

| File | Thay đổi |
|---|---|
| `wwwroot/css/cms-list-page.css` | Title/search cùng line, cùng cỡ chữ; `th` `vertical-align: middle`; bỏ margin lệch |
| `Views/Car/*.cshtml`, `Views/ListCar/*.cshtml` | Mỗi bảng: `cms-head` (tên trái + search phải) rồi `cms-table` |
| `Views/Car/CartoFactory.cshtml` + SummaryReport | Nút **Xuất Excel**: icon trái + chữ phải |

## 2026-10-16 – Quy chuẩn UI danh sách CMS (theo mẫu ObdSetting)

**Mục đích:** Một style dùng chung cho các màn danh sách xe; ObdSetting giữ nguyên style riêng.

**Thêm mới**

| File | Nội dung |
|---|---|
| `wwwroot/css/cms-list-page.css` | `cms-page` / `cms-head` / `cms-table` / pin cột; load qua `_Layout` |

**Sửa**

| File | Thay đổi |
|---|---|
| `Views/Shared/_Layout.cshtml` | Link `cms-list-page.css` |
| `Views/Car/CartoFactory.cshtml`, `RegisteredVehicle.cshtml` | Áp dụng `cms-*`; bỏ scroll dọc cố định |
| `Views/Car/ListCartoFactory.cshtml`, `ListRegisteredVehicle.cshtml` | Pin STT/giờ/biển số; RegisteredVehicle sort mới nhất trước |
| `Views/Car/ProcessingIsLoading.cshtml`, `WeighedInput.cshtml`, `CallTheScale.cshtml` | Cùng layout `cms-*` |
| `Views/ListCar/ListVehicles.cshtml`, `CarCallList.cshtml` | Cùng layout `cms-*` |
| `Controllers/CarController.cs` | `ListRegisteredVehicle` / `ListCartoFactory`: `OrderByDescending(RegisterDateOnline)` |
| `wwwroot/modules/cartofactory.js`, `registeredvehicle.js` | Sort DESC + cột pin |

---

## 2026-10-16 – Rule cut-off: sau TIME_RESET chỉ hiện ở Danh sách xe đăng ký đến 00:00

**Mục đích:** Xe đăng ký sau giờ cut-off (AllCode.TIME_RESET, vd 17:23) không còn nhảy realtime sang
CartoFactory (trang có nút Excel, dù title UI ghi "Danh sách xe đăng ký"). Chỉ hiện ở `Car/RegisteredVehicle`
đến hết ngày; sau 00:00 ngày hôm sau mới vào CartoFactory qua SQL.

**Nguyên nhân vẫn thấy nhảy:** CMS chưa deploy bản publish theo kênh `_DK`, hoặc publisher cũ còn đẩy kênh CartoFactory.

**Sửa (chặn 3 lớp)**

| File | Thay đổi |
|---|---|
| `Services/RegistrationImportService.cs` | Sau cut-off publish Redis `_DK`; trước cut-off publish kênh CartoFactory |
| `Services/RedisSubscriberService.cs` | Nếu nhận nhầm trên kênh CartoFactory mà giờ ≥ cut-off → chuyển `ReceiveRegistration_DK`, không gửi `ReceiveRegistration` |
| `Controllers/CarController.cs` + `Views/Car/CartoFactory.cshtml` | Truyền `CART_CUTOFF_TIME` từ AllCode |
| `wwwroot/modules/cartofactory.js` | Bỏ qua SignalR `ReceiveRegistration` nếu giờ đăng ký ≥ cut-off |
| `appsettings.json` | `RealtimeChannelRegistered` |

**Lưu ý vận hành:** **Bắt buộc publish lại WEB.CMS + restart** (BackgroundService `RedisSubscriberService` chỉ nạp lúc start). Hard refresh (Ctrl+F5) trang CartoFactory.

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
