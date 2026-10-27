using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class HopDongController : Controller
    {
        private readonly QLPhongTroContext _context;

        public HopDongController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // 1. Danh sách hợp đồng
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var dsHopDong = _context.ThongTinThuePhongs
                .Include(h => h.Phong)
                .Include(h => h.ThongTinKhach)
                .ToList();
            return View(dsHopDong);
        }

        // 2. GET: Lập hợp đồng mới
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            LoadDropdowns();
            return View();
        }

        // 2. POST: Xử lý lưu hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ThongTinThuePhong hopDong)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Bỏ qua kiểm tra lỗi của MaHD vì hệ thống tự sinh
            ModelState.Remove("MaHD");

            // 1. Kiểm tra Ngày bắt đầu thuê
            if (hopDong.NgayThue.HasValue)
            {
                if (hopDong.NgayThue.Value.Date < DateTime.Today)
                {
                    ModelState.AddModelError("NgayThue", "Ngày bắt đầu thuê không được nằm trong quá khứ.");
                }
                // Giới hạn tối đa 3 tháng kể từ hôm nay (có thể đổi thành AddYears(1) nếu muốn)
                else if (hopDong.NgayThue.Value.Date > DateTime.Today.AddMonths(3))
                {
                    ModelState.AddModelError("NgayThue", "Ngày bắt đầu thuê không được vượt quá 3 tháng kể từ hôm nay.");
                }
            }

            // 2. Kiểm tra Tiền đặt cọc
            if (!hopDong.TienDatCoc.HasValue || hopDong.TienDatCoc < 1000000)
            {
                ModelState.AddModelError("TienDatCoc", "Tiền đặt cọc phải từ 1.000.000 VNĐ trở lên.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 3. TỰ ĐỘNG SINH MÃ HỢP ĐỒNG (HD001, HD002...)
                    var lastHD = _context.ThongTinThuePhongs.OrderByDescending(h => h.MaHD).FirstOrDefault();
                    int nextId = 1;
                    if (lastHD != null && lastHD.MaHD.StartsWith("HD") && int.TryParse(lastHD.MaHD.Substring(2), out int lastId))
                    {
                        nextId = lastId + 1;
                    }
                    hopDong.MaHD = "HD" + nextId.ToString("D3"); // Kết quả: HD001, HD002... (Độ dài 5 ký tự, an toàn)

                    // Lưu hợp đồng
                    _context.ThongTinThuePhongs.Add(hopDong);

                    // Đổi trạng thái phòng thành "Đang thuê"
                    var phong = _context.Phongs.Find(hopDong.MaPhong);
                    if (phong != null) phong.TrangThai = "Đang thuê";

                    // Gán mã phòng cho khách
                    var khach = _context.ThongTinKhaches.Find(hopDong.MaKhachTro);
                    if (khach != null) khach.MaPhong = hopDong.MaPhong;

                    _context.SaveChanges();
                    TempData["Success"] = $"Lập hợp đồng {hopDong.MaHD} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại dropdown
            LoadDropdowns(hopDong);
            return View(hopDong);
        }

        // 3. Xóa hợp đồng (Hủy thuê)
        [HttpPost]
        public IActionResult Delete(string maHD, string maKhachTro)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var hopDong = _context.ThongTinThuePhongs.FirstOrDefault(h => h.MaHD == maHD && h.MaKhachTro == maKhachTro);
            if (hopDong != null)
            {
                try
                {
                    _context.ThongTinThuePhongs.Remove(hopDong);

                    var phong = _context.Phongs.Find(hopDong.MaPhong);
                    if (phong != null) phong.TrangThai = "Còn trống";

                    var khach = _context.ThongTinKhaches.Find(hopDong.MaKhachTro);
                    if (khach != null) khach.MaPhong = null;

                    _context.SaveChanges();
                    TempData["Success"] = "Đã hủy hợp đồng!";
                }
                catch (System.Exception ex)
                {
                    TempData["Error"] = "Không thể xóa: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(ThongTinThuePhong? hopDong = null)
        {
            ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Còn trống"), "MaPhong", "TenPhong", hopDong?.MaPhong);

            var khachTrong = _context.ThongTinKhaches
                .Where(k => k.MaPhong == null)
                .Select(k => new { k.MaKhachTro, HoTen = k.Ho + " " + k.Ten })
                .ToList();
            ViewBag.MaKhachTro = new SelectList(khachTrong, "MaKhachTro", "HoTen", hopDong?.MaKhachTro);
        }
    }
}