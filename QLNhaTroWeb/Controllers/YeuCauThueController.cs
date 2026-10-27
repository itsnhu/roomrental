using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class YeuCauThueController : Controller
    {
        private readonly QLPhongTroContext _context;

        public YeuCauThueController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // 1. Danh sách yêu cầu
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var dsYeuCau = _context.YeuCauThuePhongs
                .Include(y => y.KhachTro)
                .Include(y => y.Phong)
                .OrderByDescending(y => y.NgayDangKy)
                .ToList();

            return View(dsYeuCau);
        }

        // 2. Duyệt yêu cầu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Duyet(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var yeuCau = _context.YeuCauThuePhongs.Find(id);
            if (yeuCau != null && yeuCau.TrangThai == "Chờ duyệt")
            {
                var phong = _context.Phongs.Find(yeuCau.MaPhong);

                // Kiểm tra kỹ xem phòng còn trống không (tránh trường hợp 2 người cùng đặt 1 phòng)
                if (phong != null && phong.TrangThai == "Còn trống")
                {
                    try
                    {
                        // Bước 1: Đổi trạng thái phòng
                        phong.TrangThai = "Đang thuê";
                        _context.Phongs.Update(phong);

                        // Bước 2: Gán phòng cho khách
                        var khach = _context.ThongTinKhaches.Find(yeuCau.MaKhachTro);
                        if (khach != null)
                        {
                            khach.MaPhong = yeuCau.MaPhong;
                            _context.ThongTinKhaches.Update(khach);
                        }

                        // Bước 3: Cập nhật yêu cầu thành Đã duyệt
                        yeuCau.TrangThai = "Đã duyệt";
                        yeuCau.PhanHoiAdmin = "Admin đã duyệt yêu cầu. Chào mừng bạn đến với nhà trọ!";
                        _context.YeuCauThuePhongs.Update(yeuCau);

                        _context.SaveChanges();
                        TempData["Success"] = "Đã duyệt thành công! Khách đã được xếp vào phòng.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Lỗi khi duyệt: " + ex.Message;
                    }
                }
                else
                {
                    TempData["Error"] = "Không thể duyệt! Phòng này hiện đã có người thuê.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. Từ chối yêu cầu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TuChoi(int id, string lyDo)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var yeuCau = _context.YeuCauThuePhongs.Find(id);
            if (yeuCau != null && yeuCau.TrangThai == "Chờ duyệt")
            {
                yeuCau.TrangThai = "Từ chối";
                yeuCau.PhanHoiAdmin = string.IsNullOrEmpty(lyDo) ? "Admin đã từ chối yêu cầu." : lyDo;
                _context.YeuCauThuePhongs.Update(yeuCau);
                _context.SaveChanges();

                TempData["Success"] = "Đã từ chối yêu cầu thuê phòng.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}