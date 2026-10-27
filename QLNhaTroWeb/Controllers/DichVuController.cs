using Microsoft.AspNetCore.Mvc;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class DichVuController : Controller
    {
        private readonly QLPhongTroContext _context;

        public DichVuController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var dsDichVu = _context.DichVus.ToList();
            return View(dsDichVu);
        }

        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DichVu dichVu)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Xóa khoảng trắng thừa
            dichVu.TenDichVu = dichVu.TenDichVu?.Trim();

            // Kiểm tra trùng tên dịch vụ
            if (!string.IsNullOrEmpty(dichVu.TenDichVu) && _context.DichVus.Any(d => d.TenDichVu.ToLower() == dichVu.TenDichVu.ToLower()))
            {
                ModelState.AddModelError("TenDichVu", "Tên dịch vụ này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tự động tạo ID mới
                    dichVu.Id = _context.DichVus.Any() ? _context.DichVus.Max(x => x.Id) + 1 : 1;
                    _context.DichVus.Add(dichVu);
                    _context.SaveChanges();
                    TempData["Success"] = "Thêm dịch vụ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu CSDL: " + ex.Message);
                }
            }
            return View(dichVu);
        }

        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var dichVu = _context.DichVus.Find(id);
            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, DichVu dichVu)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != dichVu.Id) return NotFound();

            dichVu.TenDichVu = dichVu.TenDichVu?.Trim();

            if (!string.IsNullOrEmpty(dichVu.TenDichVu) && _context.DichVus.Any(d => d.TenDichVu.ToLower() == dichVu.TenDichVu.ToLower() && d.Id != id))
            {
                ModelState.AddModelError("TenDichVu", "Tên dịch vụ này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.DichVus.Update(dichVu);
                    _context.SaveChanges();
                    TempData["Success"] = "Cập nhật dịch vụ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật CSDL: " + ex.Message);
                }
            }
            return View(dichVu);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var dichVu = _context.DichVus.Find(id);
            if (dichVu != null)
            {
                try
                {
                    _context.DichVus.Remove(dichVu);
                    _context.SaveChanges();
                    TempData["Success"] = "Đã xóa dịch vụ!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Không thể xóa dịch vụ này vì đang có dữ liệu liên quan!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}