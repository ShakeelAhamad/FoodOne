using FoodOne.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add cookie authentication for admin area
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin";
        options.AccessDeniedPath = "/admin";
        options.Cookie.Name = "FoodOneAdminAuth";
    });

//Add DbContext service
builder.Services.AddDbContext<FoodAppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed a default admin user if none exists (safe no-op if table missing)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<FoodAppDbContext>();
        var hasher = new PasswordHasher<FoodOne.Models.AdminUser>();
        if (!db.AdminUsers.Any())
        {
            var admin = new FoodOne.Models.AdminUser
            {
                Email = "admin@admin.com",
                FullName = "Administrator",
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            db.AdminUsers.Add(admin);
            db.SaveChanges();
        }
    }
    catch
    {
        // Ignore errors (for example migrations not applied yet)
    }
}

app.MapStaticAssets();

app.MapControllerRoute(
    name: "Admin",
    pattern: "admin/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
