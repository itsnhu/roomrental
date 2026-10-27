using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class TraPhongController : Controller
    {
        private readonly QLPhongTroContext _context;

        public TraPhongController(QLPhongTroContext context)
        {
            _context = context;
        }

        // Hàm phụ trợ kiểm tra quyền Admin
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // 1. Danh sách lịch sử trả phòng
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var dsTraPhong = _context.TraPhongs
                .Include(t => t.Phong)
                .Include(t => t.ThongTinKhach)
                .OrderByDescending(t => t.NgayTraPhong)
                .ToList();

            return View(dsTraPhong);
        }

        // 2. GET: Tạo phiếu trả phòng
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Chỉ lấy những phòng đang có người thuê
            var phongDangThue = _context.Phongs.Where(p => p.TrangThai == "Đang thuê").ToList();
            ViewBag.MaPhong = new SelectList(phongDangThue, "MaPhong", "TenPhong");

            return View();
        }

        // 2. POST: Xử lý trả phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string maPhong)
        {
            // BẢO MẬT: Thêm kiểm tra quyền
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(maPhong))
            {
                ViewBag.Error = "Vui lòng chọn phòng!";
                ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Đang thuê"), "MaPhong", "TenPhong");
                return View();
            }

            // Tìm phòng
            var phong = _context.Phongs.Find(maPhong);
            if (phong == null || phong.TrangThai != "Đang thuê")
            {
                ViewBag.Error = "Phòng không hợp lệ hoặc không có người thuê!";
                ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Đang thuê"), "MaPhong", "TenPhong");
                return View();
            }

            // Tìm khách đang thuê phòng này
            var khach = _context.ThongTinKhaches.FirstOrDefault(k => k.MaPhong == maPhong);
            if (khach == null)
            {
                ViewBag.Error = "Không tìm thấy thông tin khách thuê cho phòng này!";
                ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Đang thuê"), "MaPhong", "TenPhong");
                return View();
            }

            try
            {
                // 1. Tạo phiếu trả phòng
                int newId = _context.TraPhongs.Any() ? _context.TraPhongs.Max(x => x.Id) + 1 : 1;
                var phieuTraPhong = new TraPhong
                {
                    Id = newId,
                    MaPhong = maPhong,
                    MaKhachTro = khach.MaKhachTro,
                    NgayTraPhong = DateTime.Now
                };
                _context.TraPhongs.Add(phieuTraPhong);

                // 2. Cập nhật trạng thái phòng thành "Còn trống"
                phong.TrangThai = "Còn trống";
                _context.Phongs.Update(phong);

                // 3. Gỡ khách khỏi phòng (Xóa mã phòng trong bảng khách)
                khach.MaPhong = null;
                _context.ThongTinKhaches.Update(khach);

                _context.SaveChanges();
                TempData["Success"] = $"Đã trả phòng {maPhong} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Đang thuê"), "MaPhong", "TenPhong");
                return View();
            }
        }
    }
}