-- =============================================================================
-- WeighedInput: nhiều SKU cân đầu vào / 1 xe (Hàng xá)
-- =============================================================================

IF OBJECT_ID(N'dbo.VehicleWeighInSku', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleWeighInSku
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VehicleWeighInSku PRIMARY KEY,
        VehicleInspectionId  INT            NOT NULL,
        SkuLine              INT            NOT NULL,
        NetWeightKg          DECIMAL(18,3)  NOT NULL CONSTRAINT DF_VehicleWeighInSku_NetWeightKg DEFAULT (0),
        CreatedBy            INT            NULL,
        CreatedDate          DATETIME       NOT NULL CONSTRAINT DF_VehicleWeighInSku_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy            INT            NULL,
        UpdatedDate          DATETIME       NULL,
        CONSTRAINT FK_VehicleWeighInSku_VI FOREIGN KEY (VehicleInspectionId)
            REFERENCES dbo.VehicleInspection (Id)
    );
    CREATE INDEX IX_VehicleWeighInSku_VI ON dbo.VehicleWeighInSku (VehicleInspectionId) INCLUDE (SkuLine, NetWeightKg);
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_GetVehicleWeighInSkuByVehicleIds
    @VehicleInspectionIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.Id,
           s.VehicleInspectionId,
           s.SkuLine,
           s.NetWeightKg AS Weight,
           s.CreatedDate,
           s.UpdatedDate
    FROM dbo.VehicleWeighInSku s
    INNER JOIN STRING_SPLIT(@VehicleInspectionIds, ',') x ON TRY_CAST(x.[value] AS INT) = s.VehicleInspectionId
    ORDER BY s.VehicleInspectionId, s.SkuLine, s.Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_InsertVehicleWeighInSku
    @VehicleInspectionId INT,
    @Weight DECIMAL(18,3),
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SkuLine INT =
        ISNULL((SELECT MAX(SkuLine) FROM dbo.VehicleWeighInSku WHERE VehicleInspectionId = @VehicleInspectionId), 0) + 1;

    INSERT INTO dbo.VehicleWeighInSku (VehicleInspectionId, SkuLine, NetWeightKg, CreatedBy, CreatedDate)
    VALUES (@VehicleInspectionId, @SkuLine, ISNULL(@Weight, 0), @CreatedBy, GETDATE());

    DECLARE @NewId BIGINT = SCOPE_IDENTITY();

    UPDATE dbo.VehicleInspection
    SET VehicleWeightIn = ISNULL((SELECT SUM(NetWeightKg) FROM dbo.VehicleWeighInSku WHERE VehicleInspectionId = @VehicleInspectionId), 0),
        UpdatedDate = GETDATE()
    WHERE Id = @VehicleInspectionId;

    SELECT @NewId AS Id, @VehicleInspectionId AS VehicleInspectionId, @SkuLine AS SkuLine,
           ISNULL(@Weight, 0) AS Weight;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_UpdateVehicleWeighInSku
    @Id BIGINT,
    @Weight DECIMAL(18,3),
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Vid INT = (SELECT VehicleInspectionId FROM dbo.VehicleWeighInSku WHERE Id = @Id);
    IF @Vid IS NULL
    BEGIN
        SELECT 0 AS Result;
        RETURN;
    END

    UPDATE dbo.VehicleWeighInSku
    SET NetWeightKg = ISNULL(@Weight, 0),
        UpdatedBy = @UpdatedBy,
        UpdatedDate = GETDATE()
    WHERE Id = @Id;

    UPDATE dbo.VehicleInspection
    SET VehicleWeightIn = ISNULL((SELECT SUM(NetWeightKg) FROM dbo.VehicleWeighInSku WHERE VehicleInspectionId = @Vid), 0),
        UpdatedDate = GETDATE()
    WHERE Id = @Vid;

    SELECT 1 AS Result, @Vid AS VehicleInspectionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DeleteVehicleWeighInSku
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Vid INT = (SELECT VehicleInspectionId FROM dbo.VehicleWeighInSku WHERE Id = @Id);
    IF @Vid IS NULL
    BEGIN
        SELECT 0 AS Result;
        RETURN;
    END

    DELETE FROM dbo.VehicleWeighInSku WHERE Id = @Id;

    ;WITH ordered AS (
        SELECT Id, ROW_NUMBER() OVER (ORDER BY SkuLine, Id) AS rn
        FROM dbo.VehicleWeighInSku WHERE VehicleInspectionId = @Vid
    )
    UPDATE s SET SkuLine = o.rn
    FROM dbo.VehicleWeighInSku s
    INNER JOIN ordered o ON o.Id = s.Id;

    UPDATE dbo.VehicleInspection
    SET VehicleWeightIn = ISNULL((SELECT SUM(NetWeightKg) FROM dbo.VehicleWeighInSku WHERE VehicleInspectionId = @Vid), 0),
        UpdatedDate = GETDATE()
    WHERE Id = @Vid;

    SELECT 1 AS Result, @Vid AS VehicleInspectionId;
END
GO

-- Xóa toàn bộ SKU khi đổi Hàng xá → Hàng bao; reset VehicleWeightIn = NULL (ô trống)
CREATE OR ALTER PROCEDURE dbo.SP_ClearVehicleWeighInSkuByVehicleId
    @VehicleInspectionId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.VehicleWeighInSku WHERE VehicleInspectionId = @VehicleInspectionId;
    UPDATE dbo.VehicleInspection
    SET VehicleWeightIn = NULL,
        UpdatedDate = GETDATE()
    WHERE Id = @VehicleInspectionId;
    SELECT 1 AS Result;
END
GO
