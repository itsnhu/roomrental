using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class DuyetDangKyController : Controller
    {
        private readonly QLPhongTroContext _context;

        public DuyetDangKyController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var dsDangKy = _context.ThongTinDangKyPhongs
                .Include(d => d.KhuVuc)
                .Include(d => d.LoaiPhong)
                .Include(d => d.Phong) // Load thông tin phòng nếu khách đã chọn
                .ToList();
            return View(dsDangKy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Duyet(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var dangKy = _context.ThongTinDangKyPhongs.Find(id);
            if (dangKy != null)
            {
                try
                {
                    // 1. Tạo mã khách mới (VD: KH004)
                    string newMaKhach = "KH" + (_context.ThongTinKhaches.Count() + 1).ToString("D3");

                    // 2. Tạo Hồ sơ khách thuê
                    var khachMoi = new ThongTinKhach
                    {
                        MaKhachTro = newMaKhach,
                        Ho = dangKy.Ho,
                        Ten = dangKy.Ten,
                        GioiTinh = dangKy.GioiTinh,
                        NgaySinh = dangKy.NgaySinh,
                        CMND = dangKy.CMND,
                        QueQuan = dangKy.QueQuan,
                        NgheNghiep = dangKy.NgheNghiep,
                        GhiChu = "Từ đơn đăng ký",
                        MaPhong = dangKy.MaPhong // Gán phòng nếu có chọn
                    };
                    _context.ThongTinKhaches.Add(khachMoi);

                    // 3. Tự động tạo Tài khoản Đăng nhập cho khách
                    // Mật khẩu mặc định là 123
                    var taiKhoanMoi = new DangNhap
                    {
                        ID = newMaKhach,
                        PassWord = "123",
                        Role = "User"
                    };
                    _context.DangNhaps.Add(taiKhoanMoi);

                    // 4. Nếu khách có chọn phòng cụ thể, tự động tạo Hợp đồng và đổi trạng thái phòng
                    if (!string.IsNullOrEmpty(dangKy.MaPhong))
                    {
                        var phong = _context.Phongs.Find(dangKy.MaPhong);
                        if (phong != null && phong.TrangThai == "Còn trống")
                        {
                            // Tạo hợp đồng
                            var hopDong = new ThongTinThuePhong
                            {
                                MaHD = "HD" + DateTime.Now.ToString("ddMMyyHHmm"),
                                MaKhachTro = newMaKhach,
                                MaPhong = dangKy.MaPhong,
                                NgayThue = DateTime.Now,
                                TienDatCoc = 0,
                                GhiChu = "Tạo tự động từ đơn đăng ký"
                            };
                            _context.ThongTinThuePhongs.Add(hopDong);

                            // Đổi trạng thái phòng
                            phong.TrangThai = "Đang thuê";
                            _context.Phongs.Update(phong);
                        }
                    }

                    // 5. Xóa đơn đăng ký
                    _context.ThongTinDangKyPhongs.Remove(dangKy);
                    _context.SaveChanges();

                    TempData["Success"] = $"Đã duyệt! Mã khách: {newMaKhach}. Tài khoản đăng nhập: {newMaKhach} / Mật khẩu: 123";
                }
                catch (System.Exception ex)
                {
                    TempData["Error"] = "Lỗi khi duyệt đơn: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}