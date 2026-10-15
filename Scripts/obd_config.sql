-- =============================================================================
-- Chức năng Cấu hình OBD (/ObdSetting/Index)
-- Tạo: bảng ObdConfig, bảng log ObdConfigLog, kiểu bảng ObdConfigImportType,
--      hàm tính ngày nghiệp vụ theo cut-off và các stored procedure.
-- Script chạy lại nhiều lần được: bảng/kiểu chỉ tạo khi chưa có, function/SP được tạo lại.
--
-- Quy tắc ngày nghiệp vụ (LoadingDate):
--   Cut-off lấy từ dbo.AllCode (Type = 'TIME_RESET', giờ:phút ở cột UpdateTime), mặc định 17:55.
--   Thời điểm <= cut-off thuộc ngày hiện tại, sau cut-off thuộc ngày hôm sau.
-- Version: mỗi lần import file trong cùng một LoadingDate là một version, bắt đầu từ 1.
-- =============================================================================

-- ---------------------------------------------------------------------------
-- 1. Bảng dữ liệu
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.ObdConfig', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ObdConfig
    (
        ObdId         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ObdConfig PRIMARY KEY,
        Delivery      BIGINT         NOT NULL,
        NetWeight     DECIMAL(18,3)  NOT NULL,
        LoadingDate   DATE           NOT NULL,
        [Version]     INT            NOT NULL,
        UserCreateID  INT            NULL,
        UserUpdateID  INT            NULL,
        CreateDate    DATETIME       NOT NULL CONSTRAINT DF_ObdConfig_CreateDate DEFAULT (GETDATE()),
        UpdateLast    DATETIME       NULL
    );

    CREATE INDEX IX_ObdConfig_LoadingDate_Version ON dbo.ObdConfig (LoadingDate, [Version]) INCLUDE (Delivery, NetWeight);
    CREATE INDEX IX_ObdConfig_Delivery ON dbo.ObdConfig (Delivery);
END
GO

-- Action: 1 = Import file Excel, 2 = Thêm mới từ view, 3 = Cập nhật bản ghi
IF OBJECT_ID(N'dbo.ObdConfigLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ObdConfigLog
    (
        LogId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ObdConfigLog PRIMARY KEY,
        ObdId        BIGINT         NULL,
        [Action]     TINYINT        NOT NULL,
        LoadingDate  DATE           NULL,
        [Version]    INT            NULL,
        FieldName    VARCHAR(50)    NULL,
        OldValue     NVARCHAR(200)  NULL,
        NewValue     NVARCHAR(200)  NULL,
        [FileName]   NVARCHAR(255)  NULL,
        TotalRow     INT            NULL,
        UserId       INT            NULL,
        CreateDate   DATETIME       NOT NULL CONSTRAINT DF_ObdConfigLog_CreateDate DEFAULT (GETDATE())
    );

    CREATE INDEX IX_ObdConfigLog_ObdId ON dbo.ObdConfigLog (ObdId);
    CREATE INDEX IX_ObdConfigLog_LoadingDate ON dbo.ObdConfigLog (LoadingDate, [Version]);
END
GO

-- ---------------------------------------------------------------------------
-- 2. Kiểu bảng nhận dữ liệu import (Table-Valued Parameter)
-- ---------------------------------------------------------------------------
IF TYPE_ID(N'dbo.ObdConfigImportType') IS NULL
BEGIN
    CREATE TYPE dbo.ObdConfigImportType AS TABLE
    (
        RowNumber  INT            NOT NULL,
        Delivery   BIGINT         NOT NULL,
        NetWeight  DECIMAL(18,3)  NOT NULL
    );
END
GO

-- ---------------------------------------------------------------------------
-- 3. Hàm tính ngày nghiệp vụ theo cut-off
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.fn_GetObdBusinessDate', N'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetObdBusinessDate;
GO
CREATE FUNCTION dbo.fn_GetObdBusinessDate (@Now DATETIME)
RETURNS DATE
AS
BEGIN
    DECLARE @Cutoff TIME(0);

    SELECT TOP 1 @Cutoff = CAST(CONVERT(VARCHAR(5), UpdateTime, 108) AS TIME(0))
    FROM dbo.AllCode
    WHERE [Type] = 'TIME_RESET' AND UpdateTime IS NOT NULL
    ORDER BY OrderNo, Id;

    IF @Cutoff IS NULL
        SET @Cutoff = '17:55:00';

    RETURN CASE WHEN CAST(@Now AS TIME) > @Cutoff
                THEN DATEADD(DAY, 1, CAST(@Now AS DATE))
                ELSE CAST(@Now AS DATE) END;
END
GO

IF OBJECT_ID(N'dbo.SP_GetObdBusinessDate', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetObdBusinessDate;
GO
CREATE PROCEDURE dbo.SP_GetObdBusinessDate
AS
BEGIN
    SET NOCOUNT ON;
    SELECT dbo.fn_GetObdBusinessDate(GETDATE()) AS BusinessDate;
END
GO

-- ---------------------------------------------------------------------------
-- 4. Tạo mới từ file Excel: mỗi lần import là một version mới của ngày nghiệp vụ
--    Kết quả: Result (1 thành công, <0 lỗi), Message, LoadingDate, Version, TotalRow
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_InsertObdConfigFromExcel', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_InsertObdConfigFromExcel;
GO
CREATE PROCEDURE dbo.SP_InsertObdConfigFromExcel
    @Rows      dbo.ObdConfigImportType READONLY,
    @UserId    INT,
    @FileName  NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME = GETDATE();
    DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(@Now);
    DECLARE @Version INT, @TotalRow INT, @LockResult INT;
    DECLARE @LockName NVARCHAR(100) = N'ObdConfig_' + CONVERT(CHAR(8), @LoadingDate, 112);

    IF NOT EXISTS (SELECT 1 FROM @Rows)
    BEGIN
        SELECT -1 AS Result, N'File không có dữ liệu' AS [Message], @LoadingDate AS LoadingDate, 0 AS [Version], 0 AS TotalRow;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        EXEC @LockResult = sp_getapplock @Resource = @LockName, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
        IF @LockResult < 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -2 AS Result, N'Hệ thống đang xử lý một lần import khác, vui lòng thử lại' AS [Message], @LoadingDate AS LoadingDate, 0 AS [Version], 0 AS TotalRow;
            RETURN;
        END

        SELECT @Version = ISNULL(MAX([Version]), 0) + 1
        FROM dbo.ObdConfig
        WHERE LoadingDate = @LoadingDate;

        INSERT INTO dbo.ObdConfig (Delivery, NetWeight, LoadingDate, [Version], UserCreateID, CreateDate)
        SELECT Delivery, NetWeight, @LoadingDate, @Version, @UserId, @Now
        FROM @Rows
        ORDER BY RowNumber;

        SET @TotalRow = @@ROWCOUNT;

        INSERT INTO dbo.ObdConfigLog ([Action], LoadingDate, [Version], [FileName], TotalRow, UserId, CreateDate)
        VALUES (1, @LoadingDate, @Version, @FileName, @TotalRow, @UserId, @Now);

        COMMIT TRANSACTION;

        SELECT 1 AS Result, N'Import thành công' AS [Message], @LoadingDate AS LoadingDate, @Version AS [Version], @TotalRow AS TotalRow;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------------
-- 5. Tạo mới từ view: thêm vào version mới nhất của ngày nghiệp vụ (chưa có thì version 1)
--    Kết quả: Result, Message, ObdId, LoadingDate, Version
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_InsertObdConfig', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_InsertObdConfig;
GO
CREATE PROCEDURE dbo.SP_InsertObdConfig
    @Delivery   BIGINT,
    @NetWeight  DECIMAL(18,3),
    @UserId     INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME = GETDATE();
    DECLARE @LoadingDate DATE = dbo.fn_GetObdBusinessDate(@Now);
    DECLARE @Version INT, @ObdId BIGINT, @LockResult INT;
    DECLARE @LockName NVARCHAR(100) = N'ObdConfig_' + CONVERT(CHAR(8), @LoadingDate, 112);

    BEGIN TRY
        BEGIN TRANSACTION;

        EXEC @LockResult = sp_getapplock @Resource = @LockName, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
        IF @LockResult < 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -2 AS Result, N'Hệ thống đang xử lý một lần import khác, vui lòng thử lại' AS [Message], CAST(0 AS BIGINT) AS ObdId, @LoadingDate AS LoadingDate, 0 AS [Version];
            RETURN;
        END

        SELECT @Version = ISNULL(MAX([Version]), 1)
        FROM dbo.ObdConfig
        WHERE LoadingDate = @LoadingDate;

        INSERT INTO dbo.ObdConfig (Delivery, NetWeight, LoadingDate, [Version], UserCreateID, CreateDate)
        VALUES (@Delivery, @NetWeight, @LoadingDate, @Version, @UserId, @Now);

        SET @ObdId = SCOPE_IDENTITY();

        INSERT INTO dbo.ObdConfigLog (ObdId, [Action], LoadingDate, [Version], FieldName, NewValue, UserId, CreateDate)
        VALUES (@ObdId, 2, @LoadingDate, @Version, 'Delivery|NetWeight',
                CAST(@Delivery AS NVARCHAR(30)) + N'|' + CAST(@NetWeight AS NVARCHAR(30)), @UserId, @Now);

        COMMIT TRANSACTION;

        SELECT 1 AS Result, N'Thêm mới thành công' AS [Message], @ObdId AS ObdId, @LoadingDate AS LoadingDate, @Version AS [Version];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------------
-- 6. Cập nhật bản ghi: chỉ sửa Delivery, giữ nguyên Version và LoadingDate
--    Kết quả: Result (1 thành công, 0 không thay đổi, -1 không tìm thấy), Message
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_UpdateObdConfig', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_UpdateObdConfig;
GO
CREATE PROCEDURE dbo.SP_UpdateObdConfig
    @ObdId     BIGINT,
    @Delivery  BIGINT,
    @UserId    INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME = GETDATE();
    DECLARE @OldDelivery BIGINT, @LoadingDate DATE, @Version INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @OldDelivery = Delivery, @LoadingDate = LoadingDate, @Version = [Version]
        FROM dbo.ObdConfig WITH (UPDLOCK, HOLDLOCK)
        WHERE ObdId = @ObdId;

        IF @OldDelivery IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result, N'Không tìm thấy bản ghi' AS [Message];
            RETURN;
        END

        IF @OldDelivery = @Delivery
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Result, N'Delivery không thay đổi' AS [Message];
            RETURN;
        END

        UPDATE dbo.ObdConfig
        SET Delivery = @Delivery,
            UserUpdateID = @UserId,
            UpdateLast = @Now
        WHERE ObdId = @ObdId;

        INSERT INTO dbo.ObdConfigLog (ObdId, [Action], LoadingDate, [Version], FieldName, OldValue, NewValue, UserId, CreateDate)
        VALUES (@ObdId, 3, @LoadingDate, @Version, 'Delivery',
                CAST(@OldDelivery AS NVARCHAR(30)), CAST(@Delivery AS NVARCHAR(30)), @UserId, @Now);

        COMMIT TRANSACTION;

        SELECT 1 AS Result, N'Cập nhật thành công' AS [Message];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------------
-- 7. Danh sách version của một ngày (dùng cho dropdown Version và 3 thẻ thống kê)
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_GetListObdConfigVersion', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetListObdConfigVersion;
GO
CREATE PROCEDURE dbo.SP_GetListObdConfigVersion
    @LoadingDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Version],
           COUNT(DISTINCT Delivery) AS TotalObd,
           SUM(NetWeight)           AS TotalNetWeight,
           COUNT(1)                 AS TotalRow,
           MAX(CASE WHEN UpdateLast > CreateDate THEN UpdateLast ELSE CreateDate END) AS LastUpdate
    FROM dbo.ObdConfig
    WHERE LoadingDate = @LoadingDate
    GROUP BY [Version]
    ORDER BY [Version] DESC;
END
GO

-- ---------------------------------------------------------------------------
-- 8. Danh sách bản ghi theo ngày + version, tìm Delivery khớp một phần, có phân trang
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_GetListObdConfig', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetListObdConfig;
GO
CREATE PROCEDURE dbo.SP_GetListObdConfig
    @LoadingDate  DATE,
    @Version      INT,
    @Delivery     VARCHAR(20) = NULL,
    @PageIndex    INT = 1,
    @PageSize     INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageIndex IS NULL OR @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize IS NULL OR @PageSize < 1 SET @PageSize = 20;
    SET @Delivery = NULLIF(LTRIM(RTRIM(@Delivery)), '');

    SELECT o.ObdId,
           o.Delivery,
           o.NetWeight,
           o.LoadingDate,
           o.[Version],
           o.UserCreateID,
           o.UserUpdateID,
           o.CreateDate,
           o.UpdateLast,
           uc.UserName AS UserCreateName,
           uu.UserName AS UserUpdateName,
           COUNT(1) OVER () AS TotalRow
    FROM dbo.ObdConfig o
    LEFT JOIN dbo.[User] uc ON uc.Id = o.UserCreateID
    LEFT JOIN dbo.[User] uu ON uu.Id = o.UserUpdateID
    WHERE o.LoadingDate = @LoadingDate
      AND o.[Version] = @Version
      AND (@Delivery IS NULL OR CAST(o.Delivery AS VARCHAR(20)) LIKE '%' + @Delivery + '%')
    ORDER BY o.ObdId
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ---------------------------------------------------------------------------
-- 9. Chi tiết một bản ghi (phần đầu popup lịch sử)
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_GetObdConfigById', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetObdConfigById;
GO
CREATE PROCEDURE dbo.SP_GetObdConfigById
    @ObdId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT o.ObdId,
           o.Delivery,
           o.NetWeight,
           o.LoadingDate,
           o.[Version],
           o.UserCreateID,
           o.UserUpdateID,
           o.CreateDate,
           o.UpdateLast,
           uc.UserName AS UserCreateName,
           uu.UserName AS UserUpdateName
    FROM dbo.ObdConfig o
    LEFT JOIN dbo.[User] uc ON uc.Id = o.UserCreateID
    LEFT JOIN dbo.[User] uu ON uu.Id = o.UserUpdateID
    WHERE o.ObdId = @ObdId;
END
GO

-- ---------------------------------------------------------------------------
-- 10. Lịch sử thao tác của một bản ghi.
--     Log import ghi một dòng cho cả file (ObdId NULL) nên ghép theo LoadingDate + Version,
--     chỉ áp dụng cho bản ghi sinh ra từ import (không có log thêm thủ công).
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SP_GetListObdConfigLog', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetListObdConfigLog;
GO
CREATE PROCEDURE dbo.SP_GetListObdConfigLog
    @ObdId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LoadingDate DATE, @Version INT, @IsManual BIT = 0;

    SELECT @LoadingDate = LoadingDate, @Version = [Version]
    FROM dbo.ObdConfig
    WHERE ObdId = @ObdId;

    IF EXISTS (SELECT 1 FROM dbo.ObdConfigLog WHERE ObdId = @ObdId AND [Action] = 2)
        SET @IsManual = 1;

    SELECT l.LogId,
           l.ObdId,
           l.[Action],
           l.LoadingDate,
           l.[Version],
           l.FieldName,
           l.OldValue,
           l.NewValue,
           l.[FileName],
           l.TotalRow,
           l.UserId,
           u.UserName,
           l.CreateDate
    FROM dbo.ObdConfigLog l
    LEFT JOIN dbo.[User] u ON u.Id = l.UserId
    WHERE l.ObdId = @ObdId
       OR (@IsManual = 0
           AND l.[Action] = 1
           AND l.ObdId IS NULL
           AND l.LoadingDate = @LoadingDate
           AND l.[Version] = @Version)
    ORDER BY l.CreateDate DESC, l.LogId DESC;
END
GO
