using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using MandalaApp.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký repository dùng ADO.NET
builder.Services.AddScoped<MandalaRepository>();

// Cấu hình Authentication: sử dụng cookie cho xác thực nội bộ và cấu hình đăng nhập qua Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login/Login"; // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập nội bộ
})
.AddGoogle(options =>
{
    // Bạn cần đăng ký ứng dụng của mình tại Google API Console để có ClientId và ClientSecret
    options.ClientId = "your-google-client-id";
    options.ClientSecret = "your-google-client-secret";
    // Nếu muốn, bạn có thể chỉ định LoginPath riêng cho Google
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Đảm bảo middleware Authentication được gọi trước Authorization
app.UseAuthentication();   
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Index}/{action=Index}/{id?}");

app.Run();