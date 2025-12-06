UPDATE ChiTietSanPham SET SoLuongTon = 1 WHERE MaChiTiet = 1;

UPDATE ChiTietSanPham SET SoLuongTon = 100 WHERE MaChiTiet = 1;

USE QLDN_CAULONG;
GO

-- Set độ ưu tiên cao để ép SQL Server chọn thằng Web làm nạn nhân (Victim)
SET DEADLOCK_PRIORITY HIGH; 

BEGIN TRAN;

-- 1. Khóa Sản phẩm 2 (Thằng mà Web chưa kịp đụng tới)
-- (Giả sử Web đang xử lý ID 1 trước, thì ở đây ta khóa ID 2 trước)
PRINT 'SSMS: Dang khoa SP 2...';
UPDATE ChiTietSanPham SET SoLuongTon = SoLuongTon - 1 WHERE MaChiTiet = 2; 

-- Chờ bấm Continue bên Visual Studio
WAITFOR DELAY '00:00:10'; 

-- 2. Cố gắng chiếm Sản phẩm 1 (Thằng mà Web đang giữ khóa)
PRINT 'SSMS: Dang cho SP 1...';
UPDATE ChiTietSanPham SET SoLuongTon = SoLuongTon - 1 WHERE MaChiTiet = 1; 

COMMIT TRAN;