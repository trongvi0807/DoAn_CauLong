-- Đặt biến cho đường dẫn sao lưu để dễ dàng quản lý
DECLARE @BackupFilePath NVARCHAR(255) = 'D:\DA_HQTCSDL\Backup\QLDN_CAULONG_Full.bak';--tên đường dẫn ổ đĩa,folder,folder tên backup,tên cần sao lưu.bak
-- Thực hiện Full Backup
BACKUP DATABASE QLDN_CAULONG
TO DISK = @BackupFilePath  -- Chỉ định đường dẫn và tên file sao lưu
WITH NOINIT,  -- NOINIT: Thêm vào cuối file backup đã có (nếu muốn ghi đè, dùng INIT)
     NAME = N'QLDN_CAULONG Full Backup',
     STATS = 10; -- Hiển thị tiến trình sao lưu mỗi 10%
GO
-- Thông báo đường dẫn file đã tạo
PRINT N'Đã hoàn thành Full Backup vào đường dẫn: D:\DA_HQTCSDL\Backup\QLDN_CAULONG_Full.bak';

-- Đặt biến cho tên cơ sở dữ liệu phục hồi và đường dẫn file backup
DECLARE @RestoreDBName NVARCHAR(100) = 'QLDN_CAULONG_TEST';--tên sẽ hiện lên sql của mình
DECLARE @BackupFilePath NVARCHAR(255) = 'D:\DA_HQTCSDL\Backup\QLDN_CAULONG_Full.bak';

-- Lệnh RESTORE
RESTORE DATABASE @RestoreDBName
FROM DISK = @BackupFilePath
WITH FILE = 1,  -- Sử dụng file backup đầu tiên trong media set
     NOUNLOAD, -- Không yêu cầu tháo gỡ băng từ (luôn dùng)
     REPLACE,  -- Cho phép ghi đè lên DB nếu nó đã tồn tại
     STATS = 10;
GO
-- Sau khi hoàn thành, bạn có thể kiểm tra DB mới
PRINT N'Đã hoàn thành Restore vào cơ sở dữ liệu mới: QLDN_CAULONG_TEST';
GO
-- Kiểm tra dữ liệu trong DB mới
SELECT * FROM QLDN_CAULONG_TEST.dbo.SanPham;
GO
