-- =============================================================================
-- Seed 3 mã OBD test cho suggest/gắn trên /Car/CartoFactory
-- Thuộc ngày nghiệp vụ hiện tại + version mới nhất (chưa có thì tạo version 1).
-- Chạy lại được: bỏ qua nếu Delivery đã tồn tại trong cùng LoadingDate + Version.
-- =============================================================================

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.fn_GetObdBusinessDate', N'FN') IS NULL
BEGIN
    RAISERROR(N'Thiếu fn_GetObdBusinessDate — chạy Scripts/obd_config.sql trước.', 16, 1);
    RETURN;
END

IF OBJECT_ID(N'dbo.ObdConfig', N'U') IS NULL
BEGIN
    RAISERROR(N'Thiếu bảng ObdConfig — chạy Scripts/obd_config.sql trước.', 16, 1);
    RETURN;
END

DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(GETDATE());
DECLARE @Version INT;
DECLARE @Now DATETIME = GETDATE();
DECLARE @UserId INT = 0;

SELECT @Version = MAX([Version]) FROM dbo.ObdConfig WHERE LoadingDate = @LoadingDate;
IF @Version IS NULL SET @Version = 1;

-- Mã test riêng (tránh trùng dữ liệu thật kiểu 83482937 đã import sẵn)
DECLARE @Seed TABLE (Delivery BIGINT NOT NULL, NetWeight DECIMAL(18,3) NOT NULL);
INSERT INTO @Seed (Delivery, NetWeight) VALUES
    (90000001, 0),
    (90000002, 0),
    (90000003, 0);

INSERT INTO dbo.ObdConfig (Delivery, NetWeight, LoadingDate, [Version], UserCreateID, CreateDate)
SELECT s.Delivery, s.NetWeight, @LoadingDate, @Version, @UserId, @Now
FROM @Seed s
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.ObdConfig o
    WHERE o.LoadingDate = @LoadingDate
      AND o.[Version] = @Version
      AND o.Delivery = s.Delivery
);

INSERT INTO dbo.ObdConfigLog (ObdId, [Action], LoadingDate, [Version], FieldName, NewValue, UserId, CreateDate)
SELECT o.ObdId, 2, o.LoadingDate, o.[Version], 'Delivery|NetWeight',
       CAST(o.Delivery AS NVARCHAR(30)) + N'|0', @UserId, @Now
FROM dbo.ObdConfig o
INNER JOIN @Seed s ON s.Delivery = o.Delivery
WHERE o.LoadingDate = @LoadingDate
  AND o.[Version] = @Version
  AND NOT EXISTS (
      SELECT 1 FROM dbo.ObdConfigLog l
      WHERE l.ObdId = o.ObdId AND l.[Action] = 2
  );

SELECT o.ObdId, o.Delivery, o.NetWeight, o.LoadingDate, o.[Version], o.CreateDate
FROM dbo.ObdConfig o
INNER JOIN @Seed s ON s.Delivery = o.Delivery
WHERE o.LoadingDate = @LoadingDate AND o.[Version] = @Version
ORDER BY o.Delivery;
