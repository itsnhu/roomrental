using Microsoft.EntityFrameworkCore;
using QLNhaTroWeb.Data; // Thay đổi namespace này nếu tên project của bạn khác

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối Database (SQL Server)
builder.Services.AddDbContext<QLPhongTroContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Thêm dịch vụ Session (Để lưu trạng thái đăng nhập)
builder.Services.AddSession();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
// BẬT CHẾ ĐỘ BÁO LỖI CHI TIẾT (Chỉ dùng khi đang sửa lỗi)
app.UseDeveloperExceptionPage();

if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Home/Error"); // Tạm ẩn dòng này đi
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Kích hoạt Session (Phải đặt trước UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();