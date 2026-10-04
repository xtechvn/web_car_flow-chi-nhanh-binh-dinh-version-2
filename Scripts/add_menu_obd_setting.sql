-- Thêm card "Cấu hình OCB" vào trang chủ CMS (/ObdSetting/Index)
-- Card trang chủ = Menu con của Menu Id 115 (xem HomeController.Index)
-- Quyền "Truy cập" (PermissionId = 1) được cấp cho mọi role đang có quyền Truy cập Menu 115 hoặc một card bất kỳ của trang chủ
-- Script chạy lại nhiều lần không bị trùng dữ liệu

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @HomeMenuId INT = 115;
DECLARE @PermissionTruyCap INT = 1;
DECLARE @Link NVARCHAR(200) = N'/ObdSetting/Index';
DECLARE @MenuId INT;

SELECT @MenuId = Id FROM dbo.Menu WHERE Link = @Link AND ParentId = @HomeMenuId;

IF @MenuId IS NULL
BEGIN
    DECLARE @FullParent VARCHAR(100);
    SELECT @FullParent = CASE WHEN ISNULL(FullParent, '') = '' THEN CAST(Id AS VARCHAR(20))
                              ELSE FullParent + ',' + CAST(Id AS VARCHAR(20)) END
    FROM dbo.Menu WHERE Id = @HomeMenuId;

    INSERT INTO dbo.Menu (ParentId, Name, Title, MenuCode, Description, Status, CreatedOn, Icon, Link, OrderNo, FullParent)
    VALUES (@HomeMenuId, N'Cấu hình OCB', N'Cấu hình OCB', 'OBD_SETTING', N'Quản lý và cấu hình thông tin OBD', 0, GETDATE(),
            N'/images/icons/icon-obd-setting.svg', @Link,
            (SELECT ISNULL(MAX(OrderNo), 0) + 1 FROM dbo.Menu WHERE ParentId = @HomeMenuId),
            @FullParent);

    SET @MenuId = SCOPE_IDENTITY();
END

INSERT INTO dbo.RolePermission (RoleId, MenuId, PermissionId)
SELECT DISTINCT rp.RoleId, @MenuId, @PermissionTruyCap
FROM dbo.RolePermission rp
WHERE rp.MenuId IN (SELECT Id FROM dbo.Menu WHERE (Id = @HomeMenuId OR ParentId = @HomeMenuId) AND Id <> @MenuId)
  AND rp.PermissionId = @PermissionTruyCap
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission x
                  WHERE x.RoleId = rp.RoleId AND x.MenuId = @MenuId AND x.PermissionId = @PermissionTruyCap);

SELECT * FROM dbo.Menu WHERE Id = @MenuId;
SELECT * FROM dbo.RolePermission WHERE MenuId = @MenuId;

COMMIT TRANSACTION;
