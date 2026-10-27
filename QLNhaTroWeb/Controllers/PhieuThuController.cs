using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class PhieuThuController : Controller
    {
        private readonly QLPhongTroContext _context;

        public PhieuThuController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // 1. Danh sách hóa đơn
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Lấy danh sách hóa đơn kèm Phòng và Loại phòng
            var dsHoaDon = _context.PhieuThus
                .Include(p => p.Phong)
                .ThenInclude(p => p.LoaiPhong)
                .OrderByDescending(p => p.NgayLap)
                .ToList();

            // Lấy danh sách Khách hàng đang thuê phòng để tra cứu tên
            ViewBag.KhachHangs = _context.ThongTinKhaches
                .Where(k => k.MaPhong != null)
                .ToList();

            return View(dsHoaDon);
        }

        // 2. GET: Lập hóa đơn mới
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Lấy đơn giá Điện và Nước từ DB để tính toán
            var giaDien = _context.DichVus.FirstOrDefault(d => d.TenDichVu.Contains("Điện"))?.Gia ?? 3500;
            var giaNuoc = _context.DichVus.FirstOrDefault(d => d.TenDichVu.Contains("Nước"))?.Gia ?? 10000;

            ViewBag.DonGiaDien = giaDien;
            ViewBag.DonGiaNuoc = giaNuoc;
            ViewBag.MaPhong = new SelectList(_context.Phongs, "MaPhong", "TenPhong");

            return View();
        }

        // 2. POST: Lưu hóa đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PhieuThu phieuThu)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Bỏ qua kiểm tra Mã PT do hệ thống tự sinh
            ModelState.Remove("MaPT");

            // Lấy đơn giá hiện tại để kiểm tra chéo (Chống hack qua DevTools)
            var giaDien = _context.DichVus.FirstOrDefault(d => d.TenDichVu.Contains("Điện"))?.Gia ?? 3500;
            var giaNuoc = _context.DichVus.FirstOrDefault(d => d.TenDichVu.Contains("Nước"))?.Gia ?? 10000;

            long expectedTienDien = (long)((phieuThu.SkDien ?? 0) * giaDien);
            long expectedTienNuoc = (long)((phieuThu.SkNuoc ?? 0) * giaNuoc);

            // Kiểm tra logic tính tiền
            if (phieuThu.TienDien != expectedTienDien)
                ModelState.AddModelError("TienDien", "Tiền điện không khớp với số ký và đơn giá.");

            if (phieuThu.TienNuoc != expectedTienNuoc)
                ModelState.AddModelError("TienNuoc", "Tiền nước không khớp với số khối và đơn giá.");

            if (phieuThu.TienNha < 0) ModelState.AddModelError("TienNha", "Tiền nhà không được âm.");
            if (phieuThu.SkDien < 0) ModelState.AddModelError("SkDien", "Số ký điện không được âm.");
            if (phieuThu.SkNuoc < 0) ModelState.AddModelError("SkNuoc", "Số khối nước không được âm.");

            if (ModelState.IsValid)
            {
                try
                {
                    // TỰ ĐỘNG SINH MÃ PHIẾU THU (1001, 1002...)
                    double maxMaPT = _context.PhieuThus.Any() ? _context.PhieuThus.Max(p => p.MaPT) : 1000;
                    phieuThu.MaPT = maxMaPT + 1;

                    // Tự động tính tổng tiền
                    phieuThu.TongTien = (phieuThu.TienNha ?? 0) + (phieuThu.TienDien ?? 0) + (phieuThu.TienNuoc ?? 0);
                    phieuThu.TrangThai = "Chưa thu";
                    phieuThu.NgayLap = DateTime.Now;

                    _context.PhieuThus.Add(phieuThu);
                    _context.SaveChanges();
                    TempData["Success"] = $"Lập hóa đơn #{phieuThu.MaPT} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            ViewBag.DonGiaDien = giaDien;
            ViewBag.DonGiaNuoc = giaNuoc;
            ViewBag.MaPhong = new SelectList(_context.Phongs, "MaPhong", "TenPhong", phieuThu.MaPhong);
            return View(phieuThu);
        }

        // 3. Thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThanhToan(double id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var hoaDon = _context.PhieuThus.Find(id);
            if (hoaDon != null && hoaDon.TrangThai != "Đã thu")
            {
                hoaDon.TrangThai = "Đã thu";
                hoaDon.NgayThu = DateTime.Now;
                _context.SaveChanges();
                TempData["Success"] = $"Đã xác nhận thu tiền hóa đơn #{hoaDon.MaPT}!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. In hóa đơn (Giữ nguyên)
        public IActionResult Print(double id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var hoaDon = _context.PhieuThus.Include(p => p.Phong).FirstOrDefault(p => p.MaPT == id);
            if (hoaDon == null) return NotFound();
            return View(hoaDon);
        }
    }
}