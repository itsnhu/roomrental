using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLNhaTroWeb.Models;
using QLNhaTroWeb.Data; // Thêm namespace này để dùng DbContext
using Microsoft.EntityFrameworkCore;

namespace QLNhaTroWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QLPhongTroContext _context; // Khai báo DbContext

        // Inject DbContext vào Constructor
        public HomeController(ILogger<HomeController> logger, QLPhongTroContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("UserID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Tính toán số liệu thống kê
            ViewBag.TotalRooms = _context.Phongs.Count();
            ViewBag.OccupiedRooms = _context.Phongs.Count(p => p.TrangThai == "Đang thuê");
            ViewBag.EmptyRooms = _context.Phongs.Count(p => p.TrangThai == "Còn trống");
            ViewBag.TotalCustomers = _context.ThongTinKhaches.Count();

            // Đếm số hóa đơn chưa thu
            ViewBag.UnpaidInvoices = _context.PhieuThus.Count(p => p.TrangThai == "Chưa thu");

            // Tính tổng doanh thu trong tháng hiện tại (chỉ tính các hóa đơn đã thu)
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            ViewBag.MonthlyRevenue = _context.PhieuThus
                .Where(p => p.TrangThai == "Đã thu" && p.NgayThu.HasValue && p.NgayThu.Value.Month == currentMonth && p.NgayThu.Value.Year == currentYear)
                .Sum(p => p.TongTien) ?? 0;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}