using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vivenda.BLL.Services;
using Vivenda.DAL.Data;
using Vivenda.DAL.Entities;
using Vivenda.DAL.Repositories;

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
builder.Services.AddScoped<IChatbotService, ChatbotService>();

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
    var adminEmail = "admin@vivenda.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true
        };
        
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Create demo agent
    var agentEmail = "agent@vivenda.com";
    var agent = await userManager.FindByEmailAsync(agentEmail);
    if (agent == null)
    {
        agent = new User
        {
            UserName = agentEmail,
            Email = agentEmail,
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "+1 555-123-4567",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(agent, "Agent123!");
        await userManager.AddToRoleAsync(agent, "Agent");
    }
    else
    {
        agent = await userManager.FindByEmailAsync(agentEmail);
    }

    // Seed Demo Properties
    if (!context.Properties.Any())
    {
        // Ensure categories exist
        var residentialCat = context.Categories.FirstOrDefault(c => c.Name == "Residential");
        if (residentialCat == null)
        {
            residentialCat = new Category { Name = "Residential", Description = "Homes and apartments for living", IconClass = "fas fa-home", IsActive = true };
            context.Categories.Add(residentialCat);
        }

        var commercialCat = context.Categories.FirstOrDefault(c => c.Name == "Commercial");
        if (commercialCat == null)
        {
            commercialCat = new Category { Name = "Commercial", Description = "Office spaces and retail", IconClass = "fas fa-building", IsActive = true };
            context.Categories.Add(commercialCat);
        }

        var luxuryCat = context.Categories.FirstOrDefault(c => c.Name == "Luxury");
        if (luxuryCat == null)
        {
            luxuryCat = new Category { Name = "Luxury", Description = "High-end premium properties", IconClass = "fas fa-gem", IsActive = true };
            context.Categories.Add(luxuryCat);
        }

        var vacationCat = context.Categories.FirstOrDefault(c => c.Name == "Vacation");
        if (vacationCat == null)
        {
            vacationCat = new Category { Name = "Vacation", Description = "Holiday homes and rentals", IconClass = "fas fa-umbrella-beach", IsActive = true };
            context.Categories.Add(vacationCat);
        }

        // Ensure amenities exist
        var amenityNames = new[] { "Swimming Pool", "Gym", "Parking", "Garden", "Air Conditioning", "Security", "Balcony", "Fireplace" };
        var amenityIcons = new[] { "fas fa-swimming-pool", "fas fa-dumbbell", "fas fa-parking", "fas fa-leaf", "fas fa-snowflake", "fas fa-shield-alt", "fas fa-door-open", "fas fa-fire" };
        
        for (int i = 0; i < amenityNames.Length; i++)
        {
            if (!context.Amenities.Any(a => a.Name == amenityNames[i]))
            {
                context.Amenities.Add(new Amenity { Name = amenityNames[i], IconClass = amenityIcons[i], IsActive = true });
            }
        }

        await context.SaveChangesAsync();

        var demoProperties = new List<Property>
        {
            // Houses for Sale
            new Property
            {
                Title = "Modern Family Home in Beverly Hills",
                Description = "Stunning 4-bedroom modern home with panoramic views, gourmet kitchen, and smart home features. Perfect for families seeking luxury living.",
                Price = 1250000,
                Address = "456 Sunset Boulevard",
                City = "Los Angeles",
                State = "California",
                ZipCode = "90210",
                Country = "USA",
                Bedrooms = 4,
                Bathrooms = 3,
                SquareFeet = 3200,
                YearBuilt = 2020,
                PropertyType = PropertyType.House,
                ListingType = ListingType.Sale,
                Status = PropertyStatus.Active,
                IsFeatured = true,
                MainImageUrl = "/images/properties/house1.jpg",
                UserId = agent!.Id,
                CategoryId = luxuryCat.Id
            },
            new Property
            {
                Title = "Cozy Suburban House",
                Description = "Charming 3-bedroom house in a quiet neighborhood. Large backyard, updated kitchen, and close to schools.",
                Price = 450000,
                Address = "123 Oak Street",
                City = "Miami",
                State = "Florida",
                ZipCode = "33101",
                Country = "USA",
                Bedrooms = 3,
                Bathrooms = 2,
                SquareFeet = 1800,
                YearBuilt = 2015,
                PropertyType = PropertyType.House,
                ListingType = ListingType.Sale,
                Status = PropertyStatus.Active,
                IsFeatured = true,
                MainImageUrl = "/images/properties/house2.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            new Property
            {
                Title = "Budget-Friendly Starter Home",
                Description = "Perfect first home! 2-bedroom house with garage, fenced yard, and recent renovations.",
                Price = 185000,
                Address = "789 Pine Avenue",
                City = "Austin",
                State = "Texas",
                ZipCode = "78701",
                Country = "USA",
                Bedrooms = 2,
                Bathrooms = 1,
                SquareFeet = 1100,
                YearBuilt = 2005,
                PropertyType = PropertyType.House,
                ListingType = ListingType.Sale,
                Status = PropertyStatus.Active,
                IsFeatured = false,
                MainImageUrl = "/images/properties/house3.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            // Apartments for Sale
            new Property
            {
                Title = "Downtown Luxury Penthouse",
                Description = "Spectacular penthouse with 360-degree city views, private elevator, and rooftop terrace. Ultimate urban living.",
                Price = 2500000,
                Address = "1 Central Park West",
                City = "New York",
                State = "New York",
                ZipCode = "10023",
                Country = "USA",
                Bedrooms = 3,
                Bathrooms = 3,
                SquareFeet = 2800,
                YearBuilt = 2022,
                PropertyType = PropertyType.Apartment,
                ListingType = ListingType.Sale,
                Status = PropertyStatus.Active,
                IsFeatured = true,
                MainImageUrl = "/images/properties/apt1.jpg",
                UserId = agent.Id,
                CategoryId = luxuryCat.Id
            },
            new Property
            {
                Title = "Modern Studio Apartment",
                Description = "Efficient studio in prime location. Open floor plan, modern finishes, building amenities included.",
                Price = 175000,
                Address = "555 Market Street",
                City = "San Francisco",
                State = "California",
                ZipCode = "94105",
                Country = "USA",
                Bedrooms = 1,
                Bathrooms = 1,
                SquareFeet = 550,
                YearBuilt = 2019,
                PropertyType = PropertyType.Apartment,
                ListingType = ListingType.Sale,
                Status = PropertyStatus.Active,
                IsFeatured = false,
                MainImageUrl = "/images/properties/apt2.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            new Property
            {
                Title = "Spacious 2BR Condo",
                Description = "Beautiful condo with modern kitchen, in-unit laundry, and stunning waterfront views.",
                Price = 320000,
                Address = "200 Bayshore Drive",
                City = "Miami",
                State = "Florida",
                ZipCode = "33131",
                Country = "USA",
                Bedrooms = 2,
                Bathrooms = 2,
                SquareFeet = 1200,
                YearBuilt = 2018,
                PropertyType = PropertyType.Condo,
                ListingType = ListingType.Sale,
                Status = PropertyStatus.Active,
                IsFeatured = true,
                MainImageUrl = "/images/properties/condo1.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            // Properties for Rent
            new Property
            {
                Title = "Luxury Beach Villa",
                Description = "Stunning beachfront villa with private pool, direct beach access, and breathtaking ocean views.",
                Price = 8500,
                Address = "100 Ocean Drive",
                City = "Miami Beach",
                State = "Florida",
                ZipCode = "33139",
                Country = "USA",
                Bedrooms = 5,
                Bathrooms = 4,
                SquareFeet = 4500,
                YearBuilt = 2021,
                PropertyType = PropertyType.Villa,
                ListingType = ListingType.Rent,
                Status = PropertyStatus.Active,
                IsFeatured = true,
                MainImageUrl = "/images/properties/villa1.jpg",
                UserId = agent.Id,
                CategoryId = vacationCat.Id
            },
            new Property
            {
                Title = "Downtown 1BR Apartment",
                Description = "Modern apartment in the heart of downtown. Walking distance to restaurants, shops, and transit.",
                Price = 1800,
                Address = "350 Main Street",
                City = "Chicago",
                State = "Illinois",
                ZipCode = "60601",
                Country = "USA",
                Bedrooms = 1,
                Bathrooms = 1,
                SquareFeet = 700,
                YearBuilt = 2017,
                PropertyType = PropertyType.Apartment,
                ListingType = ListingType.Rent,
                Status = PropertyStatus.Active,
                IsFeatured = false,
                MainImageUrl = "/images/properties/apt3.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            new Property
            {
                Title = "Family House with Pool",
                Description = "Spacious family home with swimming pool, large backyard, and 2-car garage. Great school district!",
                Price = 3200,
                Address = "890 Maple Lane",
                City = "Phoenix",
                State = "Arizona",
                ZipCode = "85001",
                Country = "USA",
                Bedrooms = 4,
                Bathrooms = 3,
                SquareFeet = 2600,
                YearBuilt = 2016,
                PropertyType = PropertyType.House,
                ListingType = ListingType.Rent,
                Status = PropertyStatus.Active,
                IsFeatured = true,
                MainImageUrl = "/images/properties/house4.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            new Property
            {
                Title = "Affordable Studio Near Campus",
                Description = "Perfect for students! Clean studio apartment near university, utilities included.",
                Price = 950,
                Address = "45 College Avenue",
                City = "Boston",
                State = "Massachusetts",
                ZipCode = "02115",
                Country = "USA",
                Bedrooms = 1,
                Bathrooms = 1,
                SquareFeet = 400,
                YearBuilt = 2010,
                PropertyType = PropertyType.Apartment,
                ListingType = ListingType.Rent,
                Status = PropertyStatus.Active,
                IsFeatured = false,
                MainImageUrl = "/images/properties/apt4.jpg",
                UserId = agent.Id,
                CategoryId = residentialCat.Id
            },
            // Commercial
            new Property
            {
                Title = "Prime Office Space Downtown",
                Description = "Professional office space in Class A building. Reception area, conference rooms, and parking.",
                Price = 5500,
                Address = "500 Financial District",
                City = "New York",
                State = "New York",
                ZipCode = "10005",
                Country = "USA",
                Bedrooms = 0,
                Bathrooms = 2,
                SquareFeet = 3000,
                YearBuilt = 2019,
                PropertyType = PropertyType.Commercial,
                ListingType = ListingType.Rent,
                Status = PropertyStatus.Active,
                IsFeatured = false,
                MainImageUrl = "/images/properties/office1.jpg",
                UserId = agent.Id,
                CategoryId = commercialCat.Id
            },
            new Property
            {
                Title = "Retail Space in Shopping Center",
                Description = "High-traffic retail location with large display windows. Ideal for boutique or restaurant.",
                Price = 4200,
                Address = "250 Shopping Plaza",
                City = "Los Angeles",
                State = "California",
                ZipCode = "90015",
                Country = "USA",
                Bedrooms = 0,
                Bathrooms = 1,
                SquareFeet = 1500,
                YearBuilt = 2020,
                PropertyType = PropertyType.Commercial,
                ListingType = ListingType.Rent,
                Status = PropertyStatus.Active,
                IsFeatured = false,
                MainImageUrl = "/images/properties/retail1.jpg",
                UserId = agent.Id,
                CategoryId = commercialCat.Id
            }
        };

        context.Properties.AddRange(demoProperties);
        await context.SaveChangesAsync();

        // Add amenities to properties
        var pool = context.Amenities.First(a => a.Name == "Swimming Pool");
        var gym = context.Amenities.First(a => a.Name == "Gym");
        var parking = context.Amenities.First(a => a.Name == "Parking");
        var garden = context.Amenities.First(a => a.Name == "Garden");
        var ac = context.Amenities.First(a => a.Name == "Air Conditioning");
        var security = context.Amenities.First(a => a.Name == "Security");
        var balcony = context.Amenities.First(a => a.Name == "Balcony");

        var props = context.Properties.ToList();
        var propertyAmenities = new List<PropertyAmenity>();

        // Add amenities to luxury properties
        foreach (var prop in props.Where(p => p.Price > 1000000 || p.PropertyType == PropertyType.Villa))
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = pool.Id });
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = gym.Id });
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = security.Id });
        }

        // Add common amenities to all properties
        foreach (var prop in props)
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = parking.Id });
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = ac.Id });
        }

        // Add garden to houses
        foreach (var prop in props.Where(p => p.PropertyType == PropertyType.House))
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = garden.Id });
        }

        // Add balcony to apartments/condos
        foreach (var prop in props.Where(p => p.PropertyType == PropertyType.Apartment || p.PropertyType == PropertyType.Condo))
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = balcony.Id });
        }

        context.PropertyAmenities.AddRange(propertyAmenities);
        await context.SaveChangesAsync();
    }
    
    // Seed default site settings
    if (!context.SiteSettings.Any())
    {
        context.SiteSettings.AddRange(
            new SiteSettings { Key = "site_name", Value = "Vivenda", Description = "Website name", Category = "General" },
            new SiteSettings { Key = "site_description", Value = "Find your dream property", Description = "Website description", Category = "General" },
            new SiteSettings { Key = "contact_email", Value = "contact@vivenda.com", Description = "Contact email address", Category = "General" },
            new SiteSettings { Key = "contact_phone", Value = "+1 234 567 890", Description = "Contact phone number", Category = "General" },
            new SiteSettings { Key = "contact_address", Value = "123 Main St, City, Country", Description = "Office address", Category = "General" },
            new SiteSettings { Key = "facebook_url", Value = "https://facebook.com", Description = "Facebook page URL", Category = "Social" },
            new SiteSettings { Key = "twitter_url", Value = "https://twitter.com", Description = "Twitter profile URL", Category = "Social" },
            new SiteSettings { Key = "instagram_url", Value = "https://instagram.com", Description = "Instagram profile URL", Category = "Social" },
            new SiteSettings { Key = "meta_keywords", Value = "vivenda, real estate, property, homes, apartments", Description = "SEO keywords", Category = "SEO" },
            new SiteSettings { Key = "google_analytics_id", Value = "", Description = "Google Analytics tracking ID", Category = "SEO" }
        );
        await context.SaveChangesAsync();
    }
}

app.Run();

