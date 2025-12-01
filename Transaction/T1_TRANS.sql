--1. READ UNCOMMITTED (Đọc dữ liệu rác), BÀI NÀY T2 CHẠY TRƯỚC
--T1 
USE QLDN_CAULONG;

--step 2

-- Đặt mức cô lập thấp nhất

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; 

BEGIN TRAN;


-- T1 đọc dữ liệu (sẽ không bị T2 block)


SELECT MaChiTiet, GiaBan, SoLuongTon


FROM ChiTietSanPham


WHERE MaChiTiet = 1;


-- Kết quả: T1 thấy SoLuongTon = 40 (dữ liệu rác)

--step 4

-- T1 đọc lại


SELECT MaChiTiet, GiaBan, SoLuongTon FROM ChiTietSanPham WHERE MaChiTiet = 1;
-- Kết quả: T1 thấy SoLuongTon = 50
-- Dữ liệu T1 đọc ở bước 2 là sai!
COMMIT TRAN;
go

--2. READ COMMITED

--step 1 

USE QLDN_CAULONG;


-- Đặt mức cô lập mặc định


SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 


BEGIN TRAN;


-- T1 đọc lần 1


SELECT SoLuongTon FROM ChiTietSanPham WHERE MaChiTiet = 1;


-- Kết quả: 50

--step 3

-- T1 làm việc gì đó...


WAITFOR DELAY '00:00:05';


-- T1 đọc lại lần 2 (trong CÙNG 1 transaction)


SELECT SoLuongTon FROM ChiTietSanPham WHERE MaChiTiet = 1;


-- Kết quả: 40 (khác với lần 1)


-- Đây là Unrepeatable Read 


COMMIT TRAN;
go

--3. REPEATABLE READ (Bóng ma dữ liệu)
--step 1
USE QLDN_CAULONG;


SET TRANSACTION ISOLATION LEVEL REPEATABLE READ; 


BEGIN TRAN;


-- T1 đếm lần 1 (đọc 2 dòng chi tiết của MaSP=1)


SELECT COUNT(*) FROM ChiTietSanPham WHERE MaSanPham = 1;


-- Kết quả: 2

--step 3

-- T1 làm việc gì đó...


WAITFOR DELAY '00:00:05';


-- T1 đếm lại lần 2


SELECT COUNT(*) FROM ChiTietSanPham WHERE MaSanPham = 1;


-- Kết quả: 3 (Một dòng "bóng ma" đã xuất hiện) 


COMMIT TRAN;
go

--4. SERIALIZABLE (Giải quyết tất cả)
--step 1

USE QLDN_CAULONG;


SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; 


BEGIN TRAN;


-- T1 đếm lần 1 (và khóa phạm vi MaSP=1)


SELECT COUNT(*) FROM ChiTietSanPham WHERE MaSanPham = 1;


-- Kết quả: 3 (từ ví dụ trước)

--step 3

-- T1 làm việc gì đó...


WAITFOR DELAY '00:00:10';


-- T1 đếm lại lần 2


SELECT COUNT(*) FROM ChiTietSanPham WHERE MaSanPham = 1;


-- Kết quả: 3 (vẫn là 3, không đổi)


-- T1 kết thúc giao tác


COMMIT TRAN;
go

--Kịch bản kiểm tra Deadlock 
--T1
USE QLDN_CAULONG;
GO

-- Khôi phục dữ liệu gốc (chạy nếu test lại)
UPDATE ChiTietSanPham SET SoLuongTon = 50 WHERE MaChiTiet = 1;
UPDATE ChiTietSanPham SET GiaBan = 4799000 WHERE MaChiTiet = 3;
GO

PRINT N'T1: Bắt đầu giao tác';
BEGIN TRAN;

-- Bước 1: T1 khóa sản phẩm A (MaChiTiet = 1)
PRINT N'T1: Đang yêu cầu khóa XLOCK trên sản phẩm A (MaChiTiet = 1)...';
UPDATE ChiTietSanPham WITH (XLOCK, ROWLOCK)
SET SoLuongTon = SoLuongTon - 1
WHERE MaChiTiet = 1;
PRINT N'T1: ĐÃ KHÓA sản phẩm A. Đang chờ 10 giây...';

-- Chờ T2 khóa sản phẩm B
WAITFOR DELAY '00:00:10';

-- Bước 2: T1 cố gắng khóa sản phẩm B (MaChiTiet = 3)
PRINT N'T1: Đang yêu cầu khóa XLOCK trên sản phẩm B (MaChiTiet = 3)...';
UPDATE ChiTietSanPham WITH (XLOCK, ROWLOCK)
SET SoLuongTon = SoLuongTon - 1
WHERE MaChiTiet = 3;
PRINT N'T1: ĐÃ KHÓA sản phẩm B.';

COMMIT TRAN;
PRINT N'T1: Đã Commit.';