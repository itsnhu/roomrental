using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QLNhaTroWeb.Controllers
{
    public class DangKyController : Controller
    {
        private readonly QLPhongTroContext _context;

        public DangKyController(QLPhongTroContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ThongTinDangKyPhong dangKy)
        {
            // 1. Xóa khoảng trắng thừa
            dangKy.Ho = dangKy.Ho?.Trim();
            dangKy.Ten = dangKy.Ten?.Trim();
            dangKy.CMND = dangKy.CMND?.Trim();
            dangKy.QueQuan = dangKy.QueQuan?.Trim();
            dangKy.NgheNghiep = dangKy.NgheNghiep?.Trim();

            // 2. KIỂM TRA NGÀY SINH
            if (!dangKy.NgaySinh.HasValue)
            {
                if (string.IsNullOrEmpty(Request.Form["NgaySinh"]))
                    ModelState.AddModelError("NgaySinh", "Vui lòng chọn ngày sinh.");
                else
                    ModelState.AddModelError("NgaySinh", "Ngày sinh không đúng định dạng.");
            }
            else
            {
                var today = DateTime.Today;
                var dob = dangKy.NgaySinh.Value.Date;

                if (dob > today)
                {
                    ModelState.AddModelError("NgaySinh", "Ngày sinh không được nằm trong tương lai.");
                }
                else if (dob == today)
                {
                    ModelState.AddModelError("NgaySinh", "Ngày sinh không hợp lệ.");
                }
                else
                {
                    int age = today.Year - dob.Year;
                    if (dob > today.AddYears(-age)) age--;

                    if (age < 18)
                    {
                        ModelState.AddModelError("NgaySinh", $"Bạn phải đủ 18 tuổi để đăng ký thuê phòng (Hiện tại bạn {age} tuổi).");
                    }
                    else if (age > 100)
                    {
                        ModelState.AddModelError("NgaySinh", "Ngày sinh không hợp lệ (vượt quá 100 tuổi). Vui lòng kiểm tra lại.");
                    }
                }
            }

            // 3. Kiểm tra trùng CMND
            if (!string.IsNullOrEmpty(dangKy.CMND))
            {
                bool isDuplicate = _context.ThongTinDangKyPhongs.Any(x => x.CMND == dangKy.CMND)
                                || _context.ThongTinKhaches.Any(x => x.CMND == dangKy.CMND);
                if (isDuplicate)
                {
                    ModelState.AddModelError("CMND", "Số CMND/CCCD này đã tồn tại trong hệ thống.");
                }
            }

            // 4. Kiểm tra Phòng cụ thể
            if (!string.IsNullOrEmpty(dangKy.MaPhong))
            {
                var phong = _context.Phongs.FirstOrDefault(p => p.MaPhong == dangKy.MaPhong);

                if (phong == null)
                {
                    ModelState.AddModelError("MaPhong", "Phòng không tồn tại.");
                }
                else if (phong.TrangThai != "Còn trống")
                {
                    ModelState.AddModelError("MaPhong", "Rất tiếc, phòng này vừa được người khác đăng ký hoặc đã có người thuê.");
                }
                else if (phong.MaKhuVuc != dangKy.MaKhuVuc || phong.MaLoaiPhong != dangKy.MaLoaiPhong)
                {
                    ModelState.AddModelError("MaPhong", "Phòng được chọn không khớp với Khu vực hoặc Loại phòng bạn đã chọn.");
                }
            }

            // 5. Kiểm tra Khu Vực/Loại Phòng hợp lệ
            if (!string.IsNullOrEmpty(dangKy.MaKhuVuc) && !_context.KhuVucs.Any(k => k.MaKhuVuc == dangKy.MaKhuVuc))
            {
                ModelState.AddModelError("MaKhuVuc", "Khu vực không hợp lệ.");
            }
            if (!string.IsNullOrEmpty(dangKy.MaLoaiPhong) && !_context.LoaiPhongs.Any(l => l.MaLoaiPhong == dangKy.MaLoaiPhong))
            {
                ModelState.AddModelError("MaLoaiPhong", "Loại phòng không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chuẩn hóa IN HOA
                    dangKy.Ho = Regex.Replace(dangKy.Ho, @"\s+", " ").ToUpper();
                    dangKy.Ten = Regex.Replace(dangKy.Ten, @"\s+", " ").ToUpper();
                    dangKy.QueQuan = Regex.Replace(dangKy.QueQuan, @"\s+", " ");
                    dangKy.NgheNghiep = Regex.Replace(dangKy.NgheNghiep, @"\s+", " ");

                    _context.ThongTinDangKyPhongs.Add(dangKy);
                    _context.SaveChanges();

                    TempData["Success"] = "Gửi đơn đăng ký thành công! Vui lòng chờ Admin duyệt.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu đơn. Vui lòng thử lại sau.");
                }
            }

            LoadDropdowns(dangKy);
            return View(dangKy);
        }

        private void LoadDropdowns(ThongTinDangKyPhong? model = null)
        {
            ViewBag.MaKhuVuc = new SelectList(_context.KhuVucs, "MaKhuVuc", "TenKhuVuc", model?.MaKhuVuc);
            ViewBag.MaLoaiPhong = new SelectList(_context.LoaiPhongs, "MaLoaiPhong", "TenLoaiPhong", model?.MaLoaiPhong);

            var phongTrong = _context.Phongs.Where(p => p.TrangThai == "Còn trống").ToList();
            ViewBag.MaPhong = new SelectList(phongTrong, "MaPhong", "TenPhong", model?.MaPhong);
            ViewBag.DanhSachPhong = phongTrong;
        }
    }
}