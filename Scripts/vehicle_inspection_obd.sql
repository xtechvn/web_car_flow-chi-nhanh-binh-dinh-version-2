-- =============================================================================
-- Gắn OBD nhận hàng cho xe đăng ký (/Car/CartoFactory – bảng Chưa xử lý)
-- Bảng quan hệ 1 xe – nhiều OBD; cùng 1 OBD có thể gắn nhiều xe.
-- Script chạy lại được.
-- =============================================================================

IF OBJECT_ID(N'dbo.VehicleInspectionObd', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleInspectionObd
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VehicleInspectionObd PRIMARY KEY,
        VehicleInspectionId  INT            NOT NULL,
        ObdId                BIGINT         NOT NULL,
        Delivery             BIGINT         NOT NULL,
        UserCreateID         INT            NULL,
        CreateDate           DATETIME       NOT NULL CONSTRAINT DF_VehicleInspectionObd_CreateDate DEFAULT (GETDATE()),
        CONSTRAINT UQ_VehicleInspectionObd_Vehicle_Obd UNIQUE (VehicleInspectionId, ObdId)
    );

    CREATE INDEX IX_VehicleInspectionObd_Vehicle ON dbo.VehicleInspectionObd (VehicleInspectionId) INCLUDE (ObdId, Delivery);
    CREATE INDEX IX_VehicleInspectionObd_Obd ON dbo.VehicleInspectionObd (ObdId);
END
GO

-- Suggest: ngày nghiệp vụ hiện tại + version mới nhất
IF OBJECT_ID(N'dbo.SP_SuggestObdConfigLatest', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_SuggestObdConfigLatest;
GO
CREATE PROCEDURE dbo.SP_SuggestObdConfigLatest
    @Keyword VARCHAR(20) = NULL,
    @Top     INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Top IS NULL OR @Top < 1 SET @Top = 20;
    IF @Top > 50 SET @Top = 50;
    SET @Keyword = NULLIF(LTRIM(RTRIM(@Keyword)), '');

    DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(GETDATE());
    DECLARE @Version INT;

    SELECT @Version = MAX([Version])
    FROM dbo.ObdConfig
    WHERE LoadingDate = @LoadingDate;

    IF @Version IS NULL
    BEGIN
        SELECT CAST(NULL AS BIGINT) AS ObdId,
               CAST(NULL AS BIGINT) AS Delivery,
               CAST(NULL AS DECIMAL(18,3)) AS NetWeight,
               CAST(NULL AS DATE) AS LoadingDate,
               CAST(NULL AS INT) AS [Version]
        WHERE 1 = 0;
        RETURN;
    END

    -- Group theo mã OBD (Delivery), cộng NetWeight các dòng cùng mã
    SELECT TOP (@Top)
           MAX(o.ObdId) AS ObdId,
           o.Delivery,
           SUM(o.NetWeight) AS NetWeight,
           MAX(o.LoadingDate) AS LoadingDate,
           MAX(o.[Version]) AS [Version]
    FROM dbo.ObdConfig o
    WHERE o.LoadingDate = @LoadingDate
      AND o.[Version] = @Version
      AND (@Keyword IS NULL OR CAST(o.Delivery AS VARCHAR(20)) LIKE '%' + @Keyword + '%')
    GROUP BY o.Delivery
    ORDER BY o.Delivery;
END
GO

-- Lấy OBD đã gắn: 1 dòng / mã Delivery, NetWeight = SUM theo mã ở version mới nhất ngày nghiệp vụ
IF OBJECT_ID(N'dbo.SP_GetVehicleInspectionObdByVehicleIds', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetVehicleInspectionObdByVehicleIds;
GO
CREATE PROCEDURE dbo.SP_GetVehicleInspectionObdByVehicleIds
    @Ids NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(GETDATE());
    DECLARE @Version INT = (SELECT MAX([Version]) FROM dbo.ObdConfig WHERE LoadingDate = @LoadingDate);

    ;WITH Ids AS (
        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT) AS VehicleInspectionId
        FROM STRING_SPLIT(@Ids, ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    WeightByDelivery AS (
        SELECT o.Delivery, SUM(o.NetWeight) AS NetWeight, MAX(o.LoadingDate) AS LoadingDate, MAX(o.[Version]) AS [Version]
        FROM dbo.ObdConfig o
        WHERE @Version IS NOT NULL
          AND o.LoadingDate = @LoadingDate
          AND o.[Version] = @Version
        GROUP BY o.Delivery
    )
    SELECT MIN(v.Id) AS Id,
           v.VehicleInspectionId,
           MAX(v.ObdId) AS ObdId,
           v.Delivery,
           MAX(v.UserCreateID) AS UserCreateID,
           MIN(v.CreateDate) AS CreateDate,
           w.LoadingDate,
           w.[Version],
           ISNULL(w.NetWeight, 0) AS NetWeight
    FROM dbo.VehicleInspectionObd v
    INNER JOIN Ids i ON i.VehicleInspectionId = v.VehicleInspectionId
    LEFT JOIN WeightByDelivery w ON w.Delivery = v.Delivery
    GROUP BY v.VehicleInspectionId, v.Delivery, w.LoadingDate, w.[Version], w.NetWeight
    ORDER BY v.VehicleInspectionId, v.Delivery;
END
GO

IF OBJECT_ID(N'dbo.SP_InsertVehicleInspectionObd', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_InsertVehicleInspectionObd;
GO
CREATE PROCEDURE dbo.SP_InsertVehicleInspectionObd
    @VehicleInspectionId INT,
    @ObdId               BIGINT,
    @UserId              INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Delivery BIGINT;
    DECLARE @NetWeight DECIMAL(18,3) = 0;
    DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(GETDATE());
    DECLARE @Version INT = (SELECT MAX([Version]) FROM dbo.ObdConfig WHERE LoadingDate = @LoadingDate);

    SELECT @Delivery = Delivery
    FROM dbo.ObdConfig
    WHERE ObdId = @ObdId;

    IF @Delivery IS NULL
    BEGIN
        SELECT -1 AS Result, N'Không tìm thấy mã OBD' AS [Message], CAST(NULL AS BIGINT) AS Id, CAST(NULL AS BIGINT) AS Delivery, CAST(0 AS DECIMAL(18,3)) AS NetWeight;
        RETURN;
    END

    -- Chặn trùng theo mã Delivery (không theo từng ObdId)
    IF EXISTS (
        SELECT 1 FROM dbo.VehicleInspectionObd
        WHERE VehicleInspectionId = @VehicleInspectionId AND Delivery = @Delivery
    )
    BEGIN
        SELECT 0 AS Result, N'Mã OBD đã được gắn cho xe này' AS [Message], CAST(NULL AS BIGINT) AS Id, @Delivery AS Delivery, CAST(0 AS DECIMAL(18,3)) AS NetWeight;
        RETURN;
    END

    IF @Version IS NOT NULL
    BEGIN
        SELECT @NetWeight = SUM(NetWeight)
        FROM dbo.ObdConfig
        WHERE LoadingDate = @LoadingDate AND [Version] = @Version AND Delivery = @Delivery;
    END

    INSERT INTO dbo.VehicleInspectionObd (VehicleInspectionId, ObdId, Delivery, UserCreateID, CreateDate)
    VALUES (@VehicleInspectionId, @ObdId, @Delivery, @UserId, GETDATE());

    SELECT 1 AS Result,
           N'Đã gắn OBD' AS [Message],
           CAST(SCOPE_IDENTITY() AS BIGINT) AS Id,
           @Delivery AS Delivery,
           ISNULL(@NetWeight, 0) AS NetWeight;
END
GO

IF OBJECT_ID(N'dbo.SP_DeleteVehicleInspectionObd', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_DeleteVehicleInspectionObd;
GO
CREATE PROCEDURE dbo.SP_DeleteVehicleInspectionObd
    @VehicleInspectionId INT,
    @ObdId               BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Delivery BIGINT =
    (
        SELECT TOP 1 Delivery FROM dbo.VehicleInspectionObd
        WHERE VehicleInspectionId = @VehicleInspectionId AND ObdId = @ObdId
    );

    IF @Delivery IS NULL
        SET @Delivery = (SELECT Delivery FROM dbo.ObdConfig WHERE ObdId = @ObdId);

    -- Gỡ toàn bộ dòng cùng mã Delivery trên xe (tránh còn sót bản ghi trùng mã)
    DELETE FROM dbo.VehicleInspectionObd
    WHERE VehicleInspectionId = @VehicleInspectionId
      AND (
            (@Delivery IS NOT NULL AND Delivery = @Delivery)
            OR ObdId = @ObdId
          );

    IF @@ROWCOUNT > 0
        SELECT 1 AS Result, N'Đã gỡ OBD' AS [Message];
    ELSE
        SELECT 0 AS Result, N'Không tìm thấy liên kết OBD' AS [Message];
END
GO
