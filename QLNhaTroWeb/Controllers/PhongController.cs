using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class PhongController : Controller
    {
        private readonly QLPhongTroContext _context;

        public PhongController(QLPhongTroContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // 1. Danh sách phòng
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var dsPhong = _context.Phongs
                .Include(p => p.KhuVuc)
                .Include(p => p.LoaiPhong)
                .ToList();

            return View(dsPhong);
        }

        // 2. GET: Thêm phòng mới
        // ... (giữ nguyên các using và phần đầu class)

        // 2. GET: Thêm phòng mới (Không thay đổi)
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            LoadDropdowns();
            return View();
        }

        // 2. POST: Xử lý thêm phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Phong phong)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Bỏ qua kiểm tra lỗi của MaPhong do người dùng không nhập
            ModelState.Remove("MaPhong");

            // Tự động xử lý chuỗi: Xóa khoảng trắng thừa và Viết Hoa
            if (!string.IsNullOrEmpty(phong.TenPhong))
                phong.TenPhong = System.Text.RegularExpressions.Regex.Replace(phong.TenPhong.Trim(), @"\s+", " ").ToUpper();

            if (!string.IsNullOrEmpty(phong.Day))
                phong.Day = System.Text.RegularExpressions.Regex.Replace(phong.Day.Trim(), @"\s+", " ").ToUpper();

            // Kiểm tra Khu vực và Loại phòng có thực sự tồn tại không
            if (!_context.KhuVucs.Any(k => k.MaKhuVuc == phong.MaKhuVuc))
                ModelState.AddModelError("MaKhuVuc", "Khu vực không hợp lệ.");

            if (!_context.LoaiPhongs.Any(l => l.MaLoaiPhong == phong.MaLoaiPhong))
                ModelState.AddModelError("MaLoaiPhong", "Loại phòng không hợp lệ.");

            if (ModelState.IsValid)
            {
                try
                {
                    // TỰ ĐỘNG TẠO MÃ PHÒNG (Tối đa 10 ký tự, không ký tự đặc biệt)
                    // Cách làm: Tìm mã phòng lớn nhất, +1, định dạng Pxxxx
                    var lastPhong = _context.Phongs.OrderByDescending(p => p.MaPhong).FirstOrDefault();
                    int nextId = 1;
                    if (lastPhong != null && lastPhong.MaPhong.StartsWith("P") && int.TryParse(lastPhong.MaPhong.Substring(1), out int lastId))
                    {
                        nextId = lastId + 1;
                    }
                    phong.MaPhong = "P" + nextId.ToString("D3"); // Kết quả: P001, P002... (Max 10 ký tự, chỉ chữ và số)

                    _context.Phongs.Add(phong);
                    _context.SaveChanges();
                    TempData["Success"] = $"Thêm phòng thành công! Mã phòng được tạo là: {phong.MaPhong}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu vào CSDL: " + ex.Message);
                }
            }

            LoadDropdowns(phong);
            return View(phong);
        }

        // ... (giữ nguyên các hàm khác)

        // 3. GET: Sửa phòng
        public IActionResult Edit(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var phong = _context.Phongs.Find(id);
            if (phong == null) return NotFound();

            LoadDropdowns(phong);
            return View(phong);
        }

        // 3. POST: Xử lý sửa phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Phong phong)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != phong.MaPhong) return NotFound();

            var phongCu = _context.Phongs.AsNoTracking().FirstOrDefault(p => p.MaPhong == id);
            if (phongCu == null) return NotFound();

            // Lỗi 3: Ngăn chặn đổi trạng thái sang "Còn trống" nếu phòng đang có khách
            if (phong.TrangThai == "Còn trống" && phongCu.TrangThai == "Đang thuê")
            {
                bool coKhachDangO = _context.ThongTinKhaches.Any(k => k.MaPhong == phong.MaPhong);
                if (coKhachDangO)
                {
                    ModelState.AddModelError("TrangThai", "Không thể chuyển sang 'Còn trống' vì phòng đang có khách. Vui lòng vào mục 'Trả Phòng' để xử lý trước.");
                }
            }

            // Kiểm tra Khu vực/Loại phòng
            if (!_context.KhuVucs.Any(k => k.MaKhuVuc == phong.MaKhuVuc)) ModelState.AddModelError("MaKhuVuc", "Khu vực không hợp lệ.");
            if (!_context.LoaiPhongs.Any(l => l.MaLoaiPhong == phong.MaLoaiPhong)) ModelState.AddModelError("MaLoaiPhong", "Loại phòng không hợp lệ.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Phongs.Update(phong);
                    _context.SaveChanges();
                    TempData["Success"] = "Cập nhật phòng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật CSDL: " + ex.Message);
                }
            }

            LoadDropdowns(phong);
            return View(phong);
        }

        // 4. Xóa phòng
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var phong = _context.Phongs.Find(id);
            if (phong != null)
            {
                // Lỗi 4: Kiểm tra ràng buộc khóa ngoại trước khi xóa để tránh lỗi SQL
                bool coDuLieuLienQuan = _context.ThongTinKhaches.Any(k => k.MaPhong == id) ||
                                        _context.PhieuThus.Any(p => p.MaPhong == id) ||
                                        _context.ThongTinThuePhongs.Any(h => h.MaPhong == id) ||
                                        _context.YeuCauThuePhongs.Any(y => y.MaPhong == id);

                if (coDuLieuLienQuan)
                {
                    TempData["Error"] = "Không thể xóa phòng này vì đang có dữ liệu liên quan (Khách thuê, Hóa đơn hoặc Hợp đồng)!";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _context.Phongs.Remove(phong);
                    _context.SaveChanges();
                    TempData["Success"] = "Đã xóa phòng thành công!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Đã xảy ra lỗi hệ thống khi xóa phòng.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Hàm phụ trợ load Dropdown
        private void LoadDropdowns(Phong? phong = null)
        {
            ViewBag.MaKhuVuc = new SelectList(_context.KhuVucs, "MaKhuVuc", "TenKhuVuc", phong?.MaKhuVuc);
            ViewBag.MaLoaiPhong = new SelectList(_context.LoaiPhongs, "MaLoaiPhong", "TenLoaiPhong", phong?.MaLoaiPhong);
        }
    }
}