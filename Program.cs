using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartShop.Models.Data;

namespace SmartShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🟢 1. Database Connections
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer("Server=DESKTOP-02PJEMP;Database=Smart_Shop;Trusted_Connection=True;TrustServerCertificate=True;"));

            // 🟢 2. Identity مع إعدادات مريحة للتجربة
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                
                // إعدادات كلمة مرور بسيطة للتجربة
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 3;
                
                // إعدادات الـ SignIn
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // 🟢 3. إعدادات الـ Cookies - هذا هو الجزء الأهم
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "SmartShopAuth";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30); // شهر كامل
                options.SlidingExpiration = true;
                
                // المسارات
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                
                // إعدادات إضافية مهمة
                options.ReturnUrlParameter = "returnUrl";
            });

            // 🟢 4. إضافة Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "SmartShopSession";
            });

            // 🟢 5. MVC Controllers
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            // الترتيب الصحيح
            app.UseRouting();
            app.UseSession();       // قبل Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // Redirect to Login
            app.Use(async (context, next) =>
            {
                // Redirect للـ Login فقط لو:
                // 1. الصفحه الرئيسية (/)
                // 2. المستخدم مش مسجل دخول
                if (context.Request.Path == "/" && !context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Identity/Account/Login");
                    return;
                }
                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();
            app.Run();
        }
    }
}