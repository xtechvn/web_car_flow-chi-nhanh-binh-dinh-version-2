-- Fix AllCode GOODS_TYPE Description (UTF-16 đúng: Hàng bao / Hàng xá)
-- Tránh lỗi font HÃ ng bao khi insert bị double-encode.

SET NOCOUNT ON;

UPDATE dbo.AllCode
SET Description = N'H' + NCHAR(0x00E0) + N'ng bao'
WHERE [Type] = N'GOODS_TYPE' AND CodeValue = 0;

UPDATE dbo.AllCode
SET Description = N'H' + NCHAR(0x00E0) + N'ng x' + NCHAR(0x00E1)
WHERE [Type] = N'GOODS_TYPE' AND CodeValue = 1;

SELECT Id, CodeValue, Description
FROM dbo.AllCode
WHERE [Type] = N'GOODS_TYPE';
GO
