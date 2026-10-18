-- =============================================================================
-- Seed OBD demo cho /ListCar/CarCallList (cột OBD/Tải trọng chỉ show)
-- 1) Cập nhật NetWeight mã OBD ngày nghiệp vụ hiện tại
-- 2) Gắn OBD vào xe đang có trên list (cần đổi @Vid)
-- Chạy lại được.
-- =============================================================================
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.fn_GetObdBusinessDate', N'FN') IS NULL
BEGIN
    RAISERROR(N'Chạy Scripts/obd_config.sql trước.', 16, 1);
    RETURN;
END

-- Xe đang gọi máng (tham khảo — chọn Id rồi gán @Vid bên dưới)
SELECT TOP 20
    v.Id, v.VehicleNumber, v.VehicleTroughStatus, v.VehicleWeighingTimeComeOut, v.VehicleLoadTaken
FROM dbo.VehicleInspection v
WHERE v.VehicleWeighingTimeComeOut IS NOT NULL
ORDER BY v.VehicleWeighingTimeComeOut DESC;

DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(GETDATE());
DECLARE @Version INT = ISNULL((SELECT MAX([Version]) FROM dbo.ObdConfig WHERE LoadingDate = @LoadingDate), 1);
DECLARE @Vid INT = 0; -- <-- thay bằng Id xe trên list

IF @Vid <= 0
BEGIN
    RAISERROR(N'Set @Vid = Id xe từ SELECT ở trên rồi chạy lại phần dưới.', 16, 1);
    RETURN;
END

MERGE dbo.ObdConfig AS t
USING (VALUES
    (CAST(90000011 AS BIGINT), CAST(12000.000 AS DECIMAL(18,3))),
    (90000012, 8500.000),
    (90000013, 15200.000)
) AS s(Delivery, NetWeight)
ON t.LoadingDate = @LoadingDate AND t.[Version] = @Version AND t.Delivery = s.Delivery
WHEN MATCHED THEN UPDATE SET NetWeight = s.NetWeight
WHEN NOT MATCHED THEN INSERT (Delivery, NetWeight, LoadingDate, [Version], UserCreateID, CreateDate)
VALUES (s.Delivery, s.NetWeight, @LoadingDate, @Version, 0, GETDATE());

INSERT INTO dbo.VehicleInspectionObd (VehicleInspectionId, ObdId, Delivery, UserCreateID, CreateDate)
SELECT @Vid, o.ObdId, o.Delivery, 0, GETDATE()
FROM dbo.ObdConfig o
WHERE o.LoadingDate = @LoadingDate AND o.[Version] = @Version
  AND o.Delivery IN (90000011, 90000012)
  AND NOT EXISTS (
      SELECT 1 FROM dbo.VehicleInspectionObd x
      WHERE x.VehicleInspectionId = @Vid AND x.Delivery = o.Delivery
  );

SELECT v.VehicleInspectionId, v.Delivery, o.NetWeight, o.ObdId
FROM dbo.VehicleInspectionObd v
INNER JOIN dbo.ObdConfig o ON o.ObdId = v.ObdId
WHERE v.VehicleInspectionId = @Vid;
