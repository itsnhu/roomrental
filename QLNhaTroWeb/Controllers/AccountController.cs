using Microsoft.AspNetCore.Mvc;
using QLNhaTroWeb.Data;
using QLNhaTroWeb.Models;
using System.Linq;

namespace QLNhaTroWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLPhongTroContext _context;

        public AccountController(QLPhongTroContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "User") return RedirectToAction("Index", "UserPortal");
            if (role == "Admin") return RedirectToAction("Index", "Home");

            if (HttpContext.Session.GetString("UserID") != null) HttpContext.Session.Clear();

            return View();
        }

        [HttpPost]
        public IActionResult Login(string id, string password, bool rememberMe)
        {
            try
            {
                id = id?.Trim();
                password = password?.Trim();

                // CỐ GẮNG TRUY VẤN (Nếu DB thiếu cột Role sẽ văng lỗi ở đây)
                var acc = _context.DangNhaps.FirstOrDefault(x => x.ID == id && x.PassWord.Trim() == password);

                if (acc == null)
                {
                    ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                    return View();
                }

                var gn = _context.GhiNhoes.FirstOrDefault();
                if (rememberMe)
                {
                    if (gn == null) _context.GhiNhoes.Add(new GhiNho { ID = id, Pass = password });
                    else { gn.ID = id; gn.Pass = password; }
                }
                else if (gn != null) _context.GhiNhoes.Remove(gn);
                _context.SaveChanges();

                HttpContext.Session.SetString("UserID", acc.ID);

                if (acc.Role == "User")
                {
                    HttpContext.Session.SetString("Role", "User");
                    return RedirectToAction("Index", "UserPortal");
                }
                else
                {
                    HttpContext.Session.SetString("Role", "Admin");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                // BẮT LỖI CHÍNH XÁC
                ViewBag.Error = "LỖI SQL: Cột 'Role' chưa tồn tại trong Database. Hãy chạy lệnh SQL thêm cột Role trong SSMS!";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "LỖI HỆ THỐNG: " + ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult ProcessQRLogin(string qrData)
        {
            if (string.IsNullOrEmpty(qrData)) return Json(new { success = false, message = "Mã QR rỗng!" });

            string cleanData = qrData.Trim();
            var acc = _context.DangNhaps.FirstOrDefault(x => x.ID == cleanData);

            if (acc == null) return Json(new { success = false, message = "Không tìm thấy tài khoản: " + cleanData });

            HttpContext.Session.SetString("UserID", acc.ID);
            HttpContext.Session.SetString("Role", acc.Role ?? "Admin");

            return Json(new { success = true, message = "Đăng nhập thành công!" });
        }
    }
}