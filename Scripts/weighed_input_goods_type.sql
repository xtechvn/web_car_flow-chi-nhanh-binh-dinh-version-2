-- =============================================================================
-- WeighedInput: cột GoodsType + AllCode GOODS_TYPE + mở rộng SP list
-- Chạy lại nhiều lần được (IF NOT EXISTS / CREATE OR ALTER).
-- =============================================================================

IF COL_LENGTH(N'dbo.VehicleInspection', N'GoodsType') IS NULL
BEGIN
    ALTER TABLE dbo.VehicleInspection ADD GoodsType INT NULL;
END
GO

-- Dùng NCHAR để tránh lỗi encoding khi sqlcmd không đọc UTF-8 (HÃ ng bao...)
IF NOT EXISTS (SELECT 1 FROM dbo.AllCode WHERE [Type] = N'GOODS_TYPE')
BEGIN
    INSERT INTO dbo.AllCode ([Type], CodeValue, Description, OrderNo, CreateDate)
    VALUES
        (N'GOODS_TYPE', 0, N'H' + NCHAR(0x00E0) + N'ng bao', 1, GETDATE()),
        (N'GOODS_TYPE', 1, N'H' + NCHAR(0x00E0) + N'ng x' + NCHAR(0x00E1), 2, GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.AllCode SET Description = N'H' + NCHAR(0x00E0) + N'ng bao'
    WHERE [Type] = N'GOODS_TYPE' AND CodeValue = 0;
    UPDATE dbo.AllCode SET Description = N'H' + NCHAR(0x00E0) + N'ng x' + NCHAR(0x00E1)
    WHERE [Type] = N'GOODS_TYPE' AND CodeValue = 1;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetListVehicleWeighedInput]
	@VehicleNumber varchar(50) = null,
	@PhoneNumber varchar(50) = null,
	@VehicleStatus int = null,
	@LoadType int = null,
	@VehicleWeighingType int = null,
	@VehicleTroughStatus varchar(50) = null,
	@TroughType int = null,
	@VehicleWeighingStatus int = null,
	@LoadingStatus int = null,
	@VehicleWeighedstatus int = null,
	@RegisterDateOnline datetime
AS
BEGIN
	DECLARE @PrevDateTime datetime =
	(
		SELECT TOP 1 RegisterDateOnline
		FROM VehicleInspection
		WHERE RegisterDateOnline > DATEADD(MINUTE, -5, DATEADD(DAY, -1, @RegisterDateOnline))
		  AND RecordNumber = 1
		ORDER BY RecordNumber ASC
	);
	IF (@PrevDateTime IS NULL)
	BEGIN
		SET @PrevDateTime =
		(
			SELECT TOP 1 RegisterDateOnline
			FROM VehicleInspection
			WHERE CAST(RegisterDateOnline AS DATE) = CAST(@RegisterDateOnline AS DATE)
			ORDER BY RecordNumber ASC
		);
	END

	SELECT
		V.Id,
		RecordNumber,
		CustomerName,
		VehicleNumber,
		RegisterDateOnline,
		DriverName,
		LicenseNumber,
		PhoneNumber,
		VehicleLoad,
		VehicleStatus,
		LoadType,
		VehicleWeighingTimeComeIn,
		VehicleWeighingTimeComeOut,
		IssueCreateDate,
		IssueUpdatedDate,
		VehicleWeighingTimeComplete,
		VehicleTroughWeight,
		AudioPath,
		VehicleTroughStatus,
		LoadingType,
		V.Note,
		V.ProtectNotes,
		V.CSNotes,
		V.GoodsType,
		ac9.Description AS GoodsTypeName,
		ac6.Description AS VehicleWeighingStatusName,
		ac5.Description AS VehicleTroughStatusName,
		ac4.Description AS TroughTypeName,
		ac3.Description AS VehicleWeighingTypeName,
		ac2.Description AS VehicleStatusName,
		ac.Description AS LoadTypeName,
		ac7.Description AS LoadingStatusName,
		ac8.Description AS VehicleWeighedstatusName,
		V.VehicleWeightIn,
		CAST(ISNULL(insp.VehicleWeightMax, 0) AS DECIMAL(18,2)) AS VehicleWeightMax
	FROM VehicleInspection V
	LEFT JOIN AllCode ac  ON V.LoadType = ac.CodeValue AND ac.Type = 'LOAD_TYPE'
	LEFT JOIN AllCode ac2 ON V.VehicleStatus = ac2.CodeValue AND ac2.Type = 'VEHICLE_STATUS'
	LEFT JOIN AllCode ac3 ON V.VehicleWeighingType = ac3.CodeValue AND ac3.Type = 'VEHICLEWEIGHING_TYPE'
	LEFT JOIN AllCode ac4 ON V.TroughType = ac4.CodeValue AND ac4.Type = 'TROUGH_TYPE'
	LEFT JOIN AllCode ac5 ON V.VehicleTroughStatus = ac5.CodeValue AND ac5.Type = 'VEHICLETROUGH_STATUS'
	LEFT JOIN AllCode ac6 ON V.VehicleWeighingStatus = ac6.CodeValue AND ac6.Type = 'VEHICLEWEIGHINGSTATUS'
	LEFT JOIN AllCode ac7 ON V.LoadingStatus = ac7.CodeValue AND ac7.Type = 'LOADINGSTATUS'
	LEFT JOIN AllCode ac8 ON V.VehicleWeighedstatus = ac8.CodeValue AND ac8.Type = 'VEHICLEWEIGHEDSTATUS'
	LEFT JOIN AllCode ac9 ON V.GoodsType = ac9.CodeValue AND ac9.Type = 'GOODS_TYPE'
	OUTER APPLY (
		SELECT TOP 1 I.VehicleWeightMax
		FROM Inspection I
		WHERE REPLACE(REPLACE(UPPER(ISNULL(I.VehicleNumber,'')), '-', ''), ' ', '')
		    = REPLACE(REPLACE(UPPER(ISNULL(V.VehicleNumber,'')), '-', ''), ' ', '')
		ORDER BY I.Id DESC
	) insp
	WHERE
	(
		 (@VehicleNumber IS NULL OR V.VehicleNumber LIKE N'%' + @VehicleNumber + '%')
	  OR (@PhoneNumber IS NULL OR V.PhoneNumber LIKE N'%' + @PhoneNumber + '%')
	)
	AND (@LoadType IS NULL OR V.LoadType = @LoadType)
	AND (@VehicleStatus IS NULL AND (V.VehicleStatus <> 0 OR V.VehicleStatus IS NULL) OR V.VehicleStatus = @VehicleStatus)
	AND (@VehicleWeighingType IS NULL AND (V.VehicleWeighingType <> 0 OR V.VehicleWeighingType IS NULL) OR V.VehicleWeighingType = @VehicleWeighingType)
	AND (@TroughType IS NULL OR V.TroughType = @TroughType)
	AND (
		 @VehicleTroughStatus IS NULL
		 AND ((V.VehicleTroughStatus <> 0 AND V.VehicleTroughStatus <> 4) OR V.VehicleTroughStatus IS NULL)
		 OR V.VehicleTroughStatus IN (SELECT CAST(value AS bigint) FROM STRING_SPLIT(@VehicleTroughStatus, ','))
		)
	AND (@VehicleWeighingStatus IS NULL AND (V.VehicleWeighingStatus <> 0 OR V.VehicleWeighingStatus IS NULL) OR V.VehicleWeighingStatus = @VehicleWeighingStatus)
	AND (@LoadingStatus IS NULL AND (V.LoadingStatus <> 0 OR V.LoadingStatus IS NULL) OR V.LoadingStatus = @LoadingStatus)
	AND (@VehicleWeighedstatus IS NULL AND (V.VehicleWeighedstatus <> 0 OR V.VehicleWeighedstatus IS NULL) OR V.VehicleWeighedstatus = @VehicleWeighedstatus)
	AND (
		 @RegisterDateOnline IS NULL
		 OR (V.RegisterDateOnline BETWEEN @PrevDateTime AND @RegisterDateOnline)
		 OR (
				V.VehicleStatus = 0
			AND CAST(V.RegisterDateOnline AS DATE) >= CAST(GETDATE() AS DATE)
			)
	    )
	ORDER BY V.VehicleWeighingTimeComeIn ASC;
END
GO

-- sp_UpdateVehicleInspection: thêm @GoodsType (giữ nguyên các param cũ)
CREATE OR ALTER PROC [dbo].[sp_UpdateVehicleInspection]
(
@Id AS INT,
@RecordNumber AS INT = Null,
@CustomerName AS NVARCHAR(500) = Null,
@VehicleNumber AS VARCHAR(50) = Null,
@RegisterDateOnline AS DATETIME = Null,
@DriverName AS NVARCHAR(500) = Null,
@LicenseNumber AS NVARCHAR(30) = Null,
@PhoneNumber AS VARCHAR(20) = Null,
@VehicleLoad AS NVARCHAR(50) = Null,
@VehicleStatus AS INT = Null,
@LoadType AS INT = Null,
@IssueCreateDate AS DATETIME = Null,
@IssueUpdatedDate AS DATETIME = Null,
@VehicleWeighingType AS INT = Null,
@VehicleWeighingTimeComeIn AS DATETIME = Null,
@VehicleWeighingTimeComeOut AS DATETIME = Null,
@VehicleWeighingTimeComplete AS DATETIME = Null,
@TroughType AS INT = Null,
@VehicleTroughTimeComeIn AS DATETIME = Null,
@VehicleTroughTimeComeOut AS DATETIME = Null,
@VehicleTroughWeight AS DECIMAL = Null,
@VehicleTroughStatus AS INT = Null,
@UpdatedBy AS INT = Null,
@LoadingStatus AS INT = Null,
@VehicleWeighedstatus AS INT = Null,
@VehicleWeighingStatus AS INT = Null,
@TimeCallVehicleTroughTimeComeIn AS DATETIME = Null,
@VehicleArrivalDate AS DATETIME = Null,
@Note AS NVARCHAR(500) = Null,
@LoadingType AS INT = Null,
@ProcessingIsLoadingDate AS DATETIME = Null,
@ProtectNotes AS NVARCHAR(500) = Null,
@AudioPath AS NVARCHAR(500) = Null,
@Rank AS INT = Null,
@CSNotes AS NVARCHAR(500) = Null,
@VehicleWeightIn AS decimal(8, 0) = Null,
@VehicleWeightOut AS decimal(8, 0) = Null,
@GoodsType AS INT = Null,
@Identity AS INT OUT
)
AS
UPDATE [VehicleInspection] SET
		RecordNumber = ISNULL(@RecordNumber, RecordNumber),
		CustomerName = ISNULL(@CustomerName, CustomerName),
		VehicleNumber = ISNULL(@VehicleNumber, VehicleNumber),
		RegisterDateOnline = ISNULL(@RegisterDateOnline, RegisterDateOnline),
		DriverName = ISNULL(@DriverName, DriverName),
		LicenseNumber = ISNULL(@LicenseNumber, LicenseNumber),
		PhoneNumber = ISNULL(@PhoneNumber, PhoneNumber),
		VehicleLoad = ISNULL(@VehicleLoad, VehicleLoad),
		VehicleStatus = ISNULL(@VehicleStatus, VehicleStatus),
		LoadType = ISNULL(@LoadType, LoadType),
		IssueCreateDate = ISNULL(@IssueCreateDate, IssueCreateDate),
		IssueUpdatedDate = ISNULL(@IssueUpdatedDate, IssueUpdatedDate),
		VehicleWeighingType = ISNULL(@VehicleWeighingType, VehicleWeighingType),
		VehicleWeighingTimeComeIn = ISNULL(@VehicleWeighingTimeComeIn, VehicleWeighingTimeComeIn),
		VehicleWeighingTimeComeOut = ISNULL(@VehicleWeighingTimeComeOut, VehicleWeighingTimeComeOut),
		VehicleWeighingTimeComplete = ISNULL(@VehicleWeighingTimeComplete, VehicleWeighingTimeComplete),
		TroughType = ISNULL(@TroughType, TroughType),
		VehicleTroughTimeComeIn = ISNULL(@VehicleTroughTimeComeIn, VehicleTroughTimeComeIn),
		VehicleTroughTimeComeOut = ISNULL(@VehicleTroughTimeComeOut, VehicleTroughTimeComeOut),
		VehicleTroughWeight = ISNULL(@VehicleTroughWeight, VehicleTroughWeight),
		VehicleTroughStatus = ISNULL(@VehicleTroughStatus, VehicleTroughStatus),
		UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy),
		UpdatedDate= GETDATE(),
		LoadingStatus=ISNULL(@LoadingStatus, LoadingStatus),
		VehicleWeighedstatus=ISNULL(@VehicleWeighedstatus, VehicleWeighedstatus),
		VehicleWeighingStatus=ISNULL(@VehicleWeighingStatus, VehicleWeighingStatus),
		TimeCallVehicleTroughTimeComeIn=ISNULL(@TimeCallVehicleTroughTimeComeIn, TimeCallVehicleTroughTimeComeIn),
		Note=ISNULL(@Note, Note),
		VehicleArrivalDate=ISNULL(@VehicleArrivalDate, VehicleArrivalDate),
		LoadingType=ISNULL(@LoadingType, LoadingType),
		ProcessingIsLoadingDate=ISNULL(@ProcessingIsLoadingDate, ProcessingIsLoadingDate),
		ProtectNotes=ISNULL(@ProtectNotes, ProtectNotes),
		AudioPath=ISNULL(@AudioPath, AudioPath),
		[Rank]=ISNULL(@Rank, [Rank]),
		CSNotes=@CSNotes,
		VehicleWeightIn=ISNULL(@VehicleWeightIn, VehicleWeightIn),
		VehicleWeightOut=ISNULL(@VehicleWeightOut, VehicleWeightOut),
		GoodsType=ISNULL(@GoodsType, GoodsType)
WHERE Id = @Id;

SELECT @Identity = @Id;
GO
