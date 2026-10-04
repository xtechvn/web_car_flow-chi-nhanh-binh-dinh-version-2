# Sơ đồ Sequence – Web Car Flow

> Đi kèm [`README.md`](./README.md). Các bước có nhãn **[NGOÀI REPO]** là suy luận từ code gọi tới,
> source thực tế nằm ở project `Web.Cargill.Api` / `App_Push_Consummer` (không có trong repo này).

Mục lục:

1. [Tài xế mở form đăng ký](#1-tài-xế-mở-form-đăng-ký)
2. [Tài xế gửi đăng ký (registerV2) – luồng chính end-to-end](#2-tài-xế-gửi-đăng-ký-registerv2--luồng-chính-end-to-end)
3. [Màn hình realtime /ListData của FE](#3-màn-hình-realtime-listdata-của-fe)
4. [Nhân viên đăng nhập CMS](#4-nhân-viên-đăng-nhập-cms)
5. [Mở một màn hình trạm trong CMS](#5-mở-một-màn-hình-trạm-trong-cms)
6. [Cập nhật trạng thái xe qua các trạm (UpdateStatus)](#6-cập-nhật-trạng-thái-xe-qua-các-trạm-updatestatus)
7. [Gọi loa – sinh audio mời xe vào cân](#7-gọi-loa--sinh-audio-mời-xe-vào-cân)
8. [Cấu hình giờ reset ngày làm việc](#8-cấu-hình-giờ-reset-ngày-làm-việc)
9. [Luồng đăng ký cũ register-V1 (Google + Zalo)](#9-luồng-đăng-ký-cũ-register-v1-google--zalo)
10. [Ghi log lỗi](#10-ghi-log-lỗi)

---

## 1. Tài xế mở form đăng ký

```mermaid
sequenceDiagram
    autonumber
    actor D as Tài xế (Browser)
    participant FE as XTECH-FRONTEND<br/>HomeController.Index
    participant API as Web.Cargill.Api<br/>[NGOÀI REPO]
    participant TG as Telegram

    D->>FE: GET /
    FE->>API: GET http://api.longancargill.com/api/vehicleInspection/get-time-countdown?LocationType=3
    alt 200 OK
        API-->>FE: { data: "dd/MM/yyyy HH:mm:ss" } giờ chốt sổ
        FE->>FE: ViewBag.Data = data
    else Lỗi
        FE->>TG: InsertLogTelegram("Insert - lỗi")
    end
    FE-->>D: HTML Views/Home/Index.cshtml (form + giờ đếm ngược)
    Note over D: JS so sánh giờ hiện tại với ViewBag.Data<br/>để khóa/mở nút GỬI ĐĂNG KÝ
```

---

## 2. Tài xế gửi đăng ký (registerV2) – luồng chính end-to-end

```mermaid
sequenceDiagram
    autonumber
    actor D as Tài xế (Browser)
    participant FEAPI as FE CarRegistrationController<br/>POST api/CarRegistration/registerV2
    participant R as Redis (db 15)
    participant M as MongoDB<br/>CarLog.Data_Car_Binh_Dinh
    participant FEHub as FE SignalR<br/>/registrationHub
    participant Q as InsertQueue (in-memory)
    participant W as InsertWorker<br/>(BackgroundService)
    participant API as Web.Cargill.Api<br/>[NGOÀI REPO]
    participant SQL as SQL Server<br/>VehicleInspection
    participant SUB as CMS RedisSubscriberService
    participant CMSHub as CMS SignalR /CarHub
    participant CMSUI as CMS: CartoFactory /<br/>RegisteredVehicle

    D->>FEAPI: fetch POST https://cargillbinhdinh.com/api/carregistration/registerV2<br/>{plateNumber, phoneNumber, Name, Referee, GPLX, Camp, Timedow}
    FEAPI->>R: GET BinhDinh_PlateNumber_{BS}{dd_MM_yyyy}
    alt Đã đăng ký trong 15 phút
        R-->>FEAPI: bản ghi cũ
        FEAPI-->>D: 400 "đã đăng ký số X, đợi 15 phút"
    else Chưa đăng ký
        FEAPI->>R: INCR counter:daily_car_count_Pro_Binh_Dinh
        R-->>FEAPI: queueNumber
        opt queueNumber == 1 (lần đầu trong ngày)
            FEAPI->>R: EXPIRE counter đến (Timedow - 5 phút)
        end
        FEAPI->>R: SET BinhDinh_PlateNumber_... TTL 15 phút
        FEAPI->>M: InsertOne(RegistrationRecord)
        opt Insert trả 0
            FEAPI->>M: InsertOne (retry 1 lần)
        end
        FEAPI->>FEHub: Clients.All "ReceiveRegistration_FE"
        opt Xử lý > 1000ms
            FEAPI->>FEAPI: ghi Logs/slow_requests.log + Telegram
        end
        FEAPI->>Q: EnqueueAsync(InsertJob, LocationType=3)
        FEAPI-->>D: 200 {Success, QueueNumber, RegistrationTime, ZaloStatus="Đang xử lý..."}

        Q-->>W: DequeueAsync (Task.Run riêng)
        W->>API: POST http://api.longancargill.com/api/vehicleInspection/insert
        alt Thành công
            API->>SQL: INSERT VehicleInspection
            API->>R: PUBLISH Add_ReceiveRegistration_BinhDinh
            API->>R: PUBLISH Add_ReceiveRegistration_BinhDinh_DK
            R-->>SUB: message RegistrationRecord
            SUB->>SQL: SP_GetDetailtVehicleInspection(record.Id)
            SQL-->>SUB: TrangThai
            SUB->>CMSHub: "ReceiveRegistration" / "ReceiveRegistration_DK"
            CMSHub-->>CMSUI: thêm dòng xe mới (WebSocket)
        else Exception
            W->>R: GET/SET CARGLL_BinhDinh (lưu job lỗi để xử lý lại)
            W->>W: LogHelper.InsertLogTelegram
        end
    end
```

---

## 3. Màn hình realtime /ListData của FE

```mermaid
sequenceDiagram
    autonumber
    actor U as Người xem
    participant FE as FE HomeController.ListData
    participant M as MongoDB
    participant Hub as FE SignalR /registrationHub

    U->>FE: GET /ListData
    FE->>M: Find RegistrationTime >= 17:55 (hôm qua hoặc hôm nay), sort QueueNumber
    M-->>FE: danh sách
    FE-->>U: HTML bảng xe
    U->>Hub: connect /registrationHub
    loop Mỗi lần có xe đăng ký mới (luồng 2)
        Hub-->>U: "ReceiveRegistration_FE"(record) → thêm dòng
    end
```

---

## 4. Nhân viên đăng nhập CMS

```mermaid
sequenceDiagram
    autonumber
    actor S as Nhân viên
    participant AC as CMS AccountController
    participant UR as UserRepository / DAL
    participant SQL as SQL Server
    participant R as Redis

    S->>AC: GET /Account/Login
    AC-->>S: form login (account.js)
    S->>AC: POST /Account/ConfirmLogin {UserName, Password}
    AC->>AC: lọc ký tự đặc biệt
    AC->>UR: CheckExistAccount
    UR->>SQL: query User
    SQL-->>UR: user
    alt Sai tài khoản / bị khóa (Status != 0)
        AC-->>S: {status: FAILED, msg}
    else Hợp lệ
        AC->>UR: GetUserPermissionById, GetListUserByUserId
        UR->>SQL: SP_GetListUserPermissionByUserId...
        AC->>AC: SignInAsync cookie "Net.Security.Cookie"<br/>claims: Id, Name, DepartmentId, Email, Role
        AC-->>S: {status: SUCCESS, direct: ReturnUrl}
    end
    Note over AC,R: Luồng SSO: GET /login/token/{token} giải mã bằng key api_manual,<br/>kiểm tra hạn LoginTokenExprire rồi tạo cookie.<br/>ManagementUser cache quyền vào Redis USER_ROLE{id}_{CompanyType}
```

---

## 5. Mở một màn hình trạm trong CMS

Ví dụ màn `Gọi xe vào máng` – các màn khác (CartoFactory, ProcessingIsLoading, CallTheScale, WeighedInput, ListVehicles, WaitingRoom) giống hệt, chỉ khác URL/SP.

```mermaid
sequenceDiagram
    autonumber
    actor S as Nhân viên (Browser)
    participant C as CMS ListCarController
    participant Auth as CustomAuthorize
    participant Repo as VehicleInspectionRepository
    participant SQL as SQL Server
    participant Hub as CMS SignalR /CarHub

    S->>C: GET /ListCar/CarCallList
    C->>Auth: kiểm tra cookie
    alt Chưa đăng nhập
        Auth-->>S: redirect /Account/RedirectLogin
    end
    C-->>S: layout + cartcalllist.js
    S->>Hub: WebSocket connect /CarHub (skipNegotiation)
    S->>C: AJAX POST /ListCar/ListCarCallView (SearchModel)
    C->>Repo: GetListVehicleCarCallList
    Repo->>SQL: AllCode TIME_RESET (xác định ngày làm việc)
    Repo->>SQL: SP_GetListVehicleCarCallList
    loop từng xe
        C->>Repo: GetListTroughWeightByVehicleInspectionId
        Repo->>SQL: SP_GetListTroughWeightByVehicleInspectionId
    end
    C-->>S: PartialView bảng xe
    Note over S,Hub: Từ đây màn hình cập nhật bằng event SignalR,<br/>không cần reload
```

---

## 6. Cập nhật trạng thái xe qua các trạm (UpdateStatus)

```mermaid
sequenceDiagram
    autonumber
    actor S as Nhân viên trạm X
    participant CC as CMS CarController.UpdateStatus
    participant Repo as VehicleInspectionRepository
    participant AC as AllCodeRepository
    participant SQL as SQL Server
    participant Hub as CMS SignalR /CarHub
    actor O as Các màn hình khác + TV

    S->>CC: AJAX POST /Car/UpdateStatus {id, status, type, weight, Note}
    CC->>Repo: GetDetailtVehicleInspection(id)
    Repo->>SQL: SP_GetDetailtVehicleInspection
    SQL-->>CC: detail
    CC->>CC: validate (đã HTTC? trạng thái không đổi? máng đã có?)
    alt Không hợp lệ
        CC-->>S: {status: ERROR, msg}
    else Hợp lệ
        CC->>Repo: UpdateCar(model)
        Repo->>SQL: sp_UpdateVehicleInspection
        opt type 6 Hoàn thành / Ngắt máng
            CC->>Repo: InsertTroughWeight
            Repo->>SQL: SP_InsertTroughWeight
        end
        opt type 2 / 8 có weight
            CC->>Repo: UpdateVehicleLoadTaken
            Repo->>SQL: SP_UpdateVehicleLoadTaken
        end
        CC->>AC: GetListSortByName(AllCodeType...) lấy tên trạng thái
        AC->>SQL: AllCode
        CC->>Hub: Clients.All.SendAsync(event theo type, detail)
        Hub-->>O: event realtime → di chuyển dòng xe giữa các bảng
        CC-->>S: {status: SUCCESS}
    end
```

Bảng `type` → event SignalR:

| type | Trạm | Trường cập nhật | Event |
|---|---|---|---|
| 1 | Bảo vệ – xe đến NM | `VehicleStatus`, `VehicleArrivalDate`, `ProtectNotes` | `ListCartoFactory_Da_SL` (đã đến) / `ListCartoFactory_rac` / `ListCartoFactory` |
| 2 | CS – phân loại | `LoadType`, `Rank`, `VehicleLoadTaken` | `ListProcessingIsLoading` |
| 3 | Gọi vào cân | `VehicleWeighingType`, `VehicleWeighingTimeComeIn` | `ListCallTheScale_Da_SL` / `_0` (Xanh, Đỏ) / `_1` |
| 4 | Gọi vào máng | `TroughType`, `VehicleTroughStatus=Đã gọi` | `UpdateMangStatus` + `ListCarCall` |
| 5 | – | `VehicleTroughWeight` | `ListCartoFactory` |
| 6 | Máng | `VehicleTroughStatus` (Bốc hàng/Hoàn thành/Bỏ lượt/Ngắt máng) + `TroughWeight` | `ListCarCall_Da_SL` / `ListCarCall_Bo_LUOT` / `ListCarCall` |
| 7 | Cân ra | `VehicleWeighingStatus`, `VehicleWeighingTimeComplete` | `ListVehicles_Da_SL` / `ListVehicles` |
| 8 | CS – hoàn thành thủ tục | `LoadingStatus`, `ProcessingIsLoadingDate` | `ListProcessingIsLoading_Da_SL` / `ListProcessingIsLoading` |
| 9 | Cân đầu vào | `VehicleWeighedstatus`, `VehicleWeighingTimeComeOut` | `ListWeighedInput_Da_SL` / `ListWeighedInput` |
| 10 / 11 / 12 | CS | `LoadingType` / `Rank` / `CSNotes` | `ProcessingIsLoading_khoa` |
| 13 | Máng | `Note` | `ListCarCall` |

Luồng nghiệp vụ một xe (thứ tự trạm):

```mermaid
sequenceDiagram
    autonumber
    participant DK as Đăng ký (FE)
    participant BV as Bảo vệ<br/>/Car/CartoFactory
    participant CS as CS<br/>/Car/ProcessingIsLoading
    participant CV as Gọi cân<br/>/Car/CallTheScale
    participant CD as Cân đầu vào<br/>/Car/WeighedInput
    participant MG as Máng<br/>/ListCar/CarCallList
    participant CR as Cân ra<br/>/ListCar/ListVehicles

    DK->>BV: ReceiveRegistration (xe mới)
    BV->>CS: type 1 = Đã đến NM → ListCartoFactory_Da_SL
    CS->>CS: type 2/10/11/12 phân loại, ghi chú
    CS->>CV: type 8 = Đã HTTC → ListProcessingIsLoading_Da_SL
    CV->>CD: type 3 = Đã vào cân → ListCallTheScale_Da_SL
    CD->>MG: type 9 = Đã cân xong đầu cao → ListWeighedInput_Da_SL
    MG->>MG: type 4 chọn máng, type 6 Bốc hàng
    MG->>CR: type 6 = Hoàn thành → ListCarCall_Da_SL
    CR->>CR: type 7 = Đã cân ra → ListVehicles_Da_SL (kết thúc)
```

---

## 7. Gọi loa – sinh audio mời xe vào cân

```mermaid
sequenceDiagram
    autonumber
    actor S as Nhân viên
    participant CC as CMS CarController.UpdateName
    participant Repo as VehicleInspectionRepository
    participant SQL as SQL Server
    participant MQ as RabbitMQ<br/>QUEUE_AUDIO_TEXT_TO_SPEED
    participant TTS as App_Push_Consummer<br/>[NGOÀI REPO]
    participant UI as /Car/CallTheScale

    S->>CC: POST /Car/UpdateName {id, name?, VehicleNumber?}
    opt có VehicleNumber
        CC->>Repo: GetAudioPathByVehicleNumber
        Repo->>SQL: SP_GetDetailVehicleAudioByPlateNumber
        SQL-->>CC: AudioPath (có thể rỗng)
    end
    CC->>Repo: UpdateCar
    Repo->>SQL: sp_UpdateVehicleInspection
    alt Biển số mới chưa có audio
        CC->>MQ: publish {Id, Type=2, PlateNumber,<br/>text_voice="Mời biển số xe X vào cân"} (retry 1 lần)
        MQ-->>TTS: consume
        TTS->>SQL: lưu VehicleAudio / AudioPath (suy luận)
    end
    CC-->>S: {status: SUCCESS}
    UI->>UI: nút loa phát thẻ audio với src = AudioPath
```

---

## 8. Cấu hình giờ reset ngày làm việc

```mermaid
sequenceDiagram
    autonumber
    actor A as Admin
    participant CF as CMS ConfigurationController
    participant AC as AllCodeRepository
    participant SQL as SQL Server
    participant R as Redis
    participant FE as FE registerV2

    A->>CF: GET /Configuration
    CF->>AC: GetListSortByName(TIME_RESET)
    CF-->>A: giờ reset hiện tại
    A->>CF: POST /Configuration/Setup {time: "HH:mm", id}
    alt time < hiện tại
        CF-->>A: FAILED "chọn thời gian lớn hơn hiện tại"
    else
        CF->>AC: UpdateResetTime(id, date_time)
        AC->>SQL: UPDATE AllCode TIME_RESET
        CF->>R: EXPIRE counter:daily_car_count_Pro_Long_An (giờ - 5 phút)
        CF-->>A: SUCCESS
    end
    Note over R,FE: LỆCH KEY: FE dùng counter:daily_car_count_Pro_Binh_Dinh<br/>→ đổi giờ ở CMS KHÔNG reset counter của FE.<br/>Mọi list/báo cáo CMS đọc TIME_RESET để xác định ngày.
```

---

## 9. Luồng đăng ký cũ register-V1 (Google + Zalo)

Không còn được form gọi, giữ lại để tham khảo.

```mermaid
sequenceDiagram
    autonumber
    actor D as Client
    participant API as FE CarRegistrationController<br/>POST register-V1
    participant V as ValidationService (MemoryCache)
    participant GS as Google Sheets API
    participant GF as Google Forms
    participant Z as Zalo OA openapi.zalo.me
    participant M as MongoDB

    D->>API: POST /api/CarRegistration/register-V1
    API->>V: ValidateCarRegistration + CheckTimeRestriction(15 phút)
    API->>GS: Values.Get A:H → đếm số thứ tự trong ngày
    API->>GF: submit formResponse
    API->>Z: gửi tin nhắn đăng ký thành công
    API->>M: InsertOne
    API->>GS: Values.Append + BatchUpdate format dòng
    API-->>D: QueueNumber, ZaloStatus
```

---

## 10. Ghi log lỗi

```mermaid
sequenceDiagram
    participant Any as FE / CMS / DbWorker
    participant LH as LogHelper.InsertLogTelegram
    participant TG as api.telegram.org

    Any->>LH: message lỗi (exception, SP lỗi kèm tham số, request chậm)
    LH->>LH: đọc appsettings.json BotSetting (bot_token, bot_group_id, environment)
    LH->>TG: sendMessage "[environment] - message"
```
