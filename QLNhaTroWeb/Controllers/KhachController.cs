using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;
using System.Text.RegularExpressions;

namespace QLNhaTroWeb.Controllers
{
    public class KhachController : Controller
    {
        private readonly QLPhongTroContext _context;

        public KhachController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var danhSachKhach = _context.ThongTinKhaches.Include(k => k.Phong).ToList();
            return View(danhSachKhach);
        }

        // GET: CREATE
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            // Chỉ load các phòng còn trống
            ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Còn trống"), "MaPhong", "TenPhong");
            return View();
        }

        // POST: CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ThongTinKhach khach)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Bỏ qua validate MaKhachTro vì hệ thống tự sinh
            ModelState.Remove("MaKhachTro");

            // Tự động sinh Mã Khách (KH001, KH002...)
            var lastKhach = _context.ThongTinKhaches.OrderByDescending(k => k.MaKhachTro).FirstOrDefault();
            int nextId = 1;
            if (lastKhach != null && lastKhach.MaKhachTro.StartsWith("KH") && int.TryParse(lastKhach.MaKhachTro.Substring(2), out int lastId))
            {
                nextId = lastId + 1;
            }
            khach.MaKhachTro = "KH" + nextId.ToString("D3");

            // Chuẩn hóa IN HOA
            if (!string.IsNullOrEmpty(khach.Ho)) khach.Ho = Regex.Replace(khach.Ho.Trim(), @"\s+", " ").ToUpper();
            if (!string.IsNullOrEmpty(khach.Ten)) khach.Ten = Regex.Replace(khach.Ten.Trim(), @"\s+", " ").ToUpper();

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu có chọn phòng, cập nhật trạng thái phòng thành Đang thuê
                    if (!string.IsNullOrEmpty(khach.MaPhong))
                    {
                        var phong = _context.Phongs.Find(khach.MaPhong);
                        if (phong != null && phong.TrangThai == "Còn trống")
                        {
                            phong.TrangThai = "Đang thuê";
                            _context.Phongs.Update(phong);
                        }
                    }

                    _context.ThongTinKhaches.Add(khach);
                    _context.SaveChanges();
                    TempData["Success"] = $"Thêm khách thuê thành công! Mã khách: {khach.MaKhachTro}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi CSDL: " + ex.Message);
                }
            }
            ViewBag.MaPhong = new SelectList(_context.Phongs.Where(p => p.TrangThai == "Còn trống"), "MaPhong", "TenPhong", khach.MaPhong);
            return View(khach);
        }

        // GET: EDIT
        public IActionResult Edit(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var khach = _context.ThongTinKhaches.Find(id);
            if (khach == null) return NotFound();

            // Load phòng hiện tại của khách + các phòng đang trống
            var availableRooms = _context.Phongs.Where(p => p.TrangThai == "Còn trống" || p.MaPhong == khach.MaPhong).ToList();
            ViewBag.MaPhong = new SelectList(availableRooms, "MaPhong", "TenPhong", khach.MaPhong);

            return View(khach);
        }

        // POST: EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, ThongTinKhach khach)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != khach.MaKhachTro) return NotFound();

            // Chuẩn hóa IN HOA
            if (!string.IsNullOrEmpty(khach.Ho)) khach.Ho = Regex.Replace(khach.Ho.Trim(), @"\s+", " ").ToUpper();
            if (!string.IsNullOrEmpty(khach.Ten)) khach.Ten = Regex.Replace(khach.Ten.Trim(), @"\s+", " ").ToUpper();

            if (ModelState.IsValid)
            {
                try
                {
                    var oldKhach = _context.ThongTinKhaches.AsNoTracking().FirstOrDefault(k => k.MaKhachTro == id);

                    // Nếu Admin thay đổi phòng ở
                    if (oldKhach != null && oldKhach.MaPhong != khach.MaPhong)
                    {
                        // 1. Trả phòng cũ về trạng thái "Còn trống" (nếu có)
                        if (!string.IsNullOrEmpty(oldKhach.MaPhong))
                        {
                            var oldPhong = _context.Phongs.Find(oldKhach.MaPhong);
                            if (oldPhong != null) oldPhong.TrangThai = "Còn trống";
                        }

                        // 2. Chuyển phòng mới sang "Đang thuê" (nếu có chọn)
                        if (!string.IsNullOrEmpty(khach.MaPhong))
                        {
                            var newPhong = _context.Phongs.Find(khach.MaPhong);
                            if (newPhong != null) newPhong.TrangThai = "Đang thuê";
                        }
                    }

                    _context.ThongTinKhaches.Update(khach);
                    _context.SaveChanges();
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi CSDL: " + ex.Message);
                }
            }

            var availableRooms = _context.Phongs.Where(p => p.TrangThai == "Còn trống" || p.MaPhong == khach.MaPhong).ToList();
            ViewBag.MaPhong = new SelectList(availableRooms, "MaPhong", "TenPhong", khach.MaPhong);
            return View(khach);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var khach = _context.ThongTinKhaches.Find(id);
            if (khach != null)
            {
                try
                {
                    // Trả lại trạng thái phòng cũ thành trống trước khi xóa khách
                    if (!string.IsNullOrEmpty(khach.MaPhong))
                    {
                        var phong = _context.Phongs.Find(khach.MaPhong);
                        if (phong != null) phong.TrangThai = "Còn trống";
                    }

                    _context.ThongTinKhaches.Remove(khach);
                    _context.SaveChanges();
                    TempData["Success"] = "Đã xóa khách thuê!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Không thể xóa vì khách đang có dữ liệu liên quan (Hóa đơn, Hợp đồng)!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}