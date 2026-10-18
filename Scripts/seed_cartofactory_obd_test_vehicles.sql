-- =============================================================================
-- Seed 3 xe test cho /Car/CartoFactory (Chưa xử lý) để thử cột OBD nhận hàng.
-- Đồng thời seed 3 mã OBD 90000001..03 (nếu chưa có).
-- Chạy lại được.
-- Lưu ý: tạm đổi RecordNumber của xe Id=7958 từ 1 → 9001 nếu đang chặn cửa sổ.
-- =============================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.fn_GetObdBusinessDate', N'FN') IS NULL
BEGIN
    RAISERROR(N'Chạy Scripts/obd_config.sql trước.', 16, 1);
    RETURN;
END

BEGIN TRAN;

-- Tránh RN=1 sau cut-off làm Prev > Cutoff (cửa sổ rỗng)
IF EXISTS (SELECT 1 FROM dbo.VehicleInspection WHERE Id = 7958 AND RecordNumber = 1)
    UPDATE dbo.VehicleInspection SET RecordNumber = 9001 WHERE Id = 7958;

DECLARE @Cutoff DATETIME =
    CAST(CAST(GETDATE() AS DATE) AS DATETIME)
    + ISNULL(
        (SELECT TOP 1 CAST(CONVERT(varchar(5), UpdateTime, 108) AS DATETIME)
         FROM dbo.AllCode WHERE [Type] = 'TIME_RESET' AND UpdateTime IS NOT NULL ORDER BY OrderNo, Id),
        CAST('17:55' AS DATETIME)
      );

DECLARE @BatchStart DATETIME = DATEADD(HOUR, -23, @Cutoff); -- trước cut-off hôm nay
DECLARE @T1 DATETIME = DATEADD(HOUR, -7, @Cutoff);
DECLARE @T2 DATETIME = DATEADD(HOUR, -6, @Cutoff);
DECLARE @T3 DATETIME = DATEADD(HOUR, -5, @Cutoff);

MERGE dbo.VehicleInspection AS t
USING (VALUES
    (1, N'TESTOBD-BATCH', @BatchStart, N'TEST OBD BATCH'),
    (2, N'TESTOBD01', @T1, N'TEST OBD KH TESTOBD01'),
    (3, N'TESTOBD02', @T2, N'TEST OBD KH TESTOBD02'),
    (4, N'TESTOBD03', @T3, N'TEST OBD KH TESTOBD03')
) AS s(RecordNumber, VehicleNumber, RegisterDateOnline, CustomerName)
ON t.VehicleNumber = s.VehicleNumber
WHEN MATCHED THEN UPDATE SET
    RecordNumber = s.RecordNumber,
    RegisterDateOnline = s.RegisterDateOnline,
    CustomerName = s.CustomerName,
    VehicleStatus = 1,
    DriverName = N'Tài xế ' + s.VehicleNumber,
    PhoneNumber = N'091100000' + CAST(s.RecordNumber AS varchar(2)),
    VehicleLoad = N'15000'
WHEN NOT MATCHED THEN INSERT
    (RecordNumber, CustomerName, VehicleNumber, RegisterDateOnline, DriverName, LicenseNumber, PhoneNumber, VehicleLoad, VehicleStatus, CreatedDate)
VALUES
    (s.RecordNumber, s.CustomerName, s.VehicleNumber, s.RegisterDateOnline,
     N'Tài xế ' + s.VehicleNumber, N'note-test', N'091100000' + CAST(s.RecordNumber AS varchar(2)), N'15000', 1, GETDATE());

-- 3 mã OBD test
DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(GETDATE());
DECLARE @Version INT = ISNULL((SELECT MAX([Version]) FROM dbo.ObdConfig WHERE LoadingDate = @LoadingDate), 1);

INSERT INTO dbo.ObdConfig (Delivery, NetWeight, LoadingDate, [Version], UserCreateID, CreateDate)
SELECT v.Delivery, 0, @LoadingDate, @Version, 0, GETDATE()
FROM (VALUES (CAST(90000001 AS BIGINT)), (90000002), (90000003)) v(Delivery)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ObdConfig o
    WHERE o.LoadingDate = @LoadingDate AND o.[Version] = @Version AND o.Delivery = v.Delivery
);

COMMIT;

SELECT Id, RecordNumber, VehicleNumber, RegisterDateOnline, VehicleStatus
FROM dbo.VehicleInspection
WHERE VehicleNumber LIKE N'TESTOBD%'
ORDER BY RegisterDateOnline;

SELECT ObdId, Delivery, NetWeight, LoadingDate, [Version]
FROM dbo.ObdConfig
WHERE Delivery IN (90000001, 90000002, 90000003)
ORDER BY Delivery;
