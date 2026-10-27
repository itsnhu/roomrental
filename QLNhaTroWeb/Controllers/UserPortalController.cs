using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLNhaTroWeb.Controllers
{
    public class UserPortalController : Controller
    {
        private readonly QLPhongTroContext _context;

        public UserPortalController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsUser() => HttpContext.Session.GetString("Role") == "User";
        private string GetCurrentUserId() => HttpContext.Session.GetString("UserID") ?? "";

        // 1. TRANG CHỦ KHÁCH
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();

            var khach = await _context.ThongTinKhaches
                .Include(k => k.Phong)
                .FirstOrDefaultAsync(k => k.MaKhachTro == maKhach);

            if (khach == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var hoaDons = new List<PhieuThu>();
            if (khach.MaPhong != null)
            {
                hoaDons = await _context.PhieuThus
                    .Where(p => p.MaPhong == khach.MaPhong)
                    .OrderByDescending(p => p.NgayLap)
                    .Take(3).ToListAsync();
            }

            ViewBag.HoaDons = hoaDons;
            return View(khach);
        }

        // 2. PHÒNG CỦA TÔI
        [HttpGet]
        public async Task<IActionResult> PhongCuaToi()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();

            var khach = await _context.ThongTinKhaches
                .Include(k => k.Phong).ThenInclude(p => p.LoaiPhong)
                .Include(k => k.Phong).ThenInclude(p => p.KhuVuc)
                .FirstOrDefaultAsync(k => k.MaKhachTro == maKhach);

            if (khach == null || string.IsNullOrEmpty(khach.MaPhong))
            {
                ViewBag.NoRoom = true;
                return View();
            }

            var hopDong = await _context.ThongTinThuePhongs
                .Where(h => h.MaKhachTro == maKhach && h.MaPhong == khach.MaPhong)
                .OrderByDescending(h => h.NgayThue)
                .FirstOrDefaultAsync();

            ViewBag.HopDong = hopDong;
            return View(khach);
        }

        // 3. HÓA ĐƠN CỦA TÔI
        [HttpGet]
        public async Task<IActionResult> HoaDon(string trangThai)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();

            var dsMaPhong = await _context.ThongTinThuePhongs
                .Where(h => h.MaKhachTro == maKhach)
                .Select(h => h.MaPhong)
                .Distinct()
                .ToListAsync();

            var query = _context.PhieuThus.Include(p => p.Phong).Where(p => dsMaPhong.Contains(p.MaPhong));

            if (!string.IsNullOrEmpty(trangThai)) query = query.Where(p => p.TrangThai == trangThai);

            return View(await query.OrderByDescending(p => p.NgayLap).ToListAsync());
        }

        // 4. HỒ SƠ CÁ NHÂN (GET)
        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();

            var khach = await _context.ThongTinKhaches.FindAsync(maKhach);
            if (khach == null) return NotFound();

            return View(khach);
        }

        // 4. HỒ SƠ CÁ NHÂN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoSo(string maKhachTro, string ghiChu)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string currentUser = GetCurrentUserId();

            if (maKhachTro != currentUser) return Forbid();

            if (ghiChu != null && ghiChu.Length > 255)
            {
                TempData["Error"] = "Ghi chú/Số điện thoại không được vượt quá 255 ký tự!";
                return RedirectToAction(nameof(HoSo));
            }

            var khach = await _context.ThongTinKhaches.FindAsync(maKhachTro);
            if (khach != null)
            {
                khach.GhiChu = ghiChu?.Trim();
                _context.Update(khach);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            return RedirectToAction(nameof(HoSo));
        }

        // 5. GỬI YÊU CẦU ĐĂNG KÝ THUÊ PHÒNG (GET)
        [HttpGet]
        public async Task<IActionResult> DangKyThue()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();
            var khach = await _context.ThongTinKhaches.FindAsync(maKhach);

            if (khach != null && !string.IsNullOrEmpty(khach.MaPhong))
            {
                TempData["Error"] = "Bạn đang thuê phòng. Không thể đăng ký thêm!";
                return RedirectToAction(nameof(PhongCuaToi));
            }

            var phongTrong = await _context.Phongs.Where(p => p.TrangThai == "Còn trống").ToListAsync();
            ViewBag.MaPhong = new SelectList(phongTrong, "MaPhong", "TenPhong");
            return View();
        }

        // 5. GỬI YÊU CẦU ĐĂNG KÝ THUÊ PHÒNG (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyThue(string maPhong, DateTime? ngayDuKienThue, string ghiChu)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();

            var phong = await _context.Phongs.FindAsync(maPhong);
            if (phong == null || phong.TrangThai != "Còn trống")
            {
                TempData["Error"] = "Rất tiếc! Phòng này vừa được người khác thuê.";
                return RedirectToAction(nameof(DangKyThue));
            }

            // Kiểm tra xem đã có yêu cầu chờ duyệt nào chưa
            var hasPending = await _context.YeuCauThuePhongs.AnyAsync(y => y.MaKhachTro == maKhach && y.TrangThai == "Chờ duyệt");
            if (hasPending)
            {
                TempData["Error"] = "Bạn đã có một yêu cầu đang chờ duyệt. Vui lòng không gửi thêm!";
                return RedirectToAction(nameof(LichSuDangKy));
            }

            var yeuCau = new YeuCauThuePhong
            {
                MaKhachTro = maKhach,
                MaPhong = maPhong,
                NgayDangKy = DateTime.Now,
                NgayDuKienThue = ngayDuKienThue,
                GhiChu = ghiChu,
                TrangThai = "Chờ duyệt"
            };
            _context.YeuCauThuePhongs.Add(yeuCau);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Gửi yêu cầu thuê phòng thành công! Vui lòng chờ Admin duyệt.";
            return RedirectToAction(nameof(LichSuDangKy));
        }

        // 6. LỊCH SỬ ĐĂNG KÝ
        [HttpGet]
        public async Task<IActionResult> LichSuDangKy()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            string maKhach = GetCurrentUserId();

            var dsYeuCau = await _context.YeuCauThuePhongs
                .Include(y => y.Phong)
                .Where(y => y.MaKhachTro == maKhach)
                .OrderByDescending(y => y.NgayDangKy)
                .ToListAsync();

            return View(dsYeuCau);
        }

        // 7. ĐỔI MẬT KHẨU (GET)
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            return View();
        }

        // 7. ĐỔI MẬT KHẨU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(string oldPass, string newPass, string confirmPass)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(newPass) || newPass.Length < 3)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 3 ký tự!";
                return View();
            }
            if (newPass != confirmPass)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            string maKhach = GetCurrentUserId();
            var acc = await _context.DangNhaps.FirstOrDefaultAsync(x => x.ID == maKhach);

            if (acc == null || acc.PassWord.Trim() != oldPass.Trim())
            {
                ViewBag.Error = "Mật khẩu cũ không chính xác!";
                return View();
            }

            acc.PassWord = newPass;
            _context.DangNhaps.Update(acc);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }
    }
}