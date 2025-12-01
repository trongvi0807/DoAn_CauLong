-- Đặt biến cho tên cơ sở dữ liệu phục hồi
DECLARE @RestoreDBName NVARCHAR(100) = 'QLDN_CAULONG_RESTORE';

-- Đặt biến cho đường dẫn cơ bản
DECLARE @BasePath NVARCHAR(255) = 'C:\Users\nguye\Desktop\DoAn_CauLong\FILE_BACKUP\';

-- =================================================================
-- 1. PHỤC HỒI FULL BACKUP (BẮT BUỘC DÙNG NORECOVERY) - T1 Thứ Hai
-- =================================================================

RESTORE DATABASE @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T1_Thu2_Full.bak'
WITH FILE = 1,
     MOVE 'QLDN_CAULONG' TO 'C:\SQL_Data_Restore\QLDN_CAULONG_RESTORE.mdf',
     MOVE 'QLDN_CAULONG_Log' TO 'C:\SQL_Data_Restore\QLDN_CAULONG_RESTORE_Log.ldf', 
     REPLACE,
     STATS = 10,
     NORECOVERY; -- Bắt buộc cho phép áp dụng các bản backup tiếp theo
GO

PRINT N'Đã phục hồi Full Backup (T1 - Thứ 2). NORECOVERY.';

-- =================================================================
-- 2. PHỤC HỒI DIFFERENTIAL BACKUP (NORECOVERY) - T2 Thứ Ba
-- =================================================================

RESTORE DATABASE @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T2_Thu3_Diff.bak'
WITH FILE = 1,
     STATS = 10,
     NORECOVERY;
GO

PRINT N'Đã phục hồi Differential Backup (T2 - Thứ 3). NORECOVERY.';

-- =================================================================
-- 3. PHỤC HỒI CÁC BẢN TRANSACTION LOG LIÊN TIẾP (NORECOVERY)
-- =================================================================

-- T3 - Thứ Tư (Log)
RESTORE LOG @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T3_Thu4_Log.trn'
WITH FILE = 1,
     STATS = 10,
     NORECOVERY;
GO

PRINT N'Đã phục hồi Log Backup (T3 - Thứ 4). NORECOVERY.';

-- T4 - Thứ Năm (Log)
RESTORE LOG @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T4_Thu5_Log.trn'
WITH FILE = 1,
     STATS = 10,
     NORECOVERY;
GO

PRINT N'Đã phục hồi Log Backup (T4 - Thứ 5). NORECOVERY.';

-- =================================================================
-- 4. PHỤC HỒI DIFFERENTIAL BACKUP (NORECOVERY) - T5 Thứ Sáu
-- =================================================================

RESTORE DATABASE @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T5_Thu6_Diff.bak'
WITH FILE = 1,
     STATS = 10,
     NORECOVERY;
GO

PRINT N'Đã phục hồi Differential Backup (T5 - Thứ 6). NORECOVERY.';

-- =================================================================
-- 5. PHỤC HỒI CÁC BẢN TRANSACTION LOG SAU DIFFERENTIAL (T5)
-- =================================================================

-- T6 - Thứ Bảy (Log) - Dùng NORECOVERY
RESTORE LOG @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T6_Thu7_Log.trn'
WITH FILE = 1,
     STATS = 10,
     NORECOVERY;
GO

PRINT N'Đã phục hồi Log Backup (T6 - Thứ 7). NORECOVERY.';

-- T7 - Chủ Nhật (Log) - Dùng RECOVERY (Đây là lệnh cuối cùng)
RESTORE LOG @RestoreDBName
FROM DISK = @BasePath + 'QLDN_CAULONG_T7_CN_Log.trn'
WITH FILE = 1,
     STATS = 10,
     RECOVERY; -- **QUAN TRỌNG: Hoàn tất phục hồi và đưa DB vào trạng thái ONLINE**
GO

PRINT N'Đã phục hồi Log Backup cuối cùng (T7 - CN) và Database đã ONLINE.';

-- =================================================================
-- KIỂM TRA KẾT QUẢ
-- =================================================================

-- Chuyển sang DB mới và kiểm tra trạng thái
USE QLDN_CAULONG_RESTORE;
SELECT DB_NAME() AS DatabaseName, DATABASEPROPERTYEX(DB_NAME(), 'Status') AS DatabaseStatus, GETDATE() AS RestoreCompletionTime;
GO