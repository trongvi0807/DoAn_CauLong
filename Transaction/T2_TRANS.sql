--T2
--1. READ UNCOMMITTED (Đọc dữ liệu rác), BÀI NÀY T2 CHẠY TRƯỚC
USE QLDN_CAULONG

--step 1

BEGIN TRAN;

-- T2 Cập nhật tồn kho từ 50 -> 40

UPDATE ChiTietSanPham


SET SoLuongTon = 40


WHERE MaChiTiet = 1;


-- T2 CHƯA COMMIT

--step 3

-- T2 quyết định hủy bỏ thay đổi


ROLLBACK TRAN;
go

--2. READ COMMITED
 --step 2

 USE QLDN_CAULONG


BEGIN TRAN


-- T2 cập nhật tồn kho


UPDATE ChiTietSanPham


SET SoLuongTon = 40


WHERE MaChiTiet = 1;


-- T2 COMMIT thay đổi


COMMIT TRAN;
go

--3. REPEATABLE READ (Bóng ma dữ liệu)
--step 2
USE QLDN_CAULONG;


BEGIN TRAN;


-- T2 THÊM 1 chi tiết mới cho MaSP=1 (size 5U)


INSERT INTO ChiTietSanPham(MaSanPham, MaSize, GiaBan, SoLuongTon, SKU)


VALUES(1, 3, 4400000, 10, 'YN-AX100ZZ-5U');


-- Lệnh này THÀNH CÔNG vì Repeatable Read không khóa INSERT 



COMMIT TRAN;
go

--4. SERIALIZABLE (Giải quyết tất cả)
--step 2 

USE QLDN_CAULONG;


BEGIN TRAN;


-- T2 THÊM 1 chi tiết mới (ví dụ: màu Đỏ)


INSERT INTO ChiTietSanPham(MaSanPham, MaMau, GiaBan, SoLuongTon, SKU)


VALUES(1, 3, 4400000, 5, 'YN-AX100ZZ-DO-6U');


-- Lệnh này bị BLOCK (treo) vì T1 đang khóa phạm vi này
 
 --step 4

 -- Ngay sau khi T1 commit, lệnh INSERT của T2 mới được thực thi.

COMMIT TRAN;
go

--Kịch bản kiểm tra Deadlock
--T2

USE QLDN_CAULONG;
GO

PRINT N'T2: Bắt đầu giao tác';
BEGIN TRAN;

-- Bước 1: T2 khóa sản phẩm B (MaChiTiet = 3)
PRINT N'T2: Đang yêu cầu khóa XLOCK trên sản phẩm B (MaChiTiet = 3)...';
UPDATE ChiTietSanPham WITH (XLOCK, ROWLOCK)
SET GiaBan = GiaBan + 1000
WHERE MaChiTiet = 3;
PRINT N'T2: ĐÃ KHÓA sản phẩm B. Đang chờ 10 giây...';

-- Chờ T1 chạy xong Bước 1
WAITFOR DELAY '00:00:10';

-- Bước 2: T2 cố gắng khóa sản phẩm A (MaChiTiet = 1)
PRINT N'T2: Đang yêu cầu khóa XLOCK trên sản phẩm A (MaChiTiet = 1)...';
UPDATE ChiTietSanPham WITH (XLOCK, ROWLOCK)
SET GiaBan = GiaBan + 1000
WHERE MaChiTiet = 1;
PRINT N'T2: ĐÃ KHÓA sản phẩm A.';

COMMIT TRAN;
PRINT N'T2: Đã Commit.';