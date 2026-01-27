using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.BLL.Services;
using RealEstateMarketplace.DAL.Data;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Register services
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    await context.Database.MigrateAsync();
    
    // Create roles
    string[] roles = { "Admin", "Agent", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Create admin user
    var adminEmail = "admin@realestate.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true
        };
        
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
    
    // Seed default site settings
    if (!context.SiteSettings.Any())
    {
        context.SiteSettings.AddRange(
            new SiteSettings { Key = "site_name", Value = "Real Estate Marketplace", Description = "Website name", Category = "General" },
            new SiteSettings { Key = "site_description", Value = "Find your dream property", Description = "Website description", Category = "General" },
            new SiteSettings { Key = "contact_email", Value = "contact@realestate.com", Description = "Contact email address", Category = "General" },
            new SiteSettings { Key = "contact_phone", Value = "+1 234 567 890", Description = "Contact phone number", Category = "General" },
            new SiteSettings { Key = "contact_address", Value = "123 Main St, City, Country", Description = "Office address", Category = "General" },
            new SiteSettings { Key = "facebook_url", Value = "https://facebook.com", Description = "Facebook page URL", Category = "Social" },
            new SiteSettings { Key = "twitter_url", Value = "https://twitter.com", Description = "Twitter profile URL", Category = "Social" },
            new SiteSettings { Key = "instagram_url", Value = "https://instagram.com", Description = "Instagram profile URL", Category = "Social" },
            new SiteSettings { Key = "meta_keywords", Value = "real estate, property, homes, apartments", Description = "SEO keywords", Category = "SEO" },
            new SiteSettings { Key = "google_analytics_id", Value = "", Description = "Google Analytics tracking ID", Category = "SEO" }
        );
        await context.SaveChangesAsync();
    }
}

app.Run();
