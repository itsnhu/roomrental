#  QLNhaTroWeb - Hệ Thống Quản Lý Nhà Trọ 

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-blueviolet)
![C%23](https://img.shields.io/badge/C%23-Language-blue)
![SQL Server](https://img.shields.io/badge/SQL_Server-Database-red)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.0-purple)

##  Giới thiệu
**QLNhaTroWeb** là một ứng dụng web toàn diện dành cho việc quản lý nhà trọ, phòng trọ. Dự án được phát triển dựa trên nền tảng **ASP.NET Core MVC (C#)** và cơ sở dữ liệu **SQL Server**. 

Điểm đặc biệt của dự án này là được **chuyển đổi hoàn toàn từ một ứng dụng Desktop (WinForms) cũ** sang kiến trúc Web hiện đại, giúp người dùng có thể truy cập mọi lúc mọi nơi thông qua trình duyệt mà không cần cài đặt phần mềm.

##  Tính năng nổi bật

### 1. Dành cho Quản trị viên (Admin)
- **Dashboard trực quan:** Thống kê tổng quan số phòng, khách thuê, doanh thu và hóa đơn chưa thu bằng biểu đồ/thẻ thông tin hiện đại.
- **Quản lý Phòng & Dịch vụ:** Thêm, sửa, xóa phòng trọ, khu vực, loại phòng và các dịch vụ đi kèm (Điện, Nước, Wifi...).
- **Quản lý Khách thuê & Hợp đồng:** Quản lý thông tin cá nhân, lập hợp đồng thuê, xử lý trả phòng tự động cập nhật trạng thái phòng.
- **Quản lý Hóa đơn thông minh:** 
  - Tự động tính tiền điện, nước dựa trên chỉ số và đơn giá.
  - Tự động tính tổng tiền và sinh mã hóa đơn.
  - Hỗ trợ in hóa đơn chuẩn.
- **Duyệt yêu cầu:** Phê duyệt đơn đăng ký thuê phòng của khách hàng.
- **Bảo mật nâng cao:** Đăng nhập bằng Mật khẩu hoặc **Quét mã QR Code** (tích hợp Camera).

### 2. Dành cho Khách thuê (User)
- **Cổng thông tin cá nhân:** Đăng nhập bằng tài khoản được cấp.
- **Tra cứu thông tin:** Xem thông tin phòng đang thuê, hợp đồng và hồ sơ cá nhân.
- **Quản lý hóa đơn:** Theo dõi lịch sử hóa đơn và trạng thái thanh toán.
- **Đổi mật khẩu:** Tự bảo mật tài khoản cá nhân.

##  Công nghệ sử dụng
- **Ngôn ngữ:** C# (.NET 8)
- **Framework:** ASP.NET Core MVC
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Frontend:** HTML5, CSS3, JavaScript, jQuery, Bootstrap 5, Bootstrap Icons
- **Xác thực:** Session, Cookie, Phân quyền Role-based (Admin/User)
- **Triển khai (Deploy):** Somee Hosting

## 🌐 Trải nghiệm
Dự án đã được triển khai
👉 **[BẤM VÀO ĐÂY ĐỂ TRẢI NGHIỆM HỆ THỐNG](http://rentalroom.somee.com/)** 
