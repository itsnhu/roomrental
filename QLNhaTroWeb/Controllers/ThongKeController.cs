using Microsoft.AspNetCore.Mvc;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly QLPhongTroContext _context;

        public ThongKeController(QLPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? thang, int? nam)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");

            // Mặc định lấy tháng/năm hiện tại nếu không có tham số
            int selectedMonth = thang ?? DateTime.Now.Month;
            int selectedYear = nam ?? DateTime.Now.Year;

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            // Lấy tất cả hóa đơn trong tháng được chọn
            var hoaDons = _context.PhieuThus
                .Where(p => p.NgayLap.Month == selectedMonth && p.NgayLap.Year == selectedYear)
                .ToList();

            // Tính toán các chỉ số
            ViewBag.TongHoaDon = hoaDons.Count;
            ViewBag.DaThu = hoaDons.Count(p => p.TrangThai == "Đã thu");
            ViewBag.ChuaThu = hoaDons.Count(p => p.TrangThai != "Đã thu");

            // Tổng doanh thu (chỉ tính hóa đơn Đã thu)
            ViewBag.DoanhThu = hoaDons.Where(p => p.TrangThai == "Đã thu").Sum(p => p.TongTien) ?? 0;

            // Tổng nợ (Chưa thu)
            ViewBag.TongNo = hoaDons.Where(p => p.TrangThai != "Đã thu").Sum(p => p.TongTien) ?? 0;

            // Truyền danh sách hóa đơn ra View để hiển thị chi tiết
            return View(hoaDons.OrderByDescending(p => p.NgayLap).ToList());
        }
    }
}
