CREATE DATABASE QLDN_CAULONG
GO

USE QLDN_CAULONG
GO

-- Bảng PhanQuyen
CREATE TABLE PhanQuyen (
    MaQuyen INT PRIMARY KEY IDENTITY(1,1),
    TenQuyen NVARCHAR(50) NOT NULL
);

-- Bảng TaiKhoan
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    Email VARCHAR(100),
    MaQuyen INT,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaQuyen) REFERENCES PhanQuyen(MaQuyen)
);

-- Bảng LoaiSanPham
CREATE TABLE LoaiSanPham (
    MaLoai INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);

-- Bảng NhaCungCap
CREATE TABLE NhaCungCap (
    MaNhaCungCap INT PRIMARY KEY IDENTITY(1,1),
    TenNhaCungCap NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai VARCHAR(15),
    Email VARCHAR(100)
);

--Bảng Hang
CREATE TABLE Hang (
    MaHang INT PRIMARY KEY IDENTITY(1,1),
    TenHang NVARCHAR(100) NOT NULL,
    QuocGia NVARCHAR(50) NULL
);

-- Bảng KhuyenMai 
CREATE TABLE KhuyenMai (
    MaKhuyenMai INT PRIMARY KEY IDENTITY(1,1),
    TenChuongTrinh NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255),
    NgayBatDau DATE,
    NgayKetThuc DATE,
    PhanTramGiam DECIMAL(5,2) CHECK (PhanTramGiam >= 0 AND PhanTramGiam <= 100),
    GiamToiDa DECIMAL(15,2) NULL,
    SoLuongSuDung INT NULL
);

-- Bảng MauSac
CREATE TABLE MauSac (
    MaMau INT PRIMARY KEY IDENTITY(1,1),
    TenMau NVARCHAR(50) NOT NULL
);

-- Bảng Size
CREATE TABLE Size (
     MaSize INT PRIMARY KEY IDENTITY(1,1),
    TenSize VARCHAR(20) NOT NULL,
    LoaiSize NVARCHAR(30) NOT NULL CHECK (LoaiSize IN ('AoQuan', 'Giay', 'Vot'))
);

-- Bảng SanPham
CREATE TABLE SanPham (
    MaSanPham INT PRIMARY KEY IDENTITY(1,1),
    TenSanPham NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(MAX),
    GiaGoc DECIMAL(15,2) CHECK (GiaGoc >= 0),
    HinhAnhDaiDien VARCHAR(255),
    MaLoai INT,
    MaNhaCungCap INT,
	MaHang INT,
    MaKhuyenMai INT NULL,
    CoSize BIT DEFAULT 0,
    CoMau BIT DEFAULT 0,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai),
    FOREIGN KEY (MaNhaCungCap) REFERENCES NhaCungCap(MaNhaCungCap),
    FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai),
	FOREIGN KEY (MaHang) REFERENCES Hang(MaHang)
);

-- Bảng ChiTietSanPham
CREATE TABLE ChiTietSanPham (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),
    MaSanPham INT,
    MaMau INT NULL,
    MaSize INT NULL,
    GiaBan DECIMAL(15,2) CHECK (GiaBan >= 0),
    SoLuongTon INT CHECK (SoLuongTon >= 0),
    SKU VARCHAR(50) UNIQUE NOT NULL,
    HinhAnh VARCHAR(255),
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham) ON DELETE CASCADE,
    FOREIGN KEY (MaMau) REFERENCES MauSac(MaMau),
    FOREIGN KEY (MaSize) REFERENCES Size(MaSize)
);

-- Bảng ThongSoVot
CREATE TABLE ThongSoVot (
    MaThongSo INT PRIMARY KEY IDENTITY(1,1),
    MaChiTiet INT,
    DoCanBang NVARCHAR(50),
    TrongLuong VARCHAR(20),
    DoCung NVARCHAR(50),
    ChieuDai NVARCHAR(20),
    SucCang NVARCHAR(20),
    FOREIGN KEY (MaChiTiet) REFERENCES ChiTietSanPham(MaChiTiet)
);

-- Bảng KhachHang
CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(255),
    MaTaiKhoan INT UNIQUE,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

-- Bảng DonHang
CREATE TABLE DonHang (
    MaDonHang INT PRIMARY KEY IDENTITY(1,1),
    NgayDat DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'Đang xử lý',
    TongTien DECIMAL(15,2) CHECK (TongTien >= 0),
    TongTienSauGiam DECIMAL(15,2) CHECK (TongTienSauGiam >= 0),
    MaKhachHang INT,
    TienGiam DECIMAL(15,2) DEFAULT 0,
    DiaChiGiaoHang NVARCHAR(255),
    SoDienThoaiNhanHang VARCHAR(15),
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);

-- Bảng ChiTietDonHang
CREATE TABLE ChiTietDonHang (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),
    MaDonHang INT,
    MaChiTietSanPham INT,
    SoLuong INT CHECK (SoLuong > 0),
    DonGia DECIMAL(15,2) CHECK (DonGia >= 0),
    ThanhTien DECIMAL(15,2) CHECK (ThanhTien >= 0),
    FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang),
    FOREIGN KEY (MaChiTietSanPham) REFERENCES ChiTietSanPham(MaChiTiet)
);

-- Bảng PhanHoi
CREATE TABLE PhanHoi (
    MaPhanHoi INT PRIMARY KEY IDENTITY(1,1),
    NoiDung NVARCHAR(255),
    NgayPhanHoi DATETIME DEFAULT GETDATE(),
    DanhGia INT CHECK (DanhGia >= 1 AND DanhGia <= 5),
    MaKhachHang INT,
    MaSanPham INT,
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
);

-- Bảng GioHang
CREATE TABLE GioHang (
    MaGioHang INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT,
    MaChiTietSanPham INT,
    SoLuong INT CHECK (SoLuong > 0),
    NgayThem DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    FOREIGN KEY (MaChiTietSanPham) REFERENCES ChiTietSanPham(MaChiTiet)
);
GO

-- =====================================
-- 1 BẢNG PHANQUYEN
-- =====================================
INSERT INTO PhanQuyen (TenQuyen)VALUES 
(N'Quản trị'),
(N'Nhân viên'),
(N'Khách hàng');
GO

-- =====================================
-- 2 BẢNG LOAISANPHAM
-- =====================================
INSERT INTO LoaiSanPham (TenLoai, MoTa)
VALUES (N'Vợt cầu lông', N'Dành cho người chơi chuyên nghiệp và phong trào'),
       (N'Giày cầu lông', N'Giày thi đấu và tập luyện'),
       (N'Áo quần thể thao', N'Trang phục cầu lông'),
       (N'Phụ kiện cầu lông', N'Túi, băng tay, quấn cán...');
GO

-- =====================================
-- 3 BẢNG NHACUNGCAP
-- =====================================
INSERT INTO NhaCungCap (TenNhaCungCap, DiaChi, SoDienThoai, Email)VALUES 
(N'Công ty TNHH Thể thao Đức Phát', N'25 Hoàng Hoa Thám, Q.Tân Bình, TP.HCM', '0905123456', 'sales@ducphat.vn'),
(N'Công ty TNHH Cầu Lông Sunrise Việt Nam', N'22 Nguyễn Văn Trỗi, Q.Phú Nhuận, TP.HCM', '0908789123', 'info@sunrisevn.com'),
(N'Công ty TNHH Thể thao Đại Hưng', N'19 Nguyễn Hữu Cảnh, Q.Bình Thạnh, TP.HCM', '0903332121', 'contact@daihung.vn'),
(N'Công ty TNHH SportPro Việt Nam', N'88 Trần Hưng Đạo, Q.1, TP.HCM', '0918234567', 'support@sportpro.vn'),
(N'Công ty TNHH Thể thao Phúc Gia', N'12 Cầu Giấy, Hà Nội', '0934556677', 'info@phucgiasport.vn');
GO

-- =====================================
-- 4 BẢNG HANG (hãng vợt)
-- =====================================
INSERT INTO Hang (TenHang, QuocGia)
VALUES (N'Yonex', N'Nhật Bản'),
       (N'Lining', N'Trung Quốc'),
       (N'Victor', N'Đài Loan'),
       (N'Mizuno', N'Nhật Bản');
GO

-- =====================================
-- 5 BẢNG KHUYENMAI
-- =====================================
INSERT INTO KhuyenMai (TenChuongTrinh, MoTa, NgayBatDau, NgayKetThuc, PhanTramGiam, GiamToiDa, SoLuongSuDung)VALUES
(N'Đại lễ 30/4 - 1/5', N'Mừng ngày giải phóng miền Nam và Quốc tế lao động', '2025-04-25', '2025-05-05', 10, 150000, 200),
(N'Chào Hè Sôi Động', N'Giảm giá dụng cụ thể thao mùa hè', '2025-06-01', '2025-06-30', 12, 200000, 150),
(N'Back To School', N'Đồng hành cùng học sinh sinh viên', '2025-08-15', '2025-09-10', 10, 100000, 300),
(N'Siêu Sale 9.9', N'Săn sale ngày đôi tháng 9', '2025-09-09', '2025-09-11', 25, 500000, 50),
(N'Black Friday 2025', N'Lễ hội mua sắm lớn nhất năm', '2025-11-28', '2025-11-30', 40, 1000000, 100),
(N'Giáng Sinh An Lành', N'Quà tặng giáng sinh cho lông thủ', '2025-11-20', '2025-12-25', 15, 200000, 100),
(N'Xả kho cuối năm', N'Thanh lý các mẫu vợt đời cũ', '2025-12-26', '2025-12-31', 50, NULL, 20),
(N'Khách hàng thân thiết', N'Tri ân khách hàng đã mua trên 5 triệu', '2025-01-01', '2025-12-31', 5, 500000, NULL);
GO

-- =====================================
-- 6 BẢNG MAUSAC
-- =====================================
INSERT INTO MauSac (TenMau)
VALUES (N'Đen'),--1
       (N'Xanh dương'),--2
       (N'Đỏ'),--3
       (N'Vàng'),--4
       (N'Bạc'),--5
       (N'Trắng'),--6
	   (N'TÍm'),--7
	   (N'Xanh lá'),--8
	   (N'Cam'),--9
	   (N'Hồng');--10
GO

-- =====================================
-- 7 BẢNG SIZE
-- =====================================
INSERT INTO Size (TenSize, LoaiSize)
VALUES ('3U', N'Vot'),--1
       ('4U', N'Vot'),--2
	   ('5U', N'Vot'),--3
       ('S', N'AoQuan'),--4
	   ('M', N'AoQuan'),--5
	   ('L', N'AoQuan'),--6
	   ('XL', N'AoQuan'),--7
	   ('2XL', N'AoQuan'),--8
	   ('30', N'Giay'),--9
	   ('31', N'Giay'),--10
	   ('32', N'Giay'),--11
	   ('33', N'Giay'),--12
	   ('34', N'Giay'),--13
	   ('35', N'Giay'),--14
	   ('36', N'Giay'),--15
	   ('37', N'Giay'),--16
       ('38', N'Giay'),--17
	   ('39', N'Giay'),--18
	   ('40', N'Giay'),--19
	   ('41', N'Giay'),--20
	   ('42', N'Giay'),--21
       ('43', N'Giay'),--22
	   ('44', N'Giay'),--23
	   ('45', N'Giay'),--24
	   ('46', N'Giay'),--25
	   ('47', N'Giay');--26
GO

INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Email, MaQuyen)
VALUES 
('admin', 'admin@123', 'admin@shopcaulong.com', 1),
('nhanvien_banhang', 'nv@123', 'nhanvien1@shopcaulong.com', 2),
('nguyenvana', 'vana@123', 'nguyenvana@gmail.com', 3),
('tranthib', 'thib@123', 'tranthib@gmail.com', 3);
GO

INSERT INTO KhachHang (HoTen, Email, SoDienThoai, DiaChi, MaTaiKhoan)
VALUES
(N'Admin', 'admin@shopcaulong.com', '0000000000', N'Tại cửa hàng', 1),
(N'Nhân Viên', 'nhanvien1@shopcaulong.com', '0000000000', N'Tại cửa hàng', 2),
(N'Nguyễn Văn A', 'nguyenvana@gmail.com', '0901234567', N'123 Nguyễn Trãi, Q.1, TP.HCM', 3),
(N'Trần Thị B', 'tranthib@gmail.com', '0909876543', N'456 Lê Lợi, Q.3, TP.HCM', 4);
GO

INSERT INTO SanPham (TenSanPham, MoTa, GiaGoc, HinhAnhDaiDien, MaLoai, MaNhaCungCap, MaHang, MaKhuyenMai, CoSize, CoMau)
VALUES 
--Vợt
(N'Vợt Cầu Lông Yonex Astrox 100ZZ', N'Yonex Astrox 100ZZ Kurenai là phiên bản nâng cấp của Astrox 100ZZ (ra mắt 2020), nổi bật với màu đỏ thẫm và công nghệ cải tiến cho lối chơi tấn công mạnh mẽ. Đây là cây vợt tiêu biểu của Yonex, được nhiều vận động viên hàng đầu như Viktor Axelsen (top 1 thế giới) sử dụng.', 4500000, 'astrox100zz.jpg', 1, 2, 1, NULL, 1, 1),
(N'Vợt Cầu Lông Yonex Astrox 88D Pro 2024', N'Chuyên dụng cho đánh đôi, vị trí cầu sau, thiên về tấn công.', 4799000, 'astrox88dpro.jpg', 1, 2, 1, NULL, 1, 1),
(N'Vợt Cầu Lông Yonex Astrox 88S Pro 2024', N'Chuyên dụng cho đánh đôi, vị trí cầu trước, thiên về tốc độ và kiểm soát.', 4799000, 'astrox88spro.jpg', 1, 2, 1, NULL, 1, 1),
(N'Vợt Cầu Lông Yonex Astrox 99 Pro', N'Vợt nặng đầu, thiên công, dành cho VĐV chuyên nghiệp.', 4190000, 'astrox99pro.jpg', 1, 2, 1, 8, 1, 1), -- Áp dụng Black Friday
(N'Vợt Cầu Lông Yonex Astrox 77 Pro', N'Vợt cân bằng, linh hoạt, dễ chơi, phù hợp cho người chơi phong trào.', 3900000, 'astrox77pro.jpg', 1, 2, 1, NULL, 1, 1),
(N'Vợt Cầu Lông Yonex Nanoflare 1000Z', N'Dòng vợt nhẹ đầu, tốc độ nhanh nhất, phù hợp cho phản tạt.', 4600000, 'nanoflare1000z.jpg', 1, 2, 1, NULL, 1, 1),
(N'Vợt Cầu Lông Yonex Nanoflare 700 Pro', N'Vợt nhẹ đầu, tốc độ cao, hỗ trợ phản tạt nhanh.', 4489000, 'nanoflare700pro.jpg', 1, 2, 1, 8, 1, 1), -- Áp dụng Chào Hè
(N'Vợt Cầu Lông Yonex Arcsaber 11 Pro', N'Dòng vợt cân bằng, kiểm soát cầu tuyệt vời, huyền thoại trở lại.', 4100000, 'arcsaber11pro.jpg', 1, 2, 1, NULL, 1, 1),

(N'Vợt Cầu Lông Lining Aeronaut 9000C', N'Dòng vợt tấn công, nặng đầu, khung vát gió', 4300000, 'lining_aero9000c.jpg', 1, 2, 2, NULL, 1, 1),
(N'Vợt Cầu Lông Lining Aeronaut 9000I', N'Dòng vợt chuyên đánh tốc độ, phản tạt nhanh', 4300000, 'lining_aero9000i.jpg', 1, 2, 2, 8, 1, 1), -- Áp dụng KM 30/4
(N'Vợt Cầu Lông Lining Tectonic 7', N'Vợt công thủ toàn diện, dễ thuần', 3200000, 'lining_tectonic7.jpg', 1, 2, 2, NULL, 1, 1),
(N'Vợt Cầu Lông Lining Axforce 90 Max', N'Vợt "Rồng" chuyên công, rất nặng đầu', 4100000, 'lining_axforce90max.jpg', 1, 2, 2, NULL, 1, 1),
(N'Vợt Cầu Lông Lining Axforce 80', N'Vợt tấn công, trợ lực tốt, dễ chơi hơn Axforce 90', 3900000, 'lining_axforce80.jpg', 1, 2, 2, 8, 1, 1), -- Áp dụng Black Friday
(N'Vợt Cầu Lông Lining Calibar 600', N'Vợt tầm trung, thiên công, giá cả phải chăng', 3100000, 'lining_calibar600.jpg', 1, 2, 2, NULL, 1, 1),
(N'Vợt Cầu Lông Lining Bladex 800', N'Dòng vợt mới, khung sắc, tốc độ nhanh', 4200000, 'lining_bladex800.jpg', 1, 2, 2, NULL, 1, 1),
(N'Vợt Cầu Lông Lining Tectonic 9', N'Vợt cao cấp, nặng đầu, tăng cường lực đập', 3500000, 'lining_tectonic9.jpg', 1, 2, 2, NULL, 1, 1),

(N'Vợt Cầu Lông Victor Auraspeed 100X Ultra', N'Dòng vợt tốc độ, linh hoạt, công thủ toàn diện', 4100000, 'victor_as100x_ultra.jpg', 1, 3, 3, NULL, 1, 1),
(N'Vợt Cầu Lông Victor TK Ryuga Metallic', N'Vợt chuyên công, thiết kế mạnh mẽ, đập cầu uy lực', 3500000, 'victor_ryuga_metallic.jpg', 1, 3, 3, NULL, 1, 1),
(N'Vợt Cầu Lông Victor Brave Sword 12 Pro', N'Huyền thoại vợt "Kiếm", khung vát kim cương, tốc độ cực nhanh', 3200000, 'victor_brs12_pro.jpg', 1, 3, 3, NULL, 1, 1),
(N'Vợt Cầu Lông Victor Jetspeed S 10', N'Vợt tốc độ, nhẹ, phản tạt nhanh, dễ điều khiển', 2850000, 'victor_js10.jpg', 1, 3, 3, 8, 1, 1), -- Áp dụng KM Giáng Sinh
(N'Vợt Cầu Lông Victor DriveX 12', N'Vợt công thủ toàn diện, kiểm soát cầu tốt', 3800000, 'victor_dx12.jpg', 1, 3, 3, NULL, 1, 1),
(N'Vợt Cầu Lông Victor Thruster F Claw', N'Vợt thiên công, nặng đầu, biệt danh "Móng Vuốt"', 2150000, 'victor_tfclaw.jpg', 1, 3, 3, 8, 1, 1), -- Áp dụng Black Friday

(N'Vợt Cầu Lông Mizuno Fortius 10 Power', N'Vợt cao cấp, chuyên công, sản xuất tại Nhật Bản', 4800000, 'mizuno_f10power.jpg', 1, 4, 4, NULL, 1, 1),
(N'Vợt Cầu Lông Mizuno Fortius 10 Quick', N'Vợt cao cấp, chuyên tốc độ, sản xuất tại Nhật Bản', 4800000, 'mizuno_f10quick.jpg', 1, 4, 4, NULL, 1, 1),
(N'Vợt Cầu Lông Mizuno JPX Reserve Edition', N'Phiên bản đặc biệt, công thủ toàn diện', 4990000, 'mizuno_jpx_reserve.jpg', 1, 4, 4, 8, 1, 1), -- Áp dụng Xả Kho
(N'Vợt Cầu Lông Mizuno Fortius 70', N'Vợt tầm trung, nặng đầu, hỗ trợ tấn công', 3599000, 'mizuno_fortius70.jpg', 1, 4, 4, NULL, 1, 1),
(N'Vợt Cầu Lông Mizuno Altius 01 Speed Limited', N'Vợt cân bằng, linh hoạt, dễ điều khiển', 4500000, 'mizuno_altius01speedlimited.jpg', 1, 4, 4, NULL, 1, 1),

--Giày
(N'Giày Cầu Lông Yonex SHB 65Z4', N'Dòng giày toàn diện, êm ái, bám sân', 2800000, 'yonex_65z4.jpg', 2, 1, 1, NULL, 1, 1),
(N'Giày Cầu Lông Yonex Eclipsion Z3', N'Tăng cường độ ổn định, chống lật cổ chân', 3100000, 'yonex_eclipsion_trang_z3.jpg', 2, 1, 1, 1, 1, 1), -- KM 30/4
(N'Giày Cầu Lông Yonex Aerus Z2', N'Siêu nhẹ, tăng tốc độ di chuyển', 3000000, 'yonex_aerus_z2_den.jpg', 2, 1, 1, NULL, 1, 1),

(N'Giày Cầu Lông Lining AYAS012-1', N'Dòng "Lưỡi Dao Bóng Tối", bám sân, ổn định', 3000000, 'lining_ayas012_den.jpg', 2, 2, 2, NULL, 1, 1),
(N'Giày Cầu Lông Lining AYZU017-1', N'Giày bền bỉ, công nghệ Bounse+', 2160000, 'lining_ayzu017.jpg', 2, 2, 2, NULL, 1, 1),
(N'Giày Cầu Lông Lining AYAS018-6', N'Giày tầm trung, êm ái, thiết kế đẹp', 2450000, 'lining_ayas018.jpg', 2, 2, 2, 3, 1, 1), -- KM Back to School

(N'Giày Cầu Lông Victor A970ACE', N'Phiên bản đặc biệt, ổn định, êm ái', 2190000, 'victor_a970.jpg', 2, 3, 3, 5, 1, 1), -- KM Black Friday
(N'Giày Cầu Lông Victor P9200III', N'Dòng P9200 huyền thoại, siêu bền', 2140000, 'victor_p9200iii.jpg', 2, 3, 3, NULL, 1, 1),

(N'Giày Cầu Lông Mizuno Wave Claw Neo 2', N'Tốc độ, êm ái, công nghệ Enerzy', 2750000, 'mizuno_waveclaw_neo2_xanh.jpg', 2, 4, 4, NULL, 1, 1),
(N'Giày Cầu Lông Mizuno Wave Fang Pro', N'Dòng giày ổn định, da thật, sản xuất tại Nhật', 3900000, 'mizuno_fangpro.jpg', 2, 4, 4, NULL, 1, 1),

--Áo/Quần
(N'Áo Cầu Lông Yonex TRM3089', N'Áo thi đấu đội tuyển, chất liệu poly', 119000, 'yonex_trm3089.jpg', 3, 1, 1, 2, 1, 1), -- KM Chào Hè
(N'Áo Cầu Lông Yonex TRM3053 ', N'Áo thi đấu đội tuyển, chất liệu poly', 169000, 'yonex_trm3053.jpg', 3, 1, 1, NULL, 1, 1),
(N'Quần Cầu Lông Yonex TSM3085', N'Quần short thể thao, co giãn 4 chiều', 139000, 'yonex_tsm3085.jpg', 3, 1, 1, NULL, 1, 1),

(N'Áo Cầu Lông Lining A421 ', N'Áo thi đấu đội tuyển Trung Quốc 2024', 130000, 'lining_a421.jpg', 3, 2, 2, NULL, 1, 1),
(N'Áo Cầu Lông Lining 3715', N'Áo thi đấu đội tuyển Trung Quốc 2024', 160000, 'lining_3715.jpg', 3, 2, 2, NULL, 1, 1),
(N'Quần Cầu Lông Lining Q21', N'Quần short, chất liệu thoáng khí', 110000, 'lining_q21.jpg', 3, 2, 2, 2, 1, 1); -- KM Chào Hè
GO

INSERT INTO ChiTietSanPham (MaSanPham, MaMau, MaSize, GiaBan, SoLuongTon, SKU, HinhAnh)
VALUES 
--Vợt
(1, 1, 1, 4500000, 50, 'YN-AX100ZZ-3U', 'astrox100zz.jpg'), -- MaChiTiet = 1
(1, 1, 2, 4500000, 100, 'YN-AX100ZZ-4U', 'astrox100zz.jpg'),-- MaChiTiet = 2
(2, 2, 1, 4799000, 100, 'YN-AX88DPRO2024-3U', 'astrox88dpro.jpg'),-- MaChiTiet = 3
(2, 2, 2, 4799000, 100, 'YN-AX88DPRO2024-4U', 'astrox88dpro.jpg'),-- MaChiTiet = 4
(3, 2, 1, 4799000, 100, 'YN-AX88SPRO2024-3U', 'astrox88spro.jpg'),-- MaChiTiet = 5
(3, 2, 2, 4799000, 100, 'YN-AX88SPRO2024-4U', 'astrox88spro.jpg'),-- MaChiTiet = 6
(4, 3, 1, 4190000, 50, 'YN-AX99PRO-DO-3U', 'astrox99pro.jpg'), -- MaChiTiet = 7
(4, 3, 2, 4190000, 100, 'YN-AX99PRO-DO-4U', 'astrox99pro.jpg'),-- MaChiTiet = 8
(4, 6, 1, 4190000, 50, 'YN-AX99PRO-TRANG-3U', 'astrox99protrang.jpg'), -- MaChiTiet = 7
(4, 6, 2, 4190000, 100, 'YN-AX99PRO-TRANG-4U', 'astrox99protrang.jpg'),-- MaChiTiet = 8
(5, 3, 1, 3900000, 50, 'YN-AX77PRO-DO-3U', 'astrox77pro.jpg'), -- MaChiTiet = 9
(5, 3, 2, 3900000, 100, 'YN-AX77PRO-DO-4U', 'astrox77pro.jpg'),-- MaChiTiet = 10
(6, 4, 1, 4600000, 50, 'YN-NNF1000Z-VANG-3U', 'nanoflare1000z.jpg'), -- MaChiTiet = 11
(6, 4, 2, 4600000, 100, 'YN-NNF1000Z-VANG-4U', 'nanoflare1000z.jpg'),-- MaChiTiet = 12
(7, 1, 1, 4489000, 50, 'YN-NNF700PRO-DEN-4U', 'nanoflare700pro.jpg'), -- MaChiTiet = 13
(8, 3, 1, 4100000, 50, 'YN-ARC11PRO-DO-3U', 'arcsaber11pro.jpg'), -- MaChiTiet = 14
(8, 3, 2, 4100000, 100, 'YN-ARC11PRO-DO-4U', 'arcsaber11pro.jpg'),-- MaChiTiet = 15

(9, 2, 1, 4300000, 100, 'LN-AER9000C-XANH-3U', 'lining_aero9000c.jpg'),-- MaChiTiet = 17
(9, 2, 2, 4300000, 100, 'LN-AER9000C-XANH-4U', 'lining_aero9000c.jpg'),-- MaChiTiet = 18
(10, 7, 3, 4300000, 100, 'LN-AER9000I-TIM-5U', 'lining_aero9000i.jpg'),-- MaChiTiet = 19
(11, 6, 2, 3200000, 100, 'LN-TEC7-TRANG-4U', 'lining_tectonic7.jpg'),-- MaChiTiet = 20
(12, 2, 2, 4100000, 40, 'LN-AX90-XANH-4U', 'lining_axforce90max.jpg'),     -- MaChiTiet = 21
(13, 1, 1, 3900000, 40, 'LN-AX80-DEN-3U', 'lining_axforce80.jpg'),     -- MaChiTiet = 22
(13, 1, 2, 3900000, 40, 'LN-AX80-DEN-4U', 'lining_axforce80.jpg'),     -- MaChiTiet = 23
(13, 1, 3, 3900000, 40, 'LN-AX80-DEN-5U', 'lining_axforce80.jpg'),     -- MaChiTiet = 24
(14, 1, 2, 3100000, 40, 'LN-CALIBAR600-VANG-4U', 'lining_calibar600.jpg'),     -- MaChiTiet = 25
(15, 3, 1, 4200000, 40, 'LN-BLADEX800-DO-3U', 'lining_bladex800.jpg'),     -- MaChiTiet = 26
(15, 3, 2, 4200000, 40, 'LN-BLADEX800-DO-4U', 'lining_bladex800.jpg'),     -- MaChiTiet = 27
(15, 1, 1, 4200000, 40, 'LN-BLADEX800-XANH-3U', 'lining_bladex800_xanh.jpg'),     -- MaChiTiet = 28
(15, 2, 2, 4200000, 40, 'LN-BLADEX800-XANH-4U', 'lining_bladex800_xanh.jpg'),     -- MaChiTiet = 28
(16, 6, 1, 3500000, 40, 'LN-TEC9-TRANG-3U', 'lining_tectonic9.jpg'),     -- MaChiTiet = 29
(16, 6, 2, 3500000, 40, 'LN-TEC9-TRANG-4U', 'lining_tectonic9.jpg'),     -- MaChiTiet = 30
(16, 6, 3, 3500000, 40, 'LN-TEC9-TRANG-5U', 'lining_tectonic9.jpg'),     -- MaChiTiet = 31

(17, 1, 1, 4100000, 40, 'VT-ARS100XULTRA-DEN-3U', 'victor_as100x_ultra.jpg'),     -- MaChiTiet = 32
(17, 1, 2, 4100000, 40, 'VT-ARS100XULTRA-DEN-4U', 'victor_as100x_ultra.jpg'),     -- MaChiTiet = 33
(18, 3, 1, 3500000, 40, 'VT-RYUGA-METALLIC-DO-3U', 'victor_ryuga_metallic.jpg'),     -- MaChiTiet = 34
(18, 3, 2, 3500000, 40, 'VT-RYUGA-METALLIC-DO-4U', 'victor_ryuga_metallic.jpg'),     -- MaChiTiet = 35
(19, 2, 1, 3200000, 40, 'VT-BS12PRO-XANH-3U', 'victor_brs12_pro.jpg'),     -- MaChiTiet = 36
(19, 2, 2, 3200000, 40, 'VT-BS12PRO-XANH-4U', 'victor_brs12_pro.jpg'),     -- MaChiTiet = 37
(20, 1, 1, 2850000, 40, 'VT-JS10-DEN-3U', 'victor_js10.jpg'),     -- MaChiTiet = 38
(20, 1, 2, 2850000, 40, 'VT-JS10-DEN-4U', 'victor_js10.jpg'),     -- MaChiTiet = 39
(21, 9, 1, 3800000, 40, 'VT-DX12-CAM-3U', 'victor_dx12.jpg'),     -- MaChiTiet = 40
(21, 9, 2, 3800000, 40, 'VT-DX12-CAM-4U', 'victor_dx12.jpg'),     -- MaChiTiet = 41
(22, 6, 2, 2150000, 40, 'VT-THRUSTER-F-CLAW-TRANG-4U', 'victor_tfclaw.jpg'),     -- MaChiTiet = 42

(23, 1, 2, 4800000, 40, 'MIZUNO-F10POWER-DEN-4U', 'mizuno_f10power.jpg'),     -- MaChiTiet = 43
(24, 1, 2, 4800000, 40, 'MIZUNO-F10QUICK-DEN-4U', 'mizuno_f10quick.jpg'),     -- MaChiTiet = 44
(25, 1, 1, 4990000, 40, 'MIZUNO-JPX-RESERVE-EDITION-DEN-3U', 'mizuno_jpx_reserve.jpg'),     -- MaChiTiet = 45
(25, 1, 2, 4990000, 40, 'MIZUNO-JPX-RESERVE-EDITION-DEN-4U', 'mizuno_jpx_reserve.jpg'),     -- MaChiTiet = 46
(26, 1, 2, 3599000, 40, 'MIZUNO-F70-DEN-4U', 'mizuno_fortius70.jpg'),     -- MaChiTiet = 47
(27, 1, 2, 4500000, 40, 'MIZUNO-ALTIUS-01SPEEDLIMITED-DEN-4U', 'mizuno_altius01speedlimited.jpg'),     -- MaChiTiet = 48

(28, 6, 14, 2800000, 30, 'YN-SHB65Z4-TRANG-35', 'yonex_65z4.jpg'),   -- MaChiTiet = 49
(28, 6, 15, 2800000, 30, 'YN-SHB65Z4-TRANG-36', 'yonex_65z4.jpg'),   -- MaChiTiet = 50
(28, 6, 16, 2800000, 30, 'YN-SHB65Z4-TRANG-37', 'yonex_65z4.jpg'),   -- MaChiTiet = 51
(28, 6, 17, 2800000, 30, 'YN-SHB65Z4-TRANG-38', 'yonex_65z4.jpg'),   -- MaChiTiet = 52
(28, 6, 18, 2800000, 30, 'YN-SHB65Z4-TRANG-39', 'yonex_65z4.jpg'),   -- MaChiTiet = 53
(28, 6, 19, 2800000, 30, 'YN-SHB65Z4-TRANG-40', 'yonex_65z4.jpg'),   -- MaChiTiet = 54
(28, 6, 20, 2800000, 30, 'YN-SHB65Z4-TRANG-41', 'yonex_65z4.jpg'),   -- MaChiTiet = 55
(28, 6, 21, 2800000, 30, 'YN-SHB65Z4-TRANG-42', 'yonex_65z4.jpg'),   -- MaChiTiet = 56
(28, 6, 22, 2800000, 30, 'YN-SHB65Z4-TRANG-43', 'yonex_65z4.jpg'),   -- MaChiTiet = 57
(28, 6, 23, 2800000, 30, 'YN-SHB65Z4-TRANG-44', 'yonex_65z4.jpg'),   -- MaChiTiet = 58
(28, 6, 24, 2800000, 30, 'YN-SHB65Z4-TRANG-45', 'yonex_65z4.jpg'),   -- MaChiTiet = 59
(28, 6, 25, 2800000, 30, 'YN-SHB65Z4-TRANG-46', 'yonex_65z4.jpg'),   -- MaChiTiet = 60
(28, 6, 26, 2800000, 30, 'YN-SHB65Z4-TRANG-47', 'yonex_65z4.jpg'),   -- MaChiTiet = 61
(29, 6, 14, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-35', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 62
(29, 6, 15, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-36', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 63
(29, 6, 16, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-37', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 64
(29, 6, 17, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-38', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 65
(29, 6, 18, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-39', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 66
(29, 6, 19, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-40', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 67
(29, 6, 20, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-41', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 68
(29, 6, 21, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-42', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 69
(29, 6, 22, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-43', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 70
(29, 6, 23, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-44', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 71
(29, 6, 24, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-45', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 72
(29, 6, 25, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-46', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 73
(29, 6, 26, 3100000, 30, 'YN-ECLIPSONZ3-TRANG-47', 'yonex_eclipsion_trang_z3.jpg'),   -- MaChiTiet = 74
(29, 2, 14, 3100000, 30, 'YN-ECLIPSONZ3-XANH-35', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 75
(29, 2, 15, 3100000, 30, 'YN-ECLIPSONZ3-XANH-36', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 76
(29, 2, 16, 3100000, 30, 'YN-ECLIPSONZ3-XANH-37', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 77
(29, 2, 17, 3100000, 30, 'YN-ECLIPSONZ3-XANH-38', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 78
(29, 2, 18, 3100000, 30, 'YN-ECLIPSONZ3-XANH-39', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 79
(29, 2, 19, 3100000, 30, 'YN-ECLIPSONZ3-XANH-40', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 80
(29, 2, 20, 3100000, 30, 'YN-ECLIPSONZ3-XANH-41', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 81
(29, 2, 21, 3100000, 30, 'YN-ECLIPSONZ3-XANH-42', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 82
(29, 2, 22, 3100000, 30, 'YN-ECLIPSONZ3-XANH-43', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 83
(29, 2, 23, 3100000, 30, 'YN-ECLIPSONZ3-XANH-44', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 84
(29, 2, 24, 3100000, 30, 'YN-ECLIPSONZ3-XANH-45', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 85
(29, 2, 25, 3100000, 30, 'YN-ECLIPSONZ3-XANH-46', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 86
(29, 2, 26, 3100000, 30, 'YN-ECLIPSONZ3-XANH-47', 'yonex_eclipsion_xanh_z3.jpg'),   -- MaChiTiet = 87
(30, 1, 14, 3000000, 30, 'YN-AERUSZ2-DEN-35', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 1, 15, 3000000, 30, 'YN-AERUSZ2-DEN-36', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 89
(30, 1, 16, 3000000, 30, 'YN-AERUSZ2-DEN-37', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 90
(30, 1, 17, 3000000, 30, 'YN-AERUSZ2-DEN-38', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 91
(30, 1, 18, 3000000, 30, 'YN-AERUSZ2-DEN-39', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 92
(30, 1, 19, 3000000, 30, 'YN-AERUSZ2-DEN-40', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 93
(30, 1, 20, 3000000, 30, 'YN-AERUSZ2-DEN-41', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 94
(30, 1, 21, 3000000, 30, 'YN-AERUSZ2-DEN-42', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 1, 22, 3000000, 30, 'YN-AERUSZ2-DEN-43', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 1, 23, 3000000, 30, 'YN-AERUSZ2-DEN-44', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 1, 24, 3000000, 30, 'YN-AERUSZ2-DEN-45', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 1, 25, 3000000, 30, 'YN-AERUSZ2-DEN-46', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 1, 26, 3000000, 30, 'YN-AERUSZ2-DEN-47', 'yonex_aerus_z2_den.jpg'),   -- MaChiTiet = 88
(30, 5, 14, 3000000, 30, 'YN-AERUSZ2-BAC-35', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 15, 3000000, 30, 'YN-AERUSZ2-BAC-36', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 16, 3000000, 30, 'YN-AERUSZ2-BAC-37', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 17, 3000000, 30, 'YN-AERUSZ2-BAC-38', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 18, 3000000, 30, 'YN-AERUSZ2-BAC-39', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 19, 3000000, 30, 'YN-AERUSZ2-BAC-40', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 20, 3000000, 30, 'YN-AERUSZ2-BAC-41', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 21, 3000000, 30, 'YN-AERUSZ2-BAC-42', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 22, 3000000, 30, 'YN-AERUSZ2-BAC-43', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 23, 3000000, 30, 'YN-AERUSZ2-BAC-44', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 24, 3000000, 30, 'YN-AERUSZ2-BAC-45', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 25, 3000000, 30, 'YN-AERUSZ2-BAC-46', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 5, 26, 3000000, 30, 'YN-AERUSZ2-BAC-47', 'yonex_aerus_z2_bac.jpg'),   -- MaChiTiet = 88
(30, 8, 14, 3000000, 30, 'YN-AERUSZ2-XANHLA-35', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 15, 3000000, 30, 'YN-AERUSZ2-XANHLA-36', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 16, 3000000, 30, 'YN-AERUSZ2-XANHLA-37', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 17, 3000000, 30, 'YN-AERUSZ2-XANHLA-38', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 18, 3000000, 30, 'YN-AERUSZ2-XANHLA-39', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 19, 3000000, 30, 'YN-AERUSZ2-XANHLA-40', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 20, 3000000, 30, 'YN-AERUSZ2-XANHLA-41', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 21, 3000000, 30, 'YN-AERUSZ2-XANHLA-42', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 22, 3000000, 30, 'YN-AERUSZ2-XANHLA-43', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 23, 3000000, 30, 'YN-AERUSZ2-XANHLA-44', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 24, 3000000, 30, 'YN-AERUSZ2-XANHLA-45', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 25, 3000000, 30, 'YN-AERUSZ2-XANHLA-46', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88
(30, 8, 26, 3000000, 30, 'YN-AERUSZ2-XANHLA-47', 'yonex_aerus_z2_xanhla.jpg'),   -- MaChiTiet = 88

(31, 1, 14, 3000000, 30, 'LN-AYAS012-1-DEN-35', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 15, 3000000, 30, 'LN-AYAS012-1-DEN-36', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 16, 3000000, 30, 'LN-AYAS012-1-DEN-37', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 17, 3000000, 30, 'LN-AYAS012-1-DEN-38', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 18, 3000000, 30, 'LN-AYAS012-1-DEN-39', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 19, 3000000, 30, 'LN-AYAS012-1-DEN-40', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 20, 3000000, 30, 'LN-AYAS012-1-DEN-41', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 21, 3000000, 30, 'LN-AYAS012-1-DEN-42', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 22, 3000000, 30, 'LN-AYAS012-1-DEN-43', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 23, 3000000, 30, 'LN-AYAS012-1-DEN-44', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 24, 3000000, 30, 'LN-AYAS012-1-DEN-45', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 25, 3000000, 30, 'LN-AYAS012-1-DEN-46', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 1, 26, 3000000, 30, 'LN-AYAS012-1-DEN-47', 'lining_ayas012_den.jpg'),   -- MaChiTiet = 88
(31, 6, 14, 3000000, 30, 'LN-AYAS012-1-TRANG-35', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 15, 3000000, 30, 'LN-AYAS012-1-TRANG-36', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 16, 3000000, 30, 'LN-AYAS012-1-TRANG-37', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 17, 3000000, 30, 'LN-AYAS012-1-TRANG-38', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 18, 3000000, 30, 'LN-AYAS012-1-TRANG-39', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 19, 3000000, 30, 'LN-AYAS012-1-TRANG-40', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 20, 3000000, 30, 'LN-AYAS012-1-TRANG-41', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 21, 3000000, 30, 'LN-AYAS012-1-TRANG-42', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 22, 3000000, 30, 'LN-AYAS012-1-TRANG-43', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 23, 3000000, 30, 'LN-AYAS012-1-TRANG-44', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 24, 3000000, 30, 'LN-AYAS012-1-TRANG-45', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 25, 3000000, 30, 'LN-AYAS012-1-TRANG-46', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(31, 6, 26, 3000000, 30, 'LN-AYAS012-1-TRANG-47', 'lining_ayas012_trang.jpg'),   -- MaChiTiet = 88
(32, 6, 14, 2160000, 30, 'LN-AYZU017-1-TRANG-35', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 15, 2160000, 30, 'LN-AYZU017-1-TRANG-36', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 16, 2160000, 30, 'LN-AYZU017-1-TRANG-37', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 17, 2160000, 30, 'LN-AYZU017-1-TRANG-38', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 18, 2160000, 30, 'LN-AYZU017-1-TRANG-39', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 19, 2160000, 30, 'LN-AYZU017-1-TRANG-40', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 20, 2160000, 30, 'LN-AYZU017-1-TRANG-41', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 21, 2160000, 30, 'LN-AYZU017-1-TRANG-42', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 22, 2160000, 30, 'LN-AYZU017-1-TRANG-43', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 23, 2160000, 30, 'LN-AYZU017-1-TRANG-44', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 24, 2160000, 30, 'LN-AYZU017-1-TRANG-45', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 25, 2160000, 30, 'LN-AYZU017-1-TRANG-46', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(32, 6, 26, 2160000, 30, 'LN-AYZU017-1-TRANG-47', 'lining_ayzu017.jpg'),   -- MaChiTiet = 88
(33, 10, 14, 2450000, 30, 'LN-AYAS018-6-HONG-35', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 15, 2450000, 30, 'LN-AYAS018-6-HONG-36', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 16, 2450000, 30, 'LN-AYAS018-6-HONG-37', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 17, 2450000, 30, 'LN-AYAS018-6-HONG-38', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 18, 2450000, 30, 'LN-AYAS018-6-HONG-39', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 19, 2450000, 30, 'LN-AYAS018-6-HONG-40', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 20, 2450000, 30, 'LN-AYAS018-6-HONG-41', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 21, 2450000, 30, 'LN-AYAS018-6-HONG-42', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 22, 2450000, 30, 'LN-AYAS018-6-HONG-43', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 23, 2450000, 30, 'LN-AYAS018-6-HONG-44', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 24, 2450000, 30, 'LN-AYAS018-6-HONG-45', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 25, 2450000, 30, 'LN-AYAS018-6-HONG-46', 'lining_ayas018.jpg'),   -- MaChiTiet = 88
(33, 10, 26, 2450000, 30, 'LN-AYAS018-6-HONG-47', 'lining_ayas018.jpg'),   -- MaChiTiet = 88

(34, 1, 14, 2190000, 30, 'VT-A970ACE-DEN-35', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 15, 2190000, 30, 'VT-A970ACE-DEN-36', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 16, 2190000, 30, 'VT-A970ACE-DEN-37', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 17, 2190000, 30, 'VT-A970ACE-DEN-38', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 18, 2190000, 30, 'VT-A970ACE-DEN-39', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 19, 2190000, 30, 'VT-A970ACE-DEN-40', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 20, 2190000, 30, 'VT-A970ACE-DEN-41', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 21, 2190000, 30, 'VT-A970ACE-DEN-42', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 22, 2190000, 30, 'VT-A970ACE-DEN-43', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 23, 2190000, 30, 'VT-A970ACE-DEN-44', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 24, 2190000, 30, 'VT-A970ACE-DEN-45', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 25, 2190000, 30, 'VT-A970ACE-DEN-46', 'victor_a970.jpg'),   -- MaChiTiet = 88
(34, 1, 26, 2190000, 30, 'VT-A970ACE-DEN-47', 'victor_a970.jpg'),   -- MaChiTiet = 88
(35, 3, 14, 2140000, 30, 'VT-P9200III-DO-35', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 15, 2140000, 30, 'VT-P9200III-DO-36', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 16, 2140000, 30, 'VT-P9200III-DO-37', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 17, 2140000, 30, 'VT-P9200III-DO-38', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 18, 2140000, 30, 'VT-P9200III-DO-39', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 19, 2140000, 30, 'VT-P9200III-DO-40', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 20, 2140000, 30, 'VT-P9200III-DO-41', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 21, 2140000, 30, 'VT-P9200III-DO-42', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 22, 2140000, 30, 'VT-P9200III-DO-43', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 23, 2140000, 30, 'VT-P9200III-DO-44', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 24, 2140000, 30, 'VT-P9200III-DO-45', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 25, 2140000, 30, 'VT-P9200III-DO-46', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88
(35, 3, 26, 2140000, 30, 'VT-P9200III-DO-47', 'victor_p9200iii.jpg'),   -- MaChiTiet = 88

(36, 6, 14, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-35', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 15, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-36', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 16, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-37', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 17, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-38', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 18, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-39', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 19, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-40', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 20, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-41', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 21, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-42', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 22, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-43', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 23, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-44', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 24, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-45', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 25, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-46', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 6, 26, 2750000, 30, 'MIZUNO-WAVECLWANEO2-TRANG-47', 'mizuno_waveclaw_neo2_trang.jpg'),   -- MaChiTiet = 88
(36, 2, 14, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-35', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 15, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-36', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 16, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-37', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 17, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-38', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 18, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-39', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 19, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-40', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 20, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-41', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 21, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-42', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 22, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-43', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 23, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-44', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 24, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-45', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 25, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-46', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(36, 2, 26, 2750000, 30, 'MIZUNO-WAVECLWANEO2-XANH-47', 'mizuno_waveclaw_neo2_xanh.jpg'),   -- MaChiTiet = 88
(37, 6, 14, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-35', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 15, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-36', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 16, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-37', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 17, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-38', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 18, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-39', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 19, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-40', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 20, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-41', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 21, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-42', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 22, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-43', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 23, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-44', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 24, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-45', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 25, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-46', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88
(37, 6, 26, 3900000, 30, 'MIZUNO-WAVEFANGPRO-TRANG-47', 'mizuno_fangpro.jpg'),   -- MaChiTiet = 88

--Áo/Quần
(38,6,4,119000,15,'YN-TRM3089-TRANG-S','yonex_trm3089.jpg'),
(38,6,5,119000,17,'YN-TRM3089-TRANG-M','yonex_trm3089.jpg'),
(38,6,6,119000,12,'YN-TRM3089-TRANG-L','yonex_trm3089.jpg'),
(38,6,7,119000,20,'YN-TRM3089-TRANG-XL','yonex_trm3089.jpg'),
(38,6,8,119000,8,'YN-TRM3089-TRANG0-2XL','yonex_trm3089.jpg'),
(39,8,4,169000,18,'YN-TRM3053-XANHLA-S','yonex_trm3053.jpg'),
(39,8,5,169000,13,'YN-TRM3053-XANHLA-M','yonex_trm3053.jpg'),
(39,8,6,169000,14,'YN-TRM3053-XANHLA-L','yonex_trm3053.jpg'),
(39,8,7,169000,8,'YN-TRM3053-XANHLA-XL','yonex_trm3053.jpg'),
(39,8,8,169000,10,'YN-TRM3053-XANHLA-2XL','yonex_trm3053.jpg'),
(40,1,4,139000,7,'YN-TSM3085-DEN-S','yonex_tsm3085.jpg'),
(40,1,5,139000,7,'YN-TSM3085-DEN-M','yonex_tsm3085.jpg'),
(40,1,6,139000,7,'YN-TSM3085-DEN-L','yonex_tsm3085.jpg'),
(40,1,7,139000,7,'YN-TSM3085-DEN-XL','yonex_tsm3085.jpg'),

(41,6,4,130000,10,'LN-A421-TRANG-S','lining_a421.jpg'),
(41,6,5,130000,11,'LN-A421-TRANG-M','lining_a421.jpg'),
(41,6,6,130000,5,'LN-A421-TRANG-L','lining_a421.jpg'),
(41,6,7,130000,11,'LN-A421-TRANG-XL','lining_a421.jpg'),
(41,6,8,130000,13,'LN-A421-TRANG-2XL','lining_a421.jpg'),
(42,3,4,130000,10,'LN-3175-DO-S','lining_3715.jpg'),
(42,3,5,130000,10,'LN-3175-DO-M','lining_3715.jpg'),
(42,3,6,130000,10,'LN-3175-DO-L','lining_3715.jpg'),
(42,3,7,130000,10,'LN-3175-DO-XL','lining_3715.jpg'),
(42,3,8,130000,10,'LN-3175-DO-2XL','lining_3715.jpg'),
(43,6,4,110000,12,'LN-Q21-TRANG-S','lining_q21.jpg'),
(43,6,5,110000,12,'LN-Q21-TRANG-M','lining_q21.jpg'),
(43,6,6,110000,12,'LN-Q21-TRANG-L','lining_q21.jpg'),
(43,6,7,110000,12,'LN-Q21-TRANG-XL','lining_q21.jpg');
GO


INSERT INTO ThongSoVot (MaChiTiet, DoCanBang, TrongLuong, DoCung, ChieuDai, SucCang)
VALUES 
-- Yonex Astrox 100ZZ 
(1, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(2, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),

-- Yonex Astrox 88D Pro 
(3, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(4, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),

-- Yonex Astrox 88S Pro 
(5, N'Hơi nặng đầu', '3U (85-89g)', N'Cứng', '670mm', '21-29 lbs'),
(6, N'Hơi nặng đầu', '4U (80-84g)', N'Cứng', '670mm', '20-28 lbs'),

-- Yonex Astrox 99 Pro 
(7, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(8, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),
(9, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(10, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),

-- Yonex Astrox 77 Pro 
(11, N'Hơi nặng đầu', '3U (85-89g)', N'Trung bình', '675mm', '21-29 lbs'),
(12, N'Hơi nặng đầu', '4U (80-84g)', N'Trung bình', '675mm', '20-28 lbs'),

-- Yonex Nanoflare 1000Z 
(13, N'Nhẹ đầu', '3U (85-89g)', N'Rất cứng', '675mm', '21-29 lbs'),
(14, N'Nhẹ đầu', '4U (80-84g)', N'Rất cứng', '675mm', '20-28 lbs'),

-- Yonex Nanoflare 700 Pro 
(15, N'Nhẹ đầu', '3U (85-89g)', N'Trung bình', '675mm', '21-29 lbs'),

-- Yonex Arcsaber 11 Pro 
(16, N'Cân bằng', '3U (85-89g)', N'Cứng', '675mm', '19-27 lbs'),
(17, N'Cân bằng', '4U (80-84g)', N'Cứng', '675mm', '19-27 lbs'),

-- Lining Aeronaut 9000C 
(18, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '22-30 lbs'),
(19, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '21-29 lbs'),

-- Lining Aeronaut 9000I 
(20, N'Nhẹ đầu', '5U (75-79g)', N'Dẻo', '675mm', '20-28 lbs'),

-- Lining Tectonic 7 
(21, N'Cân bằng', '4U (80-84g)', N'Trung bình', '675mm', '20-28 lbs'),

-- Lining Axforce 90 Max 
(22, N'Rất nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '22-30 lbs'),

-- Lining Axforce 80 
(23, N'Nặng đầu', '3U (85-89g)', N'Trung bình-Cứng', '675mm', '22-30 lbs'),
(24, N'Nặng đầu', '4U (80-84g)', N'Trung bình-Cứng', '675mm', '21-29 lbs'),
(25, N'Nặng đầu', '5U (75-79g)', N'Trung bình-Cứng', '675mm', '20-28 lbs'),

-- Lining Calibar 600 
(26, N'Hơi nặng đầu', '4U (80-84g)', N'Trung bình', '675mm', '20-28 lbs'),

-- Lining Bladex 800 
(27, N'Cân bằng', '3U (85-89g)', N'Cứng', '675mm', '22-30 lbs'),
(28, N'Cân bằng', '4U (80-84g)', N'Cứng', '675mm', '21-29 lbs'),
(29, N'Cân bằng', '3U (85-89g)', N'Cứng', '675mm', '22-30 lbs'),
(30, N'Cân bằng', '4U (80-84g)', N'Cứng', '675mm', '21-29 lbs'),

-- Lining Tectonic 9 
(31, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '22-30 lbs'),
(32, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '21-29 lbs'),
(33, N'Nặng đầu', '5U (75-79g)', N'Cứng', '675mm', '20-28 lbs'),

-- Victor Auraspeed 100X Ultra 
(34, N'Cân bằng', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(35, N'Cân bằng', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),

-- Victor TK Ryuga Metallic 
(36, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(37, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),

-- Victor Brave Sword 12 Pro 
(38, N'Cân bằng', '3U (85-89g)', N'Trung bình-Cứng', '675mm', '21-29 lbs'),
(39, N'Cân bằng', '4U (80-84g)', N'Trung bình-Cứng', '675mm', '20-28 lbs'),

-- Victor Jetspeed S 10 
(40, N'Cân bằng', '3U (85-89g)', N'Cứng', '675mm', '21-29 lbs'),
(41, N'Cân bằng', '4U (80-84g)', N'Cứng', '675mm', '20-28 lbs'),

-- Victor DriveX 12 
(42, N'Cân bằng', '3U (85-89g)', N'Trung bình-Cứng', '675mm', '21-29 lbs'),
(43, N'Cân bằng', '4U (80-84g)', N'Trung bình-Cứng', '675mm', '20-28 lbs'),

-- Victor Thruster F Claw 
(44, N'Nặng đầu', '4U (80-84g)', N'Trung bình', '675mm', '20-28 lbs'),

-- Mizuno Fortius 10 Power 
(45, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '23-27 lbs'),

-- Mizuno Fortius 10 Quick 
(46, N'Hơi nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '23-27 lbs'),

-- Mizuno JPX Reserve Edition 
(47, N'Nặng đầu', '3U (85-89g)', N'Cứng', '675mm', '22-30 lbs'),
(48, N'Nặng đầu', '4U (80-84g)', N'Cứng', '675mm', '21-29 lbs'),

-- Mizuno Fortius 70 
(49, N'Nặng đầu', '4U (80-84g)', N'Trung bình', '675mm', '20-28 lbs'),

-- Mizuno Altius 01 Speed Limited 
(50, N'Cân bằng', '4U (80-84g)', N'Trung bình', '675mm', '20-28 lbs');
GO

-- =====================================
-- Khải
-- =====================================
-- 1. Procedure: ThemDonHang
CREATE PROCEDURE ThemDonHang
@MaKhachHang INT,
    @DiaChi NVARCHAR(255),
    @SoDienThoai VARCHAR(15),
    @GhiChu NVARCHAR(500),
    @MaDonHang_OUT INT OUTPUT -- Thêm tham số OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Thêm TrangThai và cập nhật các trường mặc định khác
    INSERT INTO DonHang (MaKhachHang, DiaChiGiaoHang, SoDienThoaiNhanHang, GhiChu, TongTien, TongTienSauGiam, NgayDat, TrangThai)
    VALUES (@MaKhachHang, @DiaChi, @SoDienThoai, @GhiChu, 0, 0, GETDATE(), N'Chờ xác nhận');

    SET @MaDonHang_OUT = SCOPE_IDENTITY(); -- Gán MaDonHang vừa tạo vào biến OUTPUT
END;
GO

-- 2. Function: TinhTongTienDonHang
CREATE FUNCTION TinhTongTienDonHang (@MaDonHang INT)
RETURNS DECIMAL(15,2)
AS
BEGIN
    DECLARE @TongTien DECIMAL(15,2);

    SELECT @TongTien = SUM(SoLuong * DonGia)
    FROM ChiTietDonHang
    WHERE MaDonHang = @MaDonHang;

    RETURN ISNULL(@TongTien, 0);
END;
GO

-- 3. Trigger: CapNhatTongTien_AfterInsert
CREATE TRIGGER CapNhatTongTien_AfterInsert
ON ChiTietDonHang
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @MaDonHang INT;

        SELECT @MaDonHang = MaDonHang FROM INSERTED;

        BEGIN TRANSACTION;

        UPDATE DonHang
        SET TongTien = dbo.TinhTongTienDonHang(@MaDonHang),
            TongTienSauGiam = dbo.TinhTongTienDonHang(@MaDonHang) - ISNULL(TienGiam,0)
        WHERE MaDonHang = @MaDonHang;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT N'Lỗi khi cập nhật tổng tiền: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- 4. Cursor: KiemTraDonHangChuaThanhToan
CREATE PROCEDURE KiemTraDonHangChuaThanhToan
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @MaDonHang INT, @TenKhach NVARCHAR(100), @TongTien DECIMAL(15,2);

        DECLARE DonHangCursor CURSOR FOR
            SELECT d.MaDonHang, k.HoTen, d.TongTien
            FROM DonHang d
            JOIN KhachHang k ON d.MaKhachHang = k.MaKhachHang
            WHERE d.TrangThai = N'Đang xử lý';

        OPEN DonHangCursor;
        FETCH NEXT FROM DonHangCursor INTO @MaDonHang, @TenKhach, @TongTien;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            PRINT N'Đơn hàng #' + CAST(@MaDonHang AS NVARCHAR(10))
                + N' - Khách: ' + @TenKhach
                + N' - Tổng: ' + CAST(@TongTien AS NVARCHAR(20));
            FETCH NEXT FROM DonHangCursor INTO @MaDonHang, @TenKhach, @TongTien;
        END;

        CLOSE DonHangCursor;
        DEALLOCATE DonHangCursor;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT N'Lỗi khi duyệt đơn hàng chưa thanh toán: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

---- Ví dụ sử dụng các thành phần đã tạo:
---- 1. Procedure
--DECLARE @MaDon INT;

--EXEC @MaDon = ThemDonHang   
--    @MaKhachHang = 1,
--    @DiaChi = N'22 Lê Lợi, Hà Nội',
--    @SoDienThoai = '0988123456',
--    @GhiChu = N'Giao sau giờ hành chính';

--PRINT 'Mã đơn hàng mới tạo: ' + CAST(@MaDon AS NVARCHAR(10));

---- 2. Function
--SELECT dbo.TinhTongTienDonHang(1) AS TongTien;

---- 3. Trigger
--INSERT INTO ChiTietDonHang (MaDonHang, MaChiTietSanPham, SoLuong, DonGia, ThanhTien) VALUES
--(1, 101, 2, 150000, 2 * 150000);

---- 4. Cursor
--EXEC KiemTraDonHangChuaThanhToan;
--GO

-- =====================================
-- Trọng
-- =====================================
-- 1. Procedure: ThemSanPhamKhuyenMai – thêm sản phẩm có áp dụng khuyến mãi
CREATE PROCEDURE ThemSanPhamKhuyenMai
    @TenSanPham NVARCHAR(100),
    @MoTa NVARCHAR(MAX),
    @GiaGoc DECIMAL(15,2),
    @HinhAnhDaiDien VARCHAR(255),
    @MaLoai INT,
    @MaNhaCungCap INT,
    @MaHang INT,
    @MaKhuyenMai INT = NULL, -- cho phép NULL
    @CoSize BIT = 0,
    @CoMau BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @MaKhuyenMai IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM KhuyenMai WHERE MaKhuyenMai = @MaKhuyenMai)
        BEGIN
            RAISERROR(N'Khuyến mãi không tồn tại!', 16, 1);
            RETURN;
        END
    END

    INSERT INTO SanPham (TenSanPham, MoTa, GiaGoc, HinhAnhDaiDien, MaLoai, MaNhaCungCap, MaHang, MaKhuyenMai, CoSize, CoMau)
    VALUES (@TenSanPham, @MoTa, @GiaGoc, @HinhAnhDaiDien, @MaLoai, @MaNhaCungCap, @MaHang, @MaKhuyenMai, @CoSize, @CoMau);

    PRINT N'Thêm sản phẩm thành công!';
END
GO

-- 2. Function: GiaSauKhuyenMai – tính giá sau giảm %
CREATE FUNCTION GiaSauKhuyenMai(@MaSanPham INT)
RETURNS DECIMAL(15,2)
AS
BEGIN
    DECLARE @GiaSauGiam DECIMAL(15,2);

    SELECT TOP(1) @GiaSauGiam = 
        CASE 
            WHEN sp.MaKhuyenMai IS NOT NULL AND km.PhanTramGiam > 0 
            THEN sp.GiaGoc * (1 - km.PhanTramGiam/100.0)
            ELSE sp.GiaGoc
        END
    FROM SanPham sp
    LEFT JOIN KhuyenMai km ON sp.MaKhuyenMai = km.MaKhuyenMai
    WHERE sp.MaSanPham = @MaSanPham;

    RETURN @GiaSauGiam; -- có thể là NULL nếu MaSanPham không tồn tại
END
GO

-- 3. Trigger: trg_SanPham_ResetKhuyenMaiHetHan
CREATE TRIGGER trg_SanPham_ResetKhuyenMaiHetHan
ON SanPham
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy danh sách MaSanPham vừa bị INSERT/UPDATE
    -- mà lại bị gán cho một MaKhuyenMai đã hết hạn
    DECLARE @AffectedProducts TABLE (MaSanPham INT);
    
    INSERT INTO @AffectedProducts(MaSanPham)
    SELECT i.MaSanPham
    FROM inserted i
    JOIN KhuyenMai km ON i.MaKhuyenMai = km.MaKhuyenMai
    WHERE km.NgayKetThuc < GETDATE(); -- Điều kiện: Ngày kết thúc đã trôi qua

    -- Nếu có sản phẩm vi phạm, cập nhật lại (reset) MaKhuyenMai về NULL
    IF EXISTS (SELECT 1 FROM @AffectedProducts)
    BEGIN
        UPDATE SanPham
        SET MaKhuyenMai = NULL
        WHERE MaSanPham IN (SELECT MaSanPham FROM @AffectedProducts);
        
        PRINT N'Cảnh báo: Một số sản phẩm đã được gán khuyến mãi đã hết hạn. Đã tự động gỡ khuyến mãi.';
    END
END
GO

-- 4. Cursor: Duyệt qua toàn bộ sản phẩm hết hàng để cảnh báo
DECLARE @MaSanPham INT, @TenSanPham NVARCHAR(100), @TongTon INT

DECLARE SanPhamHetHang CURSOR FOR
SELECT sp.MaSanPham, sp.TenSanPham, ISNULL(SUM(ct.SoLuongTon), 0) AS TongTon
FROM SanPham sp
LEFT JOIN ChiTietSanPham ct ON sp.MaSanPham = ct.MaSanPham
GROUP BY sp.MaSanPham, sp.TenSanPham
HAVING ISNULL(SUM(ct.SoLuongTon), 0) = 0

OPEN SanPhamHetHang
FETCH NEXT FROM SanPhamHetHang INTO @MaSanPham, @TenSanPham, @TongTon

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT N'Sản phẩm "' + @TenSanPham + N'" (Mã: ' + CAST(@MaSanPham AS NVARCHAR(10)) + N') đã hết hàng!'
    FETCH NEXT FROM SanPhamHetHang INTO @MaSanPham, @TenSanPham, @TongTon
END

CLOSE SanPhamHetHang
DEALLOCATE SanPhamHetHang
GO

-- 5. Transaction: Cập nhật đồng thời giá và tồn kho
-- Tạo bảng LichSuThayDoiGia 
CREATE TABLE LichSuThayDoiGia (
    MaLichSu INT PRIMARY KEY IDENTITY(1,1),
    MaChiTiet INT NOT NULL,
    GiaCu DECIMAL(15,2) NOT NULL,
    GiaMoi DECIMAL(15,2) NOT NULL,
    SoLuongCu INT NOT NULL,
    SoLuongMoi INT NOT NULL,
    ThoiGianThayDoi DATETIME DEFAULT GETDATE(),
    NguoiThayDoi NVARCHAR(100) DEFAULT SYSTEM_USER,
    FOREIGN KEY (MaChiTiet) REFERENCES ChiTietSanPham(MaChiTiet)
);
GO

-- Tạo bảng LogLoiGiaoTac
CREATE TABLE LogLoiGiaoTac (
    MaLog INT PRIMARY KEY IDENTITY(1,1),
    MaChiTiet INT,
    Loi NVARCHAR(1000),
    ThoiGian DATETIME DEFAULT GETDATE(),
    NguoiThucHien NVARCHAR(100) DEFAULT SYSTEM_USER
);
GO

CREATE PROCEDURE CapNhatGiaVaTonKho
    @MaChiTiet INT,
    @GiaBanMoi DECIMAL(15,2),
    @SoLuongTonMoi INT,
    @MucCoLap NVARCHAR(20) = 'READ_COMMITTED'
AS
BEGIN
    DECLARE @RetryCount INT = 0
    DECLARE @MaxRetries INT = 3
    DECLARE @DeadlockPriority NVARCHAR(20) = 'LOW' -- hoặc 'HIGH'
    
    WHILE @RetryCount < @MaxRetries
    BEGIN
        BEGIN TRY
            -- THIẾT LẬP ISOLATION LEVEL (theo giáo trình trang 23-39)
            IF @MucCoLap = 'READ_UNCOMMITTED'
                SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
            ELSE IF @MucCoLap = 'READ_COMMITTED'
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED
            ELSE IF @MucCoLap = 'REPEATABLE_READ'
                SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
            ELSE IF @MucCoLap = 'SERIALIZABLE'
                SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
            
            -- THIẾT LẬP DEADLOCK PRIORITY (theo giáo trình trang 43)
            IF @DeadlockPriority = 'HIGH'
                SET DEADLOCK_PRIORITY HIGH
            ELSE
                SET DEADLOCK_PRIORITY LOW
            
            BEGIN TRANSACTION
            
            -- CHIẾN LƯỢC CHỐNG DEADLOCK: 
            -- 1. Truy cập bảng theo thứ tự nhất quán
            -- 2. Sử dụng khóa phù hợp
            -- 3. Giữ transaction ngắn nhất có thể
            
            -- BƯỚC 1: Lấy thông tin hiện tại với khóa phù hợp
            DECLARE @GiaCu DECIMAL(15,2), @SoLuongCu INT
            
            SELECT @GiaCu = GiaBan, @SoLuongCu = SoLuongTon
            FROM ChiTietSanPham WITH (UPDLOCK, ROWLOCK) -- Khóa ở mức dòng
            WHERE MaChiTiet = @MaChiTiet
            
            -- Kiểm tra tính hợp lệ
            IF @GiaBanMoi < 0 OR @SoLuongTonMoi < 0
            BEGIN
                RAISERROR(N'Giá và số lượng không được âm', 16, 1)
            END
            
            -- BƯỚC 2: Cập nhật giá - giữ transaction ngắn
            UPDATE ChiTietSanPham 
            SET GiaBan = @GiaBanMoi
            WHERE MaChiTiet = @MaChiTiet
            
            -- BƯỚC 3: Cập nhật tồn kho
            UPDATE ChiTietSanPham 
            SET SoLuongTon = @SoLuongTonMoi
            WHERE MaChiTiet = @MaChiTiet
            
            -- Ghi log (thao tác nhanh)
            INSERT INTO LichSuThayDoiGia (MaChiTiet, GiaCu, GiaMoi, SoLuongCu, SoLuongMoi)
            VALUES (@MaChiTiet, @GiaCu, @GiaBanMoi, @SoLuongCu, @SoLuongTonMoi)
            
            COMMIT TRANSACTION
            
            PRINT N'Thành công! Isolation: ' + @MucCoLap + N', Retry: ' + CAST(@RetryCount AS NVARCHAR)
            RETURN 0 -- Thành công
            
        END TRY
        BEGIN CATCH
            DECLARE @ErrorNumber INT = ERROR_NUMBER()
            DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
            
            -- Xử lý ROLLBACK an toàn
            IF XACT_STATE() <> 0
                ROLLBACK TRANSACTION
            
            -- XỬ LÝ DEADLOCK (Error 1205)
            IF @ErrorNumber = 1205 
            BEGIN
                SET @RetryCount = @RetryCount + 1
                
                IF @RetryCount < @MaxRetries
                BEGIN
                    PRINT N'Deadlock! Thử lại lần ' + CAST(@RetryCount AS NVARCHAR) + N' sau 1 giây...'
                    WAITFOR DELAY '00:00:01' -- Chờ trước khi retry
                    CONTINUE
                END
                ELSE
                BEGIN
                    PRINT N'Thất bại sau ' + CAST(@MaxRetries AS NVARCHAR) + N' lần thử'
                    RETURN -1
                END
            END
            ELSE
            BEGIN
                -- Các lỗi khác (không retry)
                PRINT N'Lỗi: ' + @ErrorMessage
                RETURN -1
            END
        END CATCH
    END
    
    RETURN -1
END
GO

---- Ví dụ sử dụng các thành phần đã tạo:

---- Sử dụng procedure ThemSanPhamKhuyenMai
--EXEC ThemSanPhamKhuyenMai 
--    @TenSanPham = N'Vợt Yonex Nanoflare 800',
--    @MoTa = N'Vợt tấn công nhanh',
--    @GiaGoc = 3800000,
--    @HinhAnhDaiDien = 'nanoflare800.jpg',
--    @MaLoai = 1,
--    @MaNhaCungCap = 1,
--    @MaHang = 1,
--    @MaKhuyenMai = 1,
--    @CoSize = 0,
--    @CoMau = 1

---- Sử dụng function GiaSauKhuyenMai
--SELECT dbo.GiaSauKhuyenMai(1) AS GiaSauKhuyenMai



----Ktra CapNhatGiaVaTonKho
---- Kiểm tra dữ liệu trước khi test
--SELECT MaChiTiet, GiaBan, SoLuongTon 
--FROM ChiTietSanPham 
--WHERE MaChiTiet = 1

---- Test cập nhật thành công
--EXEC CapNhatGiaVaTonKho 
--    @MaChiTiet = 1,
--    @GiaBanMoi = 4300000,
--    @SoLuongTonMoi = 30,
--    @MucCoLap = 'READ_COMMITTED'

---- Kiểm tra kết quả
--SELECT * FROM ChiTietSanPham WHERE MaChiTiet = 1
--SELECT * FROM LichSuThayDoiGia WHERE MaChiTiet = 1

---- Test giá âm
--EXEC CapNhatGiaVaTonKho 
--    @MaChiTiet = 1,
--    @GiaBanMoi = -1000,  -- Giá âm
--    @SoLuongTonMoi = 30

---- Test số lượng âm
--EXEC CapNhatGiaVaTonKho 
--    @MaChiTiet = 1,
--    @GiaBanMoi = 4300000,
--    @SoLuongTonMoi = -5   -- Số lượng âm

---- Kiểm tra log lỗi
--SELECT * FROM LogLoiGiaoTac

---- Test với các mức isolation khác nhau
--EXEC CapNhatGiaVaTonKho 
--    @MaChiTiet = 2,
--    @GiaBanMoi = 1800000,
--    @SoLuongTonMoi = 20,
--    @MucCoLap = 'REPEATABLE_READ'

--EXEC CapNhatGiaVaTonKho 
--    @MaChiTiet = 3,
--    @GiaBanMoi = 530000,
--    @SoLuongTonMoi = 40,
--    @MucCoLap = 'SERIALIZABLE'
--GO

-- =====================================
-- Đăng
-- =====================================
-- 1. Procedure: ThemTaiKhoanMoi – tạo tài khoản + phân quyền mặc định.
CREATE PROCEDURE ThemTaiKhoanMoi
    @TenDangNhap VARCHAR(50),
    @MatKhau VARCHAR(255),
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaQuyenMacDinh INT;

    -- Lấy mã quyền mặc định là "Khách hàng"
    SELECT @MaQuyenMacDinh = MaQuyen FROM PhanQuyen WHERE TenQuyen = N'Khách hàng';

    -- Nếu chưa có quyền "Khách hàng" thì tự tạo
    IF @MaQuyenMacDinh IS NULL
    BEGIN
        INSERT INTO PhanQuyen (TenQuyen) VALUES (N'Khách hàng');
        SET @MaQuyenMacDinh = SCOPE_IDENTITY();
    END

    -- Thêm tài khoản mới
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Email, MaQuyen)
    VALUES (@TenDangNhap, @MatKhau, @Email, @MaQuyenMacDinh);

    PRINT N'Tạo tài khoản mới thành công!';
END
GO
--Test--
--EXEC ThemTaiKhoanMoi
--    @TenDangNhap = 'khachhang_test_01',
--    @MatKhau = 'pass123',
--    @Email = 'test1@gmail.com';
--GO

--EXEC ThemTaiKhoanMoi
--    @TenDangNhap = 'nhanvien',
--    @MatKhau = 'pass123',
--    @Email = 'nhanvien@gmail.com';
--GO

--EXEC ThemTaiKhoanMoi
--    @TenDangNhap = 'admin2',
--    @MatKhau = 'pass123',
--    @Email = 'admin2@gmail.com';
--GO

--SELECT* FROM TaiKhoan
--GO



-- 2. Function: KiemTraDangNhap(@User, @Pass) – kiểm tra đăng nhập.
CREATE FUNCTION KiemTraDangNhap
(
    @User VARCHAR(50),
    @Pass VARCHAR(255)
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT;

    IF EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = @User AND MatKhau = @Pass)
        SET @KetQua = 1;
    ELSE
        SET @KetQua = 0;

    RETURN @KetQua;
END
GO
-- Test đăng nhập sai
--SELECT CASE WHEN dbo.KiemTraDangNhap('admin', 'abcd') = 1
--            THEN N'-> Kết quả: Đăng nhập thành công (ĐÚNG)'
--            ELSE N'-> Kết quả: Đăng nhập thất bại (SAI)'
--       END AS KetQua;
--GO

-- 3. Trigger: KiemTraEmailTrung – không cho 2 tài khoản cùng email.
CREATE TRIGGER KiemTraEmailTrung
ON TaiKhoan
FOR INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT Email
        FROM TaiKhoan
        WHERE Email IN (SELECT Email FROM inserted)
        GROUP BY Email
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR (N'Email này đã tồn tại! Không thể thêm hoặc cập nhật.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO
--PRINT N'Test 3.1: Thử INSERT tài khoản mới TRÙNG EMAIL (nguyenvana@gmail.com)...';
--BEGIN TRY
--    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Email, MaQuyen)
--    VALUES ('levand', 'pass456', 'nguyenvana@gmail.com', 3);
--    PRINT N'-> Kết quả: INSERT thành công (SAI - Trigger không chạy)';
--END TRY
--BEGIN CATCH
--    PRINT N'-> Kết quả: LỖI BỊ BẮT (ĐÚNG) - ' + ERROR_MESSAGE();
--END CATCH;
--GO

--PRINT N'Test 3.2: Thử UPDATE tài khoản TRÙNG EMAIL (đổi email "nhanvien_banhang" thành "nguyenvana@gmail.com")...';
--BEGIN TRY
--    UPDATE TaiKhoan
--    SET Email = 'nguyenvana@gmail.com' -- Email này đã tồn tại của 'Nguyễn Văn A'
--    WHERE TenDangNhap = 'nhanvien_banhang';
--    PRINT N'-> Kết quả: UPDATE thành công (SAI - Trigger không chạy)';
--END TRY
--BEGIN CATCH
--    PRINT N'-> Kết quả: LỖI BỊ BẮT (ĐÚNG) - ' + ERROR_MESSAGE();
--END CATCH;
--GO


-- 4. Cursor: Duyệt danh sách tài khoản chưa gán quyền.
DECLARE @MaTaiKhoan INT,
        @TenDangNhap VARCHAR(50),
        @Email VARCHAR(100);

DECLARE Cursor_TaiKhoan_ChuaGanQuyen CURSOR FOR
    SELECT MaTaiKhoan, TenDangNhap, Email
    FROM TaiKhoan
    WHERE MaQuyen IS NULL;   -- Chưa gán quyền

OPEN Cursor_TaiKhoan_ChuaGanQuyen;

FETCH NEXT FROM Cursor_TaiKhoan_ChuaGanQuyen
INTO @MaTaiKhoan, @TenDangNhap, @Email;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT N'Tài khoản chưa gán quyền: ' 
          + CAST(@MaTaiKhoan AS VARCHAR) 
          + ' - ' + @TenDangNhap 
          + ' - ' + ISNULL(@Email, 'Không có email');

    FETCH NEXT FROM Cursor_TaiKhoan_ChuaGanQuyen
    INTO @MaTaiKhoan, @TenDangNhap, @Email;
END;

CLOSE Cursor_TaiKhoan_ChuaGanQuyen;
DEALLOCATE Cursor_TaiKhoan_ChuaGanQuyen;
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Email, MaQuyen)
VALUES
('user_khongquyen', 'pass123', NULL, NULL);
GO

-- =====================================
-- Vĩ
-- =====================================
-- 1. procedure thêm phản hồi và có kiểm tra xem sản phẩm có tồn tại hay không
CREATE PROCEDURE THEMPHANHOI
    @NoiDung NVARCHAR(300),
    @DanhGia INT,
    @MaKH INT,
    @MaSP INT
AS    
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Kiểm tra Đánh giá hợp lệ (Giữ nguyên)
    IF @DanhGia IS NULL OR @DanhGia < 1 OR @DanhGia > 5
    BEGIN
        -- Sử dụng RAISERROR thay vì PRINT và RETURN để báo lỗi rõ ràng hơn trong C#
        RAISERROR(N'Lỗi: Đánh giá không hợp lệ (phải nằm trong khoảng từ 1 sao đến 5 sao)', 16, 1)
        RETURN
    END
    
    -- 2. Kiểm tra Khách hàng tồn tại (Giữ nguyên)
    IF NOT EXISTS(SELECT 1 FROM KhachHang WHERE MaKhachHang = @MaKH)
    BEGIN
        RAISERROR(N'Lỗi: Không tìm thấy khách hàng này.', 16, 1)
        RETURN
    END
    
    -- 3. Kiểm tra Sản phẩm tồn tại (Giữ nguyên)
    IF NOT EXISTS(SELECT 1 FROM SanPham WHERE MaSanPham = @MaSP)
    BEGIN    
        RAISERROR(N'Lỗi: Không tìm thấy sản phẩm.', 16, 1)
        RETURN
    END   
    
    -- 4. Thực hiện INSERT
    INSERT INTO PhanHoi (NoiDung, NgayPhanHoi, DanhGia, MaKhachHang, MaSanPham)
    VALUES (@NoiDung, GETDATE(), @DanhGia, @MaKH, @MaSP)
    
    -- Thông báo thành công
    SELECT N'Thêm phản hồi thành công' AS ThongBao
END
GO
--INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Email, MaQuyen, NgayTao)
--VALUES ('levand', 'levand@123', 'levand@gmail.com', 3, GETDATE());
--GO
--INSERT INTO KhachHang (HoTen, Email, SoDienThoai, DiaChi, MaTaiKhoan, NgayTao)
--VALUES (N'Lê Văn D', 'levand@gmail.com', '0987654321', N'789 Trường Chinh, Q. Tân Bình, TP.HCM', 5, GETDATE()); 


--DECLARE @MaKH_C INT = 3;
--DECLARE @MaCTSP_Vot88DPro4U INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-AX88DPRO2024-4U'); 
--DECLARE @MaCTSP_QuanYN INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-TSM3085-DEN-L'); 
--INSERT INTO DonHang (MaKhachHang, TrangThai, TongTien, TongTienSauGiam, DiaChiGiaoHang, SoDienThoaiNhanHang)
--VALUES (@MaKH_C, N'Đã Giao', 4938000.00, 4938000.00, N'789 Trường Chinh, Q. Tân Bình, TP.HCM', N'0987654321');
--DECLARE @MaDH2 INT = SCOPE_IDENTITY();
-----đơn hàng của khách hàng 3
--INSERT INTO ChiTietDonHang (MaDonHang, MaChiTietSanPham, SoLuong, DonGia, ThanhTien)
--VALUES  
--    (@MaDH2, @MaCTSP_Vot88DPro4U, 1, 4799000.00, 4799000.00), 
--    (@MaDH2, @MaCTSP_QuanYN, 1, 139000.00, 139000.00);

--	EXEC THEMPHANHOI
--    @NoiDung = N'Chất lượng vợt tuyệt vời, đánh rất đầm tay và trợ lực tốt.',
--    @DanhGia = 5,
--    @MaKH = 3, 
--    @MaSP = 2; 

--SELECT N'--- Kiểm tra PhanHoi ---' AS ThongBao;
--SELECT * FROM PhanHoi WHERE MaKhachHang = 3;
-----đơn hàng của khách hàng 2
--DECLARE @MaKH_C INT = 2;
--DECLARE @MaCTSP_Vot88DPro4U INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-AX88DPRO2024-4U'); 
--DECLARE @MaCTSP_Nanoflare INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-NNF1000Z-VANG-3U'); 
--INSERT INTO DonHang (MaKhachHang, TrangThai, TongTien, TongTienSauGiam, DiaChiGiaoHang, SoDienThoaiNhanHang)
--VALUES (@MaKH_C, N'Hoàn Thành', 9399000.00, 9399000.00, N'456 Lê Lợi, Q.3, TP.HCM', N'0909876543');
--DECLARE @MaDH2 INT = SCOPE_IDENTITY();

---- Chi tiết đơn hàng 2
--INSERT INTO ChiTietDonHang (MaDonHang, MaChiTietSanPham, SoLuong, DonGia, ThanhTien)
--VALUES 
--    (@MaDH2, @MaCTSP_Vot88DPro4U, 1, 4799000.00, 4799000.00),    
--    (@MaDH2, @MaCTSP_Nanoflare, 1, 4600000.00, 4600000.00);

--	EXEC THEMPHANHOI
--    @NoiDung = N'Chất lượng vợt chưa tốt.',
--    @DanhGia = 2,
--    @MaKH = 2, 
--    @MaSP = 2; 


--DECLARE @MaKH_A INT = (SELECT MaKhachHang FROM KhachHang WHERE HoTen = N'Nguyễn Văn A');
--DECLARE @MaKH_B INT = (SELECT MaKhachHang FROM KhachHang WHERE HoTen = N'Trần Thị B');

---- Lấy MaChiTietSanPham
--DECLARE @MaCTSP_Vot100ZZ INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-AX100ZZ-4U'); 
--DECLARE @MaCTSP_QuanYN INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-TSM3085-DEN-L');
--DECLARE @MaCTSP_Giay65Z4 INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'YN-SHB65Z4-TRANG-41'); 
--DECLARE @MaCTSP_AoLN INT = (SELECT MaChiTiet FROM ChiTietSanPham WHERE SKU = 'LN-A421-TRANG-M');


---- 2. ĐƠN HÀNG 1: KHÁCH HÀNG NGUYỄN VĂN A (ĐÃ GIAO)
--INSERT INTO DonHang (MaKhachHang, TrangThai, TongTien, TongTienSauGiam, DiaChiGiaoHang, SoDienThoaiNhanHang)
--VALUES (@MaKH_A, N'Đã giao', 4500000.00 + 139000.00, 4639000.00, N'123 Nguyễn Trãi, Q.1, TP.HCM', N'0901234567');

--DECLARE @MaDH1 INT = SCOPE_IDENTITY();

---- Chi tiết đơn hàng 1
--INSERT INTO ChiTietDonHang (MaDonHang, MaChiTietSanPham, SoLuong, DonGia, ThanhTien)
--VALUES (@MaDH1, @MaCTSP_Vot100ZZ, 1, 4500000.00, 4500000.00), 
--       (@MaDH1, @MaCTSP_QuanYN, 2, 139000.00, 278000.00); 


---- 3. ĐƠN HÀNG 2: KHÁCH HÀNG TRẦN THỊ B (ĐANG XỬ LÝ)
--INSERT INTO DonHang (MaKhachHang, TrangThai, TongTien, TongTienSauGiam, DiaChiGiaoHang, SoDienThoaiNhanHang)
--VALUES (@MaKH_B, N'Đang xử lý', 2800000.00 + 130000.00, 2930000.00, N'456 Lê Lợi, Q.3, TP.HCM', N'0909876543');

--DECLARE @MaDH2 INT = SCOPE_IDENTITY();

---- Chi tiết đơn hàng 2
--INSERT INTO ChiTietDonHang (MaDonHang, MaChiTietSanPham, SoLuong, DonGia, ThanhTien)
--VALUES (@MaDH2, @MaCTSP_Giay65Z4, 1, 2800000.00, 2800000.00),
--       (@MaDH2, @MaCTSP_AoLN, 1, 130000.00, 130000.00);      
--GO
--EXEC THEMPHANHOI
--    @NoiDung = N'Chất lượng vợt tuyệt vời, đánh rất đầm tay và trợ lực tốt.',
--    @DanhGia = 5,
--    @MaKH = 1, 
--    @MaSP = 1; 

--SELECT N'--- Kiểm tra PhanHoi ---' AS ThongBao;
--SELECT * FROM PhanHoi WHERE MaKhachHang = 1;
----trường hợp sai 
--EXEC THEMPHANHOI
--    @NoiDung = N'Tôi muốn đánh giá vợt này nhưng chưa kịp mua.',
--    @DanhGia = 4,
--    @MaKH = 1,
--    @MaSP = 2;
--	--trường hợp sai vì chưa mua 
--	EXEC THEMPHANHOI
--    @NoiDung = N'Thử đánh giá giày, nhưng đơn hàng vẫn chưa nhận được.',
--    @DanhGia = 4,
--    @MaKH = 2, 
--    @MaSP = 28;
--GO

-- 2. Function tính điểm trung bình đánh giá
CREATE FUNCTION TrungBinhDanhGia (@MaSP int)
returns decimal(4,2)
as 
begin 
declare @DiemTB decimal(4,2)
select @DiemTB = AVG(CAST(DanhGia as decimal(3,2)))
from PhanHoi
where MaSanPham = @MaSP
if @DiemTB is NULL
set @DiemTB = 0.0
return @DiemTB
end
go
----đúng
--SELECT 
--    SP.MaSanPham, 
--    SP.TenSanPham, 
--    dbo.TrungBinhDanhGia(SP.MaSanPham) AS DiemTrungBinh
--FROM 
--    SanPham SP;
--	select * from PhanHoi;


--- 3. Trigger: XoaKhachHang_XoaPhanHoi – khi xóa khách hàng thì xóa luôn phản hồi.
go
CREATE TRIGGER XoaKhachHang_XoaPhanHoi
ON KhachHang
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        -- Xóa phản hồi của khách hàng
        DELETE FROM PhanHoi
        WHERE MaKhachHang IN (SELECT MaKhachHang FROM deleted);

        -- Xóa giỏ hàng của khách hàng
        DELETE FROM GioHang
        WHERE MaKhachHang IN (SELECT MaKhachHang FROM deleted);

        -- Xóa chi tiết đơn hàng trước
        DELETE FROM ChiTietDonHang
        WHERE MaDonHang IN (
            SELECT MaDonHang FROM DonHang
            WHERE MaKhachHang IN (SELECT MaKhachHang FROM deleted)
        );

        -- Xóa đơn hàng của khách hàng
        DELETE FROM DonHang
        WHERE MaKhachHang IN (SELECT MaKhachHang FROM deleted);

        -- Cuối cùng mới xóa khách hàng
        DELETE FROM KhachHang
        WHERE MaKhachHang IN (SELECT MaKhachHang FROM deleted);

        COMMIT TRAN;
        PRINT N'Đã xóa khách hàng và dữ liệu liên quan thành công.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        PRINT N'Lỗi khi xóa khách hàng: ' + ERROR_MESSAGE();
    END CATCH
END;
--SELECT * FROM PhanHoi WHERE MaKhachHang = 1;
---- xóa dữ liệu
--DELETE FROM KhachHang WHERE MaKhachHang = 1;
----đã xóa hoàn toàn khách hàng 3
go
-- 4. Cursor: Duyệt phản hồi có đánh giá <= 2 để gửi cảnh báo.
Create PROCEDURE SP_ThongBaoDanhGiaThap
AS
BEGIN
    SELECT 
        PH.MaPhanHoi,
        PH.NoiDung,
        PH.DanhGia,
        PH.MaKhachHang,
        KH.HoTen,
        SP.TenSanPham
    FROM PhanHoi PH
    JOIN KhachHang KH ON PH.MaKhachHang = KH.MaKhachHang
    JOIN SanPham SP ON PH.MaSanPham = SP.MaSanPham
    WHERE PH.DanhGia <= 2
    ORDER BY PH.MaPhanHoi DESC;
END
GO

-- =====================================
-- Lam
-- =====================================
-- 1. FUNCTION: Tổng số sản phẩm đã bán theo mã sản phẩm
CREATE FUNCTION Tong_SPDaBan(@MaSP INT)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        sp.TenSanPham,ISNULL(SUM(ctdh.SoLuong), 0) AS TongSoLuongBan
    FROM SanPham sp, ChiTietSanPham ctsp, ChiTietDonHang ctdh
    WHERE sp.MaSanPham = ctsp.MaSanPham
      AND ctdh.MaChiTietSanPham = ctsp.MaChiTiet
      AND sp.MaSanPham = @MaSP
    GROUP BY sp.TenSanPham
);
GO


-- TEST:
--SELECT * FROM dbo.Tong_SPDaBan(1);

GO

-- 2. PROCEDURE: THỐNG KÊ DOANH THU THEO THÁNG+NĂM
CREATE PROC ThongKeDoanhThuThang_Nam @Thang INT, @Nam INT
AS
BEGIN
     SELECT 
          MONTH(dh.NgayDat) AS Thang,
          YEAR(dh.NgayDat) AS Nam,
          SUM(dh.TongTienSauGiam) AS TongDoanhThu,
          COUNT(dh.MaDonHang) AS SoLuongDon
     FROM DonHang dh
     WHERE MONTH(dh.NgayDat) = @Thang AND YEAR(dh.NgayDat) = @Nam
           AND dh.TrangThai = N'Đã hoàn thành'
     GROUP BY MONTH(dh.NgayDat), YEAR(dh.NgayDat)
END
GO

--TEST--
--EXEC ThongKeDoanhThuThang_Nam @Thang = 3, @Nam = 2025

--THỐNG KÊ DOANH THU THEO QUÝ
CREATE PROC ThongKeDoanhThu_Quy --(1 quý = 3 tháng )
    @Nam INT
AS
BEGIN
    SELECT 
        DATEPART(QUARTER, NgayDat) AS Quy,
        YEAR(NgayDat) AS Nam,
        SUM(TongTienSauGiam) AS TongDoanhThu,
        COUNT(MaDonHang) AS SoLuongDon
    FROM DonHang
    WHERE YEAR(NgayDat) = @Nam
          AND TrangThai = N'Đã hoàn thành'
    GROUP BY DATEPART(QUARTER, NgayDat), YEAR(NgayDat)
    ORDER BY Quy;
END
GO

--EXEC ThongKeDoanhThu_Quy @Nam = 2025;

GO

--THỐNG KÊ DOANH THU THEO NĂM
CREATE PROCEDURE ThongKeDoanhThuNam
    @Nam INT
AS
BEGIN
    SELECT 
        YEAR(NgayDat) AS Nam,
        SUM(TongTienSauGiam) AS TongDoanhThu,
        COUNT(MaDonHang) AS SoLuongDon
    FROM DonHang
    WHERE YEAR(NgayDat) = @Nam
          AND TrangThai = N'Đã hoàn thành'
    GROUP BY YEAR(NgayDat)
END
GO

-- EXEC ThongKeDoanhThuNam @Nam = 2025;

GO

-- 3. TRIGGER: Cập nhật doanh thu CỬA HÀNG khi đơn hàng được hoàn thành
CREATE TRIGGER trg_DoanhThuCuaHang
ON DonHang
AFTER UPDATE
AS
BEGIN
    IF UPDATE(TrangThai)
    BEGIN
        DECLARE @TongDoanhThu DECIMAL(15,2)
        SET @TongDoanhThu = (SELECT SUM(TongTienSauGiam) AS TONGDOANHTHU
							FROM DonHang
							WHERE TrangThai = N'Đã hoàn thành')
        PRINT N'Tổng doanh thu cửa hàng hiện tại: ' + CAST(@TongDoanhThu AS NVARCHAR) + 'VND'
    END
END;
GO

--TEST--
--SELECT SUM(TongTienSauGiam) AS TongDoanhThu
--FROM DonHang
--WHERE TrangThai = N'Hoàn thành';


--UPDATE DonHang
--SET TrangThai = N'Hoàn thành'
--WHERE MaDonHang = 3;
GO

-- 4. CURSOR: Duyệt doanh thu cửa hàng từng tháng trong năm
CREATE PROCEDURE ThongKeDoanhThuTheoThang
    @Nam INT
AS
BEGIN
    SELECT 
        MONTH(NgayDat) AS Thang,
        SUM(TongTienSauGiam) AS TongDoanhThu
    FROM DonHang
    WHERE YEAR(NgayDat) = @Nam
          AND TrangThai = N'Đã hoàn thành'
    GROUP BY MONTH(NgayDat)
    ORDER BY Thang
END






--Các khuyến mãi còn hoạt động của sản phẩm--
--SELECT 
--    sp.TenSanPham,
--    km.TenChuongTrinh,
--    km.PhanTramGiam,
--    km.NgayBatDau,
--    km.NgayKetThuc,
--    GETDATE() as [GioHienTai_Server], -- So sánh giờ này với NgayKetThuc
--    CASE 
--        WHEN km.MaKhuyenMai IS NULL THEN N'Không có KM'
--        WHEN km.NgayBatDau > GETDATE() THEN N'Chưa bắt đầu'
--        WHEN km.NgayKetThuc < GETDATE() THEN N'Đã hết hạn' -- Nếu rơi vào đây -> Giá sẽ về gốc
--        ELSE N'Đang hoạt động'
--    END as TrangThai
--FROM SanPham sp
--LEFT JOIN KhuyenMai km ON sp.MaKhuyenMai = km.MaKhuyenMai
--WHERE km.MaKhuyenMai IS NOT NULL