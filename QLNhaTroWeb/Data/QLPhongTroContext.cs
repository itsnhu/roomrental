using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Models;

namespace QLNhaTroWeb.Data
{
    public class QLPhongTroContext : DbContext
    {
        public QLPhongTroContext(DbContextOptions<QLPhongTroContext> options) : base(options)
        {
        }

        // Khai báo các bảng
        public DbSet<DangNhap> DangNhaps { get; set; }
        public DbSet<GhiNho> GhiNhoes { get; set; }
        public DbSet<ThongTinKhach> ThongTinKhaches { get; set; }
        public DbSet<KhuVuc> KhuVucs { get; set; }
        public DbSet<LoaiPhong> LoaiPhongs { get; set; }
        public DbSet<Phong> Phongs { get; set; }
        public DbSet<PhieuThu> PhieuThus { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<TraPhong> TraPhongs { get; set; }
        public DbSet<ThongTinThuePhong> ThongTinThuePhongs { get; set; }
        public DbSet<ThongTinDangKyPhong> ThongTinDangKyPhongs { get; set; }
        public DbSet<YeuCauThuePhong> YeuCauThuePhongs { get; set; }


        // Sau này sẽ thêm Phongs, ThongTinKhaches... vào đây
    }
}